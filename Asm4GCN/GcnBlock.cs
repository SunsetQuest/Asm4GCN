// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without any warranty.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace GcnTools
{
    /// <summary>
    /// Asm4GCN represents several GcnStmts and everything that would go along with 
    /// that including labels, predefinitions, register pools, and variables.
    /// </summary>
    public class GcnBlock
    {
        /// <summary>gcnStmts is a list of GcnStmt that hold information on GCN Asm instructions. GCN instructions get converted to gcnStmts. </summary>
        public List<GcnStmt> gcnStmts = new List<GcnStmt>();
        /// <summary>This is a class that holds label information.</summary>
        public Labels labels = new Labels();
        /// <summary>This is a list of defines that should be replaced if run across in the code.</summary>
        public List<Define> defines = new List<Define>();
        /// <summary>sRegPool is a list of available scaler registers for the entire session. If left null then RegPool is not used. If it is used it initializes with regs 0-127 unless a customized register pool is specified using #S_POOL or #V_POOL.</summary>
        public RegPool sRegPool;
        /// <summary>vRegPool is a list of available vector registers for the entire session. If left null then RegPool is not used. If it is used it initializes with regs 0-127 unless a customized register pool is specified using #S_POOL or #V_POOL.</summary>
        public RegPool vRegPool;
        /// <summary>This is a list of the current variable names with their currently assigned register.</summary>
        public Dictionary<string, AsmVar> vars = new Dictionary<string, AsmVar>();
        /// <summary>Keeps a usage count of each register size. It also remember the maximum.</summary>
        public RegUsageCalc sRegUsageCalc;
        /// <summary>Keeps a usage count of each register size. It also remember the maximum.</summary>
        public RegUsageCalc vRegUsageCalc;


        /// <summary>
        /// Compiles for both specifications and binary output. 
        /// </summary>
        public byte[] CompileFull(string[] srcLines, out List<GcnStmt> _gcnStmts, out List<RegUsage> sRegUsage, out List<RegUsage> vRegUsage, out int binSize, out string logOutput, out bool compileSuccessful)
        {
            Log log = new Log();
            sRegUsageCalc = new RegUsageCalc(true);
            vRegUsageCalc = new RegUsageCalc(true);
            sRegPool = new RegPool(log);
            vRegPool = new RegPool(log);

            byte[] bin = Compile(srcLines, out binSize, log);

            _gcnStmts = gcnStmts;

            sRegUsage = sRegUsageCalc.GetUsage();
            vRegUsage = vRegUsageCalc.GetUsage();
            logOutput = log.ToString();
            compileSuccessful = !log.hasErrors;
            return bin;
        }

        /// <summary>
        /// CompileWithoutSpecs is used in conjunction with CompileForBin. Its primary use is to get 
        /// the number of registers and size needs of the asm block.
        /// </summary>
        public void CompileForSpecs(string[] srcLines, out List<GcnStmt> _gcnStmts, out List<RegUsage> sRegUsage, 
            out List<RegUsage> vRegUsage, out int binSize, out string logOutput)
        {
            Log log = new Log();
            sRegUsageCalc = new RegUsageCalc(true);
            vRegUsageCalc =  new RegUsageCalc(true);

            Compile(srcLines, out binSize, log);
            _gcnStmts = gcnStmts;

            sRegUsage = sRegUsageCalc.GetUsage();
            vRegUsage = vRegUsageCalc.GetUsage();
            logOutput = log.ToString();
        }


        /// <summary>
        /// CompileForBin takes an asm block and creates a bin. #s_pool and #v_pool can optionally 
        /// be used to specify what register numbers are available.
        /// </summary>
        public byte[] CompileForBin(string[] srcLines, out List<GcnStmt> _gcnStmts, out int binSize, 
            out string logOutput, out bool compileSuccessful)
        {
            Log log = new Log();
            // only initialize s and vRegPool here if we are creating a bin
            sRegPool = new RegPool(log);
            vRegPool = new RegPool(log);

            byte[] bin = Compile(srcLines, out binSize, log);
            _gcnStmts = gcnStmts;

            logOutput = log.ToString();
            compileSuccessful = !log.hasErrors;
            return bin;
        }


        private byte[] Compile(string[] srcLines, out int binSize, Log log)
        {
            // log.WriteLine("Starting Lines: {0} ms", startedAt.ElapsedMilliseconds);
            bool inCommentMode = false;
            List<GcnStmt> needLabelFilled = new List<GcnStmt>();
            for (int line = 1; line < srcLines.Length + 1; line++)  // from lines 1 to Last
                ProcessLine(srcLines[line - 1], ref inCommentMode, needLabelFilled, line, log);

            // At this point we have to finish two things: 
            // (1) Find the "stmt.opSize" of each stmt and build min/max distance tables
            // (2) Fill in the "stmt.opCode" for any stmt that have a label.

            /////////// Fill in the OpSize for each statement ///////////
            FillOpSizeValue(log, needLabelFilled);


            /////////// Optional final Error checking ///////////
            int loc = 0;
            for (int i = 0; i < gcnStmts.Count; i++)
            {
                GcnStmt stmt = gcnStmts[i];
                if (stmt.opSize != stmt.opCode.Size)
                    log.Error("ERROR: (internal) Stmt {0} on line {1} has an opSize of {2} however HasValue is {3}", 
                        stmt.inst.name, stmt.srcLine, stmt.opSize, stmt.opCode.literal.HasValue);
                if (stmt.opSize == 0)
                    log.WriteLine("Stmt {0} had an opSizeOfZero");
                if (stmt.locInBin != loc)
                    log.WriteLine("stmt.locInBin ({0}) might not correct.", stmt.locInBin);
                if (stmt.GcnStmtId != i)
                    log.WriteLine("stmt.GcnStmtId ({0}) might not correct.", stmt.GcnStmtId);
                loc += stmt.opSize;
            }

            /////////// Lets write all the output ///////////
            if (log.hasErrors)
                log.WriteLine("One or more Error(s) in GCN Assembly");
            
            /////////// Get bin size ///////////
            binSize = 0;
            foreach (GcnStmt stmt in gcnStmts)
                binSize += stmt.opSize;

            /////////// Create bin ///////////
            // Write bin to output file
            byte[] bin = new byte[binSize];
            MemoryStream ms = new MemoryStream(bin);
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                foreach (GcnStmt op in gcnStmts)
                {
                    //writer.Write()
                    writer.Write(op.opCode.code);
                    if (op.opCode.literal.HasValue)
                        writer.Write((uint)op.opCode.literal);
                }
            }

            return bin;

            //if (doOutputInter) // code to for debugging
            //{  try { using (BinaryWriter writer = new BinaryWriter(File.Open(OutputInter, FileMode.Create))){
            //         foreach (GcnStmt s in gcnStmts) {
            //                writer.Write(s.inst.id);
            //                writer.Write(s.GcnStmtId);
            //                writer.Write(s.options);
            //                writer.Write(s.srcLine); }
            //            writer.Close(); } }
            //    catch (Exception e) { log.Error("Unable to write intermediate output file. {0}", e.Message); } }
        }

        private void FillOpSizeValue(Log log, List<GcnStmt> needLabelFilled)
        {
            /////////// (1) Find the "stmt.opSize" of each stmt and build min/max distance tables ///////////
            // log.WriteLine("Starting to fill stmt.opSizes: {0} ms", startedAt.ElapsedMilliseconds);
            int[] minDists = new int[gcnStmts.Count + 1];
            int[] maxDists = new int[gcnStmts.Count + 1];
            while (true)
            {
                int minDist = 0, maxDist = 0;

                // fill min/max distance lists
                List<GcnStmt> toResolove = new List<GcnStmt>();

                for (int i = 0; i < gcnStmts.Count; i++)
                {
                    if (gcnStmts[i].opSize == 0)
                    {
                        minDist += 4;
                        maxDist += 8;
                        toResolove.Add(gcnStmts[i]);
                    }
                    else
                    {
                        minDist += gcnStmts[i].opSize;
                        maxDist += gcnStmts[i].opSize;
                    }

                    minDists[i + 1] = minDist;
                    maxDists[i + 1] = maxDist;
                }

                if (toResolove.Count == 0)
                    break;

                foreach (GcnStmt stmt in toResolove)
                {
                    // Summary: Lets first try and make the instruction as large as possible by using the  
                    // largest jump length.  If its of size 4 then it always will always be 4.

                    // Log logMax = new Log(stmt.srcLine, true);
                    Log logMax = log; // lets just re-use log in here since we will not using it
                    logMax.paused = true;
                    logMax.lineNum = stmt.srcLine;

                    int minDistance = 0;
                    int maxDistance = 0;

                    string optionsMax = stmt.options = Regex.Replace(stmt.options, @"@[a-z_][0-9a-z_]+", delegate(Match m)
                    {
                        string labelName = m.Value.Remove(0, 1);
                        int label_loc = labels.GetNearestLabel(labelName, stmt.srcLine, log).stmtLoc;
                        int min = minDists[label_loc] - minDists[stmt.GcnStmtId] - 4;
                        int max = maxDists[label_loc] - maxDists[stmt.GcnStmtId] - 4;
                        minDistance += min;
                        maxDistance += max;
                        return max.ToString();
                    });

                    // If the jump length is known then lets finish this stmt now and short circuit to the next stmt.
                    if (minDistance == maxDistance)
                    {
                        stmt.opCode = Encoder.convertInstToBin(optionsMax, stmt.inst, logMax);
                        stmt.options = optionsMax;
                        stmt.opSize = stmt.opCode.Size;
                        needLabelFilled.Remove(stmt); // future: Lists can be slow as they fills up (maybe use dictionary)
                        logMax.CommitMessagesAndUnPause();
                        // Console.WriteLine("###Jump on LINE(" + stmt.srcLine + ")  Inst:" + stmt.inst.id + " Options:" + stmt.options);

                        // we should update the rest of the minDists[] and maxDists[]  here
                        if (stmt.opSize == 4)
                            for (int i = stmt.GcnStmtId; i < gcnStmts.Count; i++)
                                maxDists[i + 1] -= 4;
                        else
                            for (int i = stmt.GcnStmtId; i < gcnStmts.Count; i++)
                                minDists[i + 1] += 4;

                        // for (int i = 0; i < gcnStmts.Count; i++)
                        //    Console.WriteLine(minDists[i] + " " + maxDists[i]);

                        continue;
                    }


                    OpCode opCodeMax = Encoder.convertInstToBin(optionsMax, stmt.inst, logMax);
                    // Console.WriteLine("###Max LINE:" + stmt.srcLine + " Inst:" + stmt.inst.id);


                    //// if the largest jump length results in a 4 byte op, use 4 and short circuit to next statement ////
                    if (!opCodeMax.literal.HasValue && !logMax.hasErrors)
                    {
                        stmt.opSize = 4;

                        // we should update the rest of the minDists[] and maxDists[]  here
                        if (stmt.opSize == 4)
                            for (int i = stmt.GcnStmtId; i < gcnStmts.Count; i++)
                                maxDists[i + 1] -= 4;
                        else
                            for (int i = stmt.GcnStmtId; i < gcnStmts.Count; i++)
                                minDists[i + 1] += 4;

                        continue;
                    }


                    /////// lets try again but with the shortest distances possible. If still size 8 then it's 8. /////
                    string optionsMin = stmt.options = Regex.Replace(stmt.options, @"@[a-z_][0-9a-z_]+", delegate(Match m)
                    {
                        string labelName = m.Value.Remove(0, 1);
                        int label_loc = labels.GetNearestLabel(labelName, stmt.srcLine, log).stmtLoc;
                        int min = minDists[label_loc] - minDists[stmt.GcnStmtId] - 4;
                        return min.ToString();
                    });


                    Log logMin = new Log(stmt.srcLine, true);
                    OpCode opCodeMin = Encoder.convertInstToBin(optionsMin, stmt.inst, logMin);
                    // Console.WriteLine("###Min LINE:" + stmt.srcLine + " Inst:" + stmt.inst.id);


                    if (logMax.hasErrors && logMin.hasErrors) // if both versions have errors now then they always will
                        stmt.opSize = opCodeMin.Size;
                    else if (logMax.hasErrors)
                        stmt.opSize = opCodeMin.Size;
                    else if (logMin.hasErrors)
                        stmt.opSize = opCodeMax.Size;
                    else if (opCodeMin.literal.HasValue && opCodeMax.literal.HasValue)
                        stmt.opSize = 8;
                    else // if both min and max versions are different sizes then it could either. Try again later...
                        stmt.opSize = 0; //future: maybe just give it 8 and be done with it?
                }

                // we don't show any errors here, they are shown in doLast

                log.DeletePendingMessagesAndUnPause();
            }
            // copy the distance value from minDists to locInBin.
            for (int i = 0; i < gcnStmts.Count; i++)
                gcnStmts[i].locInBin = minDists[i];


            /////////// (2) Fill in the "stmt.opCode" for any stmt that have a label. ///////////
            // log.WriteLine("Starting to fill stmt.opSizes: {0} ms", startedAt.ElapsedMilliseconds);
            foreach (GcnStmt stmt in needLabelFilled)
            {
                // Lets Convert labels to hex values
                stmt.options = Regex.Replace(stmt.options, @"@[a-z_][0-9a-z_]+", delegate(Match m)
                {
                    string labelName = m.Value.Remove(0, 1);
                    int label_loc = labels.GetNearestLabel(labelName, stmt.srcLine, log).stmtLoc;
                    int distance = minDists[label_loc] - minDists[stmt.GcnStmtId] - 4;
                    return distance.ToString();
                });

                log.lineNum = stmt.srcLine;
                stmt.opCode = Encoder.convertInstToBin(stmt.options, stmt.inst, log);
                // Console.WriteLine("###Jump on LINE(" + stmt.srcLine + ")  Inst:" + stmt.inst.id + " Options:" + stmt.options);
            }
        }

        private void ProcessLine(string curLine, ref bool inCommentMode, List<GcnStmt> needLabelFilled, int line, Log log)
        {
            log.lineNum = line;
            // log.Error("{0}\t{1}", line, curLine.ToLower());

            // Skip empty lines
            if (string.IsNullOrWhiteSpace(curLine))
                return;

            // Cleanup - Remove starting / ending whitespace, replace tabs with spaces
            curLine = Regex.Replace(curLine.Trim().ToLower(), @"[ \t]+", " ");
            

            ///////// Strip out /* some_text */ comments  /////////
            curLine = Regex.Replace(curLine, @"/\*.*?\*/", "");
            if (inCommentMode)
            {
                if (Regex.IsMatch(curLine, @".*\*/"))
                {
                    inCommentMode = false;
                    curLine = Regex.Replace(curLine, @".*\*/", "");
                }
                else
                    return; // this entire line is in comment mode
            }
            else if (Regex.IsMatch(curLine, @"/\*.*")) // not inCommentMode AND opening comment found '\*'
            {
                inCommentMode = true;
                curLine = Regex.Replace(curLine, @"/\*.*", "");
            }

            // Split out a line (label_name + statements + comment)
            Match l = Regex.Match(curLine, @"^\s*(?:([a-z_][a-z0-9_]*):)?\s*(.*?)\s*;?\s*(?://.*)?$");
            curLine = l.Groups[2].Value;

            // Add any labels on the line
            if (l.Groups[1].Success)
                labels.AddLabel(l.Groups[1].Value, gcnStmts.Count, line, log);

            // Skip empty lines
            if (string.IsNullOrWhiteSpace(curLine))
                return;

            string cmd = Regex.Match(curLine, @"(?<=^[ \t]*\#)(define|ref|[vs]_pool)").Value;
            
            ///////// Process #S_POOL / #V_POOL  -->  sRegPool/vRegPool /////////
            if (cmd == "v_pool" || cmd == "s_pool") // Regex.IsMatch(curLine, @"^[ \t]*\#[vs]_pool[ \t]"))
            {
                // skip line if #S_POOL / #V_POOL
                if (sRegPool == null)
                    return; 
                
                // Show warning if there are already existing vars.
                if (vars.Count > 0)
                    log.Warning("#S_POOL / #V_POOL normally occur in the header area. Having this command in the "
                        + "body can be used to clear all variable names and pool reservations.");

                Match def = Regex.Match(curLine, @"^[ \t]*#(v|s)_pool[ \t]*(?:(?:[vs](\d+)|(\S+?))\s*,?\s*)+");
                if (!def.Groups[0].Success | !def.Groups[2].Success)
                {
                    log.Error("error processing #POOL statement in '{0}'", curLine);
                    return;
                }

                if (def.Groups[3].Success)
                    log.Error("unknown value '{0}' in POOL command (skipping)", def.Groups[3]);

                int[] available_Regs = new int[def.Groups[2].Captures.Count];
                for (int i = 0; i < available_Regs.Length; i++)
                    available_Regs[i] = Int32.Parse(def.Groups[2].Captures[i].Value);

                if (def.Groups[1].Value == "v")
                    vRegPool = new RegPool(available_Regs, log);
                else
                    sRegPool = new RegPool(available_Regs, log);

                return;  // we are done with this line.
            }

            ///////// Process new #Defines /////////
            // lets see if the first word is a #define
            if (cmd == "define") 
            {
                Match def = Regex.Match(curLine,
                    @"^\s*\#(?:define)\s*" +
                    @"(?<name>[a-z_][a-z0-9_]*)" +
                    @"(?:\(\s*" +
                    @"(?:(?<params>[a-z_][a-z0-9_]*)(?:\s*,\s*(?!\)))?)+" +
                    @"\s*\))?" +
                    @"\s*(?<main>.*?)\s*" +
                    @"(?://.*)?$");
                if (!def.Groups["name"].Success | !def.Groups["main"].Success)
                {
                    log.Error("unrecognized #define '{0}'", curLine);
                    return;
                }
                Define define = new Define { name = def.Groups["name"].Value };
                if (def.Groups["params"].Success)
                {
                    define.defParams = new string[def.Groups["params"].Captures.Count];
                    for (int i = 0; i < define.defParams.Length; i++)
                        define.defParams[i] = def.Groups["params"].Captures[i].Value;
                }

                if (def.Groups["main"].Success)
                    define.data = def.Groups["main"].Value; // you have NonSerializedAttribute idea what are you doing 676

                // if the label does not start and end with a "_" then throw a warning
                if (define.name[0] != '_' || define.name[define.name.Length-1] != '_')
                    log.Warning("To prevent unintended usage the #define '{0}' should begin and end with an underscore.", define.name);

                // check for duplicates
                foreach (var d in defines)
                {
                    int dParamCt = (d.defParams == null) ? 0 : d.defParams.Length;
                    int defineParamCt = (define.defParams == null) ? 0 : define.defParams.Length;
                    // if name and param count match then we have a duplicate
                    if (d.name == define.name &&
                        (d.defParams == null ? 0 : d.defParams.Length) == (define.defParams == null ? 0 : define.defParams.Length))
                    {
                        log.Warning("Duplicate #define found for '{0}', replacing with new value.", define.name);
                        defines.Remove(d);
                        break;
                    }
                }

                defines.Add(define);
                return;
            }

            /////////   Replace Defines   /////////
            // Lets check this line to see if there are any defines in it.
            foreach (Define define in defines)
            {
                if (define.defParams == null)
                    curLine = curLine.Replace(define.name, define.data); // curLine = Regex.Replace(curLine, define.name, define.data);
                else
                {
                    // Given: #define dog(op1) dog with op1 bones   THEN    dog(mouse) ---> rat+mouse   
                    // Then: A dog(six) will be happy.
                    // Results In:  a dog with six bones will be happy.

                    // Step 1) find "dog(six)" and replace "op1" in "dog with op1 bones" with "six"
                    // Step 2) replace "dog(six)" in row and replace with "dog with six bones"

                    string items = (define.defParams.Length - 1).ToString();
                    string searchFor = define.name + @"\((?<1>[^\r\n,\)\(]+?)(?:,(?<1>[^\r\n,\)\(]+?)){" + items + @"}\)";

                    curLine = Regex.Replace(curLine, searchFor, delegate(Match m)
                    {
                        int curParamNum = 0;
                        string val = define.data;
                        foreach (Capture item in m.Groups[1].Captures)
                            val = val.Replace(define.defParams[curParamNum++], item.Value); 
                        return val;
                    });
                }
            }

            // Lets split up multiple statements on one line
            string[] stmts = curLine.Split(new char[] { ';' } , StringSplitOptions.RemoveEmptyEntries);
            foreach (string stmt in stmts)
            {
                // Extract first word, and options
                char[] delimiterChars = { ',', ' ', '\t' };
                string[] commands = stmt.Split(delimiterChars, 2, StringSplitOptions.RemoveEmptyEntries);
                string firstWord = commands[0];
                string options = commands.Count()>1?commands[1].Trim():"";


                /////////  Process Register Reservations and Freeing /////////
                // New Format: [sv][1248][fiub][#] VarName    Free Format: free VarName
                // ren cat dog
                //future wish-list: allow arrays to be created like v4f[4] myFourFloats;
                if (firstWord == "free")
                {
                    string[] matches = options.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string name in matches)
                        FreeVariable(name, log);
                    continue;
                }

                if (firstWord == "ren")
                {
                    string[] matches = options.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 1; i < commands.Length; i += 2)
                        RenameVariable(matches[i-1], matches[i], log);
                    continue;
                }

                Match r1 = Regex.Match(firstWord, @"(?<1>[sv])(?<2>1|2|4|8|16)(?<3>[fiub])");
                if (r1.Success)
                {
                    char type = r1.Groups[1].Value[0];
                    int size = Int32.Parse(r1.Groups[2].Value);
                    char dataType = r1.Groups[3].Value[0];

                    
                    MatchCollection r2 = Regex.Matches(options, 
                        @"(?<4>[a-z_][a-z_0-9]*)" +
                        @"(?<8>[ \t]+(?<5>[sv])(?:(?<6>\d+)|(?:\[(?<6>\d+):(?<7>\d+)\])))?\s*(,|$)\s*");
                    if (r2.Count > 0) // there should be at least one match
                    {
                        foreach (Match m in r2)
                        {
                            GroupCollection g = m.Groups;
                            string name = g[4].Value;

                            if (g[8].Success)
                            {
                                int requestedReg = Int32.Parse(g[6].Value);
                                AddVar(name, (type == 's'), size, dataType, line, log, requestedReg);
                                // error checking
                                if (r1.Groups[7].Success)
                                    if ((int.Parse(g[7].Value) - requestedReg + 1) != (size / 4))
                                        log.Warning("The variable size({0}) does not match the size of {1}[#:#]", size, type);
                                if (r1.Groups[1].Value != g[5].Value)
                                    log.Warning("The variable type ({0}) does not match the register type {1}", type, g[5].Value);
                            }
                            else
                                AddVar(name, (type == 's'), size, dataType, line, log);
                        }
                    }
                    else
                        log.Error("unrecognized format. Format: (s|v)(1|2|4|8|16)(f|i|u|b) varName [s#|v#]");

                    continue; 
                }

                ///////// lookup instruction details /////////
                InstInfo inst;
                if (!ISA_DATA.isa_inst_dic.TryGetValue(firstWord, out inst))
                {
                    log.Error("'{0}' is an unrecognized instruction", firstWord);
                    continue;
                }

                ///////// Replace var names with registers making sure to exclude reserved words /////////
                options = Regex.Replace(options, @"(?<=,|\t|\ |^)(?![vs]\d+)([a-z_][a-z0-9_]*)(?=,|\t|\ |$)", delegate(Match r)
                {
                    string varName = r.Value;

                    // ignore reserved words
                    if (Array.BinarySearch<string>(ISA_DATA.AsmReserveDWords, varName) >= 0)
                        return varName;

                    // ignore common register aliases
                    if (ISA_DATA.sRegAliases.ContainsKey(varName))
                        return varName;

                    // Lets lookup the GcnVar to make sure it exists and also retrieve it.
                    AsmVar nr;
                    if (!vars.TryGetValue(varName, out nr))
                        log.Error("Variable '{0}' is not defined.", varName);

                    return nr.RegAsString;
                });

                ///////// If instruction has a destination label then lets save it for later. /////////
                GcnStmt mcInfo = new GcnStmt { inst = inst, options = options, GcnStmtId = gcnStmts.Count, srcLine = line };
                if (Regex.IsMatch(options, @"(?:^|\s|,)@[a-z_][0-9a-z_]+"))
                {
                    if (inst.minSize == inst.maxSize)
                        mcInfo.opSize = inst.minSize;
                    needLabelFilled.Add(mcInfo);
                }
                else
                {
                    mcInfo.opCode = Encoder.convertInstToBin(options, inst, log);
                    mcInfo.opSize = mcInfo.opCode.Size;
                }

                ///////// Lets add the stmt to the list /////////
                gcnStmts.Add(mcInfo);

            } // end Stmt
        }
        
        /// <summary>Adds a GcnVar to the Vars collection and also makes sure that it is valid.</summary>
        private void AddVar(string name, bool isScaler, int byteSize, char type, int line, Log log, int fixedRegNo = -1)
        {
            // make sure the reserved word is not in ISA_DATA.AsmReserveDWords, it will be added regardless
            if (Array.BinarySearch<string>(ISA_DATA.AsmReserveDWords, name) >= 0)
                log.Error("'{0}' cannot be used as a register name because it is a reserved word.", name);

            // make sure the variable name is not a common alias
            else if (ISA_DATA.sRegAliases.ContainsKey(name))
                log.Error("'{0}' cannot be used as a variable because it is a common register alias.", name);

            // make sure the name is not already added to the dictionary
            else if (vars.ContainsKey(name))
                log.Error("Variable '{0}' has already been declared.", name);

            else
            {
                // Lets add mark that register as used in the pool (if it is in the pool)
                int regNo = 0; 
                RegPool regPool = isScaler ? sRegPool : vRegPool;
                if (regPool != null)
                    if (fixedRegNo < 0)
                        regNo = regPool.ReserveRegs((byteSize + 3) / 4); // reserve reg and get reg number
                    else
                    {
                        regNo = fixedRegNo;
                        if (regPool.ReserveSpecificRegs(fixedRegNo, ((byteSize + 3) / 4)) < 0)
                            log.Error("The registers used in '#REF {1} {0}' must also be added to the (S/V)RegPool.", fixedRegNo, name);
                    }
                
                // lets now add it to the var dictionary
                vars.Add(name, new AsmVar { isScaler = isScaler, size = byteSize, type = type, name = name, regNo = regNo });

                // lets calculate usage 
                RegUsageCalc regUsageCalc = isScaler ? sRegUsageCalc : vRegUsageCalc;
                if (regUsageCalc != null)
                    regUsageCalc.AddToCalc(byteSize, line);
            }
        }

        /// <summary>Frees a variable by removing it from regCalculations and marking the register as available.</summary>
        private void FreeVariable(string name, Log log)
        {
            AsmVar nr;
            if (vars.TryGetValue(name, out nr))
            {
                vars.Remove(name);

                // lets calculate usage 
                RegUsageCalc regUsageCalc = nr.isScaler ? sRegUsageCalc : vRegUsageCalc;
                if (regUsageCalc != null)
                        regUsageCalc.RemoveFromCalc(nr.size);
                
                if (sRegPool != null)
                    (nr.isScaler ? sRegPool : vRegPool).FreeReg(nr.regNo);
            }
            else
                log.Error("Variable '{0}' cannot be freed because it does not exist.", name);
        }

        /// <summary>Renames a variable to a new name.</summary>
        private void RenameVariable(string from, String to, Log log)
        {
            AsmVar nr;
            if (vars.TryGetValue(from, out nr))
            {
                nr.name = to;
                vars.Remove(from);
                vars.Add(to, nr);
            }
            else
                log.Error("Variable '{0}' cannot be renamed because it does not exist.", from);
        }
    }
}