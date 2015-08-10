// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without warranty.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GcnTools
{
    /// <summary>Stores information about where and how a variable is used in a statement.</summary>
    public struct VariableUsageLoc
    {
        /// <summary>Points to the Variable object this usage is references to.</summary>
        public Variable variable;

        /// <summary>If this variable uses an index (i.e. myvar[1]) then this holds that index. 
        /// The value is 0 if zero index is used or if no index is specified.</summary>
        public int indexOffset;

        /// <summary>The position in the original options string where the first character of the variable was found.
        /// (Each statement has a first_word+options.)</summary>
        public int startPos;

        /// <summary>the length of the var including index</summary>
        public int varLength;

        /// <summary>Returns the register as a string such as 's17' or 'v9'</summary>
        public string RegAsString
        {
            get { return (variable.isScaler ? "s" : "v") + (variable.regNo + indexOffset); }
        }
    }

    public class Variables
    {
        public Dictionary<string, Variable> varsByName = new Dictionary<string, Variable>();

        /// <summary>sRegPool is a list of available scaler registers for the entire session. If left null then RegPool is not used. If it is used it initializes with regs 0-127 unless a customized register pool is specified using #S/V_POOL.</summary>
        public RegPool sRegPool, vRegPool;

        /// <summary>Keeps a usage count of each register size. It also remember the maximum.</summary>
        public RegUsageCalc sRegUsageCalc, vRegUsageCalc;

        /// <summary>Compile Log</summary>
        Log log;

        public Variables(bool UseRegPool, bool CalcRegUsage, Log log)
        {
            this.log = log;

            if (UseRegPool)
            {
                sRegPool = new RegPool(log);
                vRegPool = new RegPool(log);
            }

            if (CalcRegUsage)
            {
                sRegUsageCalc = new RegUsageCalc(true);
                vRegUsageCalc = new RegUsageCalc(true);
            }
        }


        public bool UsingRegPools
        {
            get { return (sRegPool != null); }
        }

        public void ReloadRegPool(char type, int[] available_Regs)
        {
            if (type == 'v')
                vRegPool = new RegPool(available_Regs, log);
            else if (type == 's')
                sRegPool = new RegPool(available_Regs, log);
            else
                log.Error("'{0}' is an unknown type for a new RegPool (s or v only).", type);

        }

        //public bool TryGetVar(string name, out Variable variable)
        //{
        //    bool success = varsByName.TryGetValue(name, out variable);
        //    return success;
        //}

        /// <summary>Adds a GcnVar to the Vars collection while making sure it is valid.</summary>
        public void AddVariable(List<Variable> pendingNewVarsToAddToNextStmt, Stmt stmt, Log log, char type, int size, char dataType, string options)
        {
            MatchCollection r2 = Regex.Matches(options,
                @"(?<4>[a-z_][a-z_0-9]*)" +
                @"((?<8>\ +(?<5>[sv])(?:(?<6>\d+)|(?:\[(?<6>\d+):(?<7>\d+)\])))|" + // assign exact reg
                @"(?:\ +(?<9>[a-z_][a-z0-9_]*)(?:\[(?<10>\d+)\])?))?" + // Match to another Vars Reg#
                @"\s*(,|$)\s*");
            if (r2.Count <= 0) // there should be at least one match
            {
                log.Error("The new variable options could not be understood: " + options);
                return;
            }

            foreach (Match m in r2)
            {
                GroupCollection g = m.Groups;

                string name = g[4].Value;
                string varsRegToCopy = "";
                int varsRegToCopyIndex = 0;
                int requestedReg = -1;

                if (g[8].Success)
                {
                    requestedReg = Int32.Parse(g[6].Value);

                    // error checking
                    if (type != g[5].Value[0])
                        log.Warning("The variable type ({0}) does not match the register type {1}", type, g[5].Value);
                    if (g[7].Success)
                        if ((int.Parse(g[7].Value) - requestedReg + 1) != (size / 4))
                            log.Warning("The variable size({0}) does not match the size of {1}[#:#]", size, type);
                }
                else if (g[9].Success)
                {
                    varsRegToCopy = g[9].Value;
                    if (g[10].Success)
                        varsRegToCopyIndex = int.Parse(g[10].Value);

                    // make sure the name is not already added to the dictionary
                    Variable copySrc;
                    if (!varsByName.TryGetValue(varsRegToCopy, out copySrc))
                    {
                        log.Error("The past variable '{0}' cannot be found.", varsRegToCopy);
                        continue;
                    }

                    // if this is copying a reg from another variable and that variable is fixed then copy the reg now.
                    if (copySrc.isRegisterNumSpecifed)
                        requestedReg = copySrc.regNo + varsRegToCopyIndex;

                    if (type != (copySrc.isScaler ? 's' : 'v'))
                        log.Warning("'{0}' is type '{1}' however '{2}' is type '{3}'.", name, type, varsRegToCopy, copySrc.isScaler ? 's' : 'v');

                    if (requestedReg + ((size + 3) / 4) > copySrc.regNo + copySrc.RegsRequired)
                        log.Warning("The new variable '{0}' extends past the source variables last register.", name);
                }

                // make sure the reserved word is not in ISA_DATA.AsmReserveDWords, it will be added regardless
                if (Array.BinarySearch<string>(ISA_DATA.AsmReserveDWords, name) >= 0)
                {
                    log.Error("'{0}' cannot be used as a register name because it is a reserved word.", name);
                    continue;
                }

                // make sure the variable name is not a common alias
                if (ISA_DATA.sRegAliases.ContainsKey(name))
                {
                    log.Error("'{0}' cannot be used as a variable because it is a common register alias.", name);
                    continue;
                }

                // make sure the name is not already added to the dictionary
                if (varsByName.ContainsKey(name))
                {
                    log.Error("Variable '{0}' has already been declared.", name);
                    continue;
                }

                // lets now add it to the var dictionary
                Variable var = new Variable()
                {
                    name = name,
                    isScaler = (type == 's'),
                    size = size,
                    type = dataType,
                    regNo = requestedReg, // -1 for not yet assigned
                    variablesRegToCopy = varsRegToCopy,
                    variablesRegToCopyIndex = varsRegToCopyIndex,
                    isRegisterNumSpecifed = (requestedReg >= 0)
                };
                varsByName.Add(name, var);
                pendingNewVarsToAddToNextStmt.Add(var);

                // lets calculate usage 
                RegUsageCalc regUsageCalc = var.isScaler ? sRegUsageCalc : vRegUsageCalc;
                if (regUsageCalc != null)
                    regUsageCalc.AddToCalc(var.size, stmt);

            }
        }

        /// <summary>Frees a variable by removing it from regCalculations and marking the register as available.</summary>
        public Variable FreeVariable(string name, List<Stmt> gcnStmts)
        {
            if (gcnStmts.Count == 0)
            {
                log.Error("There should be at least one statement before using 'free' with '{0}'", name);
                return null;
            }

            Stmt lastGcnStmt = gcnStmts[gcnStmts.Count - 1];

            Variable nr;
            if (varsByName.TryGetValue(name, out nr))
            {
                nr.stmtTerminated = lastGcnStmt;
                gcnStmts[gcnStmts.Count - 1].freeVars.Add(nr);
            }
            else
                log.Error("Variable '{0}' cannot be freed because it does not exist.", name);

            return nr;
        }


        public void ProcessAutomaticFreeing(List<Stmt> gcnStmts)
        {
            //future: maybe use stmtsUsedIn for performance
            for (int i = gcnStmts.Count - 1; i >= 0; i--)
                foreach (VariableUsageLoc v in gcnStmts[i].vars)
                    if (!v.variable.IsTerminated)
                    {
                        v.variable.stmtTerminated = gcnStmts[i];
                        gcnStmts[i].freeVars.Add(v.variable);
                    }

            foreach (KeyValuePair<string, Variable> v in varsByName)
            {
                // if 'endStmt' is null after the initial load, point it to the very end
                if (v.Value.stmtTerminated == null)
                {
                    if (v.Value.stmtsUsedIn.Count > 0) //protect 
                        v.Value.stmtTerminated = v.Value.stmtsUsedIn[v.Value.stmtsUsedIn.Count - 1];
                    else
                        log.Warning("Variable '{0}' was declared but never used.", v.Value.name);
                }

                if (v.Value.stmtsUsedIn.Count == 1)
                    if (!(v.Value.isRegisterNumSpecifed | v.Value.isRegisterFromAnotherVar)) // Pre-filled regs might be used once.
                        log.Warning("'{0}' is only used once. Usually Variables are used 2 or times.", v.Value.name);
            }
        }


        public void AssignRegNumbers(List<Stmt> gcnStmts)
        {
            foreach (Stmt stmt in gcnStmts)
            {
                // note: When using the 'free' keyword, variables are freed before the previous statement.
                // Also, vars are freed before declarations so registers can be reused in the same instruction. 
                log.lineNum = stmt.lineNum;

                ////////////////////// free the vars //////////////////////
                if (UsingRegPools)
                    foreach (Variable v in stmt.freeVars)
                        if (v.stmtTerminated != v.stmtDeclared)
                            (v.isScaler ? sRegPool : vRegPool).FreeReg(v.regNo);
                        else
                            log.Warning("Variable, {0}, was declared and last used in the same statement.", v.name);

                ////////////////////// declare new vars //////////////////////
                // we first add variables that specify a register or variables
                foreach (Variable v in stmt.newVars)
                {
                    if (v.isRegisterNumSpecifed && !v.isRegisterFromAnotherVar)
                    {
                        // Adds a GcnVar to the Vars collection and also makes sure that it is valid.
                        // Lets add mark that register as used in the pool (if it is in the pool)
                        RegPool regPool = v.isScaler ? sRegPool : vRegPool;
                        if (UsingRegPools)
                            if (regPool.ReserveSpecificRegs(v.regNo, v.RegsRequired) < 0)
                                log.Error("The registers, {1}, used in '{0}' must was not found in the allowed register pool.", v.regNo, v.name);

                        // lets calculate usage 
                        RegUsageCalc regUsageCalc = v.isScaler ? sRegUsageCalc : vRegUsageCalc;
                        if (regUsageCalc != null)
                            regUsageCalc.AddToCalc(v.size, stmt);
                    }

                    // process declarations with variable as register reference 
                    else if (v.isRegisterFromAnotherVar)
                    {
                        Variable lu;
                        if (!varsByName.TryGetValue(v.variablesRegToCopy, out lu))
                            log.Error("Variable, '{0}', could not be found.", v.variablesRegToCopy);
                        else if (lu.stmtTerminated == null)
                            log.Error("Variable, '{0}', can not use the same register as '{1}' because its still in use.", v.name, lu.name);
                        else
                        {
                            int regNum = lu.regNo + v.variablesRegToCopyIndex;
                            RegPool regPool = v.isScaler ? sRegPool : vRegPool;
                            if (UsingRegPools)
                                v.regNo = regPool.ReserveSpecificRegs(regNum, v.size);
                        }
                    }
                }

                foreach (Variable v in stmt.newVars)
                {
                    if (!v.isRegisterNumSpecifed) // exclude Registers with specified register numbers
                        v.regNo = (v.isScaler ? sRegPool : vRegPool).ReserveRegs(v.RegsRequired, v.RegsRequired);
                }

                // Replace the variables with register numbers.
                //if (stmt.vars.Count > 0)
                for (int i = stmt.vars.Count - 1; i >= 0; i--)
                {
                    VariableUsageLoc v = stmt.vars[i];
                    stmt.options = stmt.options.Remove(v.startPos, v.varLength).Insert(v.startPos, v.RegAsString);
                }
            }
        }

    
 
        /// <summary>The number of variables.</summary>
        public int Count
        {
            get { return varsByName.Count; }
        }


        public void MarkVariableUsedInStmt(Stmt stmt, string name, int index, int startPos, int length)
        {
            Variable nr;
            if (!varsByName.TryGetValue(name, out nr))
                log.Error("Variable, '{0}', is not defined.", name);
            else
            {
                if (nr.stmtTerminated == null)
                    nr.stmtsUsedIn.Add(stmt);
                else
                    log.Error("Variable, " + name + ", is being used but has already been marked terminated.");

                stmt.vars.Add(new VariableUsageLoc()
                {
                    variable = nr,
                    indexOffset = index,
                    startPos = startPos,
                    varLength = length
                });
            }

        }
    }


    /// <summary>AsmVar is a variable that is automatically assigned to a register.</summary>
    public class Variable
    {
        /// <summary>The friendly variable name for 1 or more registers.</summary>
        public string name;
        /// <summary>The beginning actual register number where this variable is assigned.</summary>
        public int regNo;
        /// <summary>True if a Scaler register, False if Vector Register.</summary>
        public bool isScaler;
        /// <summary>The actual size of the register in bytes. Usually either 1, 2, 4, or 8.</summary>
        public int size;
        /// <summary>Gets the number of DWORD regs required to fit this register.</summary>
        public int RegsRequired { get { return ((size + 3) / 4); } }
        /// <summary>They data dataType for the register/variable. Usually either f=float, i=int, u=unsigned, or b=bool.</summary>
        public char type; // f, i, u, b
        /// <summary>True when, upon decoration, the exact register number is specified or it copies the register number from a variable where its reg is specified.</summary>
        public bool isRegisterNumSpecifed;
        /// <summary>True when the register number is specified when declared.</summary>
        public bool isRegisterFromAnotherVar { get { return !String.IsNullOrEmpty(variablesRegToCopy); } }
        /// <summary>The name of the variable that the register number is based off of.</summary>
        public string variablesRegToCopy;
        /// <summary>The indexing of the variable that the register number is based off of.</summary>
        public int variablesRegToCopyIndex;
        /// <summary>The statement number where the variable was declared.</summary>
        public Stmt stmtDeclared;
        /// <summary>The Statement where the variable was terminated. This can be different then the last statement it was used on.</summary>
        public Stmt stmtTerminated;
        /// <summary>A list of statements where this variable is used.</summary>
        public List<Stmt> stmtsUsedIn = new List<Stmt>();

        public Variable()
        {
        }

        public Variable(string name, bool isScaler, int byteSize, char type, int fixedRegNo = -1)
        {
            this.name = name;
            this.isScaler = isScaler;
            this.size = byteSize;
            this.type = type;
            this.regNo = fixedRegNo; // -1 for not yet assigned
            this.isRegisterNumSpecifed = (fixedRegNo >= 0);

        }

        public Variable(string name, bool isScaler, int byteSize, char type, string variablesRegToCopy, int variablesRegToCopyIndex)
        {
            this.name = name;
            this.isScaler = isScaler;
            this.size = byteSize;
            this.type = type;
            this.regNo = -1; // -1 for not yet assigned
            this.variablesRegToCopy = variablesRegToCopy;
            this.variablesRegToCopyIndex = variablesRegToCopyIndex;
            this.isRegisterNumSpecifed = false;
        }

        public bool IsTerminated
        {
            get { return (stmtTerminated != null); }
        }
    }
}
