    // Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
    // Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
    // Source & Executable can be used in commercial applications and is provided AS-IS without warranty.

    using System;
    using System.Collections.Generic;

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

        /// <summary>Adds a GcnVar to the Vars collection and also makes sure that it is valid.</summary>
        public Variable Add(string name, bool isScaler, int byteSize, char type, Stmt stmt, int fixedRegNo = -1)
        {
            // make sure the reserved word is not in ISA_DATA.AsmReserveDWords, it will be added regardless
            if (Array.BinarySearch<string>(ISA_DATA.AsmReserveDWords, name) >= 0)
                log.Error("'{0}' cannot be used as a register name because it is a reserved word.", name);

            // make sure the variable name is not a common alias
            else if (ISA_DATA.sRegAliases.ContainsKey(name))
                log.Error("'{0}' cannot be used as a variable because it is a common register alias.", name);

            // make sure the name is not already added to the dictionary
            else if (varsByName.ContainsKey(name))
                log.Error("Variable '{0}' has already been declared.", name);

            // lets now add it to the var dictionary
            else
            {
                Variable var = new Variable(name, isScaler, byteSize, type, stmt, fixedRegNo);
                varsByName.Add(name, var);
                if (fixedRegNo >= 0)
                    ReserveSpecificRegister(name, isScaler, byteSize, stmt, fixedRegNo);
                return var;
            }

            return null;
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
            }
            else
                log.Error("Variable '{0}' cannot be freed because it does not exist.", name);

            if (gcnStmts.Count == 0)
                log.Error("There should be at least one statement before using 'free' with '{0}'", name);
            else
                gcnStmts[gcnStmts.Count - 1].freeVars.Add(nr);

            return nr;
        }

        //    //Removed: we should not really "rename" but rather free and create a new with the same reg
        ///// <summary>Renames a variable to a new name.</summary>
        //public void Rename(string from, String to, List<GcnStmt> gcnStmts, Log log)
        //{
        //    Variable freed = FreeVariable(from, gcnStmts, log);
        //    // add aaaa,bbbb,cccc
        //    Variable freed = FreeVariable(from, gcnStmts, log);
        //
        //
        //    Variable nr;
        //    if (TryGetValue(from, out nr))
        //    {
        //        nr.name = to;
        //        Remove(from);
        //        Add(to, nr);
        //
        //        if (nr.IsTerminated)
        //            log.Warning("Variable '{0}' should not be renamed because it has been freed already.", from);
        //    }
        //    else
        //        log.Error("Variable '{0}' cannot be renamed because it does not exist.", from);
        //}


        public void ProcessAutomaticFreeing(List<Stmt> gcnStmts)
        {
            foreach (KeyValuePair<string, Variable> v in varsByName)
            {
                if (v.Value.stmtTerminated == null)
                    if (v.Value.stmtsUsedIn.Count == 0)
                    {
                        log.Warning("Variable '{0}' was declared but never used.", v.Value.name);
                        // varsByName.Remove(v.Value.name); //todo: add me
                        continue;
                    }

                // if 'endStmt' is null after the initial load, point it to the very end
                if (v.Value.stmtTerminated == null)
                    v.Value.stmtTerminated = v.Value.stmtsUsedIn[v.Value.stmtsUsedIn.Count - 1];
            }
        }

        public void AssignRegNumbers( List<Stmt> gcnStmts)
        {
            foreach (Stmt stmt in gcnStmts)
            {
                // declare new vars
                if (UsingRegPools)
                    foreach (Variable v in stmt.newVars)
                        if (v.regNo < 0)
                            v.regNo = (v.isScaler ? sRegPool : vRegPool).ReserveRegs((v.size + 3) / 4);


                //todo: finish
                ////replace the reg numbers
                //if (stmt.vars.Count > 0)
                //{
                //    string newString = stmt.options.Substring(0, stmt.vars[0].startPos);
                //    int cur = newString.Length;
                //    for (int i = 0; i < stmt.vars.Count; i++)
                //    {
                //        VariableUsageLoc v = stmt.vars[i];
                //        newString += v.RegAsString + stmt.options.Substring(v.varLength, v.startPos - cur);
                //        cur = v.startPos + v.variable.name.Length;
                //    }
                //    stmt.options = newString;
                //}

                string newString = "";
                int cur = 0;
                foreach (VariableUsageLoc v in stmt.vars)
                {
                    newString += stmt.options.Substring(cur, v.startPos - cur) + v.RegAsString;
                    cur = v.startPos + v.variable.name.Length;
                }
                newString += stmt.options.Substring(cur, stmt.options.Length - cur);
                stmt.options = newString;


                // free the vars
                if (UsingRegPools)
                    foreach (Variable v in stmt.freeVars)
                        (v.isScaler ? sRegPool : vRegPool).FreeReg(v.regNo);

            }
        }
 

        /// <summary>Adds a GcnVar to the Vars collection and also makes sure that it is valid.</summary>
        private void ReserveSpecificRegister(string name, bool isScaler, int byteSize, Stmt stmt, int fixedRegNo = -1)
        {
            // Lets add mark that register as used in the pool (if it is in the pool)
            RegPool regPool = isScaler ? sRegPool : vRegPool;
            if (UsingRegPools)
                if (regPool.ReserveSpecificRegs(fixedRegNo, ((byteSize + 3) / 4)) < 0)
                    log.Error("The registers, {1}, used in '{0}' must was not found in the allowed register pool.", fixedRegNo, name);

            // lets calculate usage 
            RegUsageCalc regUsageCalc = isScaler ? sRegUsageCalc : vRegUsageCalc;
            if (regUsageCalc != null)
                regUsageCalc.AddToCalc(byteSize, stmt);
        }


        public int Count
        {
            get { return varsByName.Count; }
        }

        ///// <summary>
        ///// The Statement where the variable was terminated. This can be different then the last statement it was used on.
        ///// </summary>
        ///// <param name="stmt">The GcnStmt directly before where a free command.</param>
        //public void AddVariableUsage(GcnStmt stmt, Log log)
        //{
        //    VariableUsedOnStmt

        //    if (endStmt != null)
        //        stmtsUsedIn.Add(stmt);
        //    else
        //        log.Error("The variable," + name + " is being used but has already been marked terminated.");
        //}

        public void MarkVariableUsedInStmt(Stmt stmt, string name, int index, int startPos, int length)
        {
            Variable nr;
            if (!varsByName.TryGetValue(name, out nr))
                log.Error("Variable '{0}' is not defined.", name);
            else
            {
                if (nr.stmtTerminated == null)
                    nr.stmtsUsedIn.Add(stmt);
                else
                    log.Error("The variable," + name + " is being used but has already been marked terminated.");

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
        /// <summary>They data dataType for the register/variable. Usually either f=float, i=int, u=unsigned, or b=bool.</summary>
        public char type; // f, i, u, b
        ///// <summary>The statement number where life begins for a variable because of a declaration.</summary>
        //public GcnStmt startStmt;
        /// <summary>The Statement where the variable was terminated. This can be different then the last statement it was used on.</summary>
        public Stmt stmtTerminated;
        /// <summary>A list of statements where this variable is used.</summary>
        public List<Stmt> stmtsUsedIn = new List<Stmt>();

        public Variable()
        {
        }

        public Variable(string name, bool isScaler, int byteSize, char type, Stmt stmt, int fixedRegNo = -1)
        {
            this.name = name;
            this.isScaler = isScaler;
            this.size = byteSize;
            this.type = type;
            //this.startStmt = stmt;
            this.regNo = fixedRegNo; // -1 for not yet assigned
            //stmtsUsedIn.Add(stmt);
        }

        public bool IsTerminated
        {
            get { return (stmtTerminated != null); }
        }
    }
}
