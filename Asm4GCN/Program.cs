// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without any warranty.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GcnTools
{
    class Program
    {
        static void Main(string[] args)
        {
            // args = "TestInput.txt output.bin /OutputUsage".Split(' '); // <---- uncomment to use testinput.txt
            // args = "TestInput.txt output.bin /showstmts".Split(' '); // <---- uncomment to use testinput.txt

            Log log = new Log(0);

            Stopwatch startedAt = new Stopwatch(); startedAt.Start();

            if (args.Count() < 2)
            {
                // process asm             -> finished bin
                // process asm,RegPool     -> finished bin
                // process asm             -> partial + binSize,vRegCt,sRegCt
                // process partial+RegPool -> finished bin
                Console.WriteLine(
                    //01234567890123456789012345678901234567890123456789012345678901234567890123456789
                    "Usage: Asm4GCN.exe <input> <output> /OutputUsage /ShowStmts\n\n" +
                    " <input>      This is an Asm text file.\n\n" +
                    //" /OutputBin   This is the finished GCN asm bin file.\n" +
                    " /OutputUsage Outputs the vector\\scalar register counts and the\n" +
                    "              binary size. When in this mode a usage file is\n" +
                    "              created and not a bin.\n\n" +
                    " /ShowStmts   Shows the statements in a friendly to read format.\n\n" +
                    " A example usage plan using the 'outputusage' option:\n" +
                    "  1) Extract Asm block from c/c++/c# code into myAsm.txt.\n" +
                    "  2) Get the register and size requirements for the block..\n" +
                    "     example: Asm4GCN.exe myAsm.txt /OutputUsage AsmBlockReq.txt\n" +
                    "  3) Using the requirements, create dummy code with the same\n" +
                    "     scalar\\vector register needs and bin size then note what\n" +
                    "     actual registers are used.\n" +
                    "  4) Now that the registers are known, the binary file can be \n" +
                    "     finished.\n" +
                    "     example: Asm4GCN.exe tempStage.int /OutputBin myBin.bin\n" +
                    "  5) Now that we have the asm block in binary form we need to\n" +
                    "     locate the dummy code and replace it with the newly\n"+
                    "     generated binaries.\n");
                return;
            }

            //string OutputBin = "", OutputUsage = "";
            string outputFilename = args[1];
            bool doShowStmts = false;
            bool inUsageMode = false;
            //bool doOutputBin = false;
            for (int i = 2; i < args.Count(); i++)
            {
                if (args[i].ToLower() == "/outputusage")
                    inUsageMode = true;
                else if (args[i].ToLower() == "/showstmts")
                    doShowStmts = true;
                else
                {
                    Console.WriteLine("Unknown command line option: " + args[i]);
                    return;
                }
            }


            /////////////////// Read the input file ///////////////////
            string[] lines = new string[0];
            try
            {
                lines = System.IO.File.ReadAllLines(args[0]);
            }
            catch (Exception e)
            {
                log.Error("Unable to open input file. Details: {0}", e.Message);
                return;
            }

            GcnBlock Asm4GCN = new GcnBlock();
            
            List<GcnStmt> gcnStmts;
            int binSize = 0;

            if (inUsageMode)
            {
                List<RegUsage> sRegUsage, vRegUsage;
                string logOutput;
                Asm4GCN.CompileForSpecs(lines, out gcnStmts, out sRegUsage, out vRegUsage, out binSize, out logOutput);
                log.Append(logOutput);

                // Write bin or halfway point to output file
                int stats_4ByteInstCt = 0;
                int stats_8ByteInstCt = 0;
                foreach (GcnStmt op in gcnStmts)
                    if (op.opCode.literal.HasValue) stats_8ByteInstCt++; else stats_4ByteInstCt++;

                if (true) // lets just always show this for now
                {

                    log.WriteLine("4-Byte_Inst_Count:{0}  8-Byte_Inst_Count:{1}  Total:{2} bytes",
                        stats_4ByteInstCt, stats_8ByteInstCt, stats_4ByteInstCt * 4 + stats_8ByteInstCt * 8);

                    //log.WriteLine("Displaying all {0} Scaler sizes:", sRegUsage.Count);
                    for (int i = 0; i < sRegUsage.Count; i++)
                        log.WriteLine("{1} Scaler reg(s) of size {0} needed", sRegUsage[i].regSize, sRegUsage[i].timesUsed);
                    //log.WriteLine("Line # on last S Reg increase: {0}", sRegUsageCalc.lineOnLastPoolIncrease);

                    //log.WriteLine("Displaying all {0} Vector sizes:", vRegUsage.Count);
                    for (int i = 0; i < vRegUsage.Count; i++)
                        log.WriteLine("{1} Vector reg(s) of size {0} needed", vRegUsage[i].regSize, vRegUsage[i].timesUsed);
                    //log.WriteLine("Line # on last S Reg increase: {0}", sRegUsageCalc.lineOnLastPoolIncrease);

                }

                // Lets save the regUsage to a file
                try
                {
                    //MemoryStream stream = new MemoryStream();
                    //BinaryWriter writer2 = new BinaryWriter(stream);
                    using (BinaryWriter writer = new BinaryWriter(File.Open(outputFilename, FileMode.Create)))
                    {
                        writer.Write(stats_4ByteInstCt);
                        writer.Write(stats_8ByteInstCt);
                        writer.Write(sRegUsage.Count);
                        for (int i = 0; i < sRegUsage.Count; i++)
                        {
                            writer.Write(sRegUsage[i].regSize);
                            writer.Write(sRegUsage[i].timesUsed);
                        }

                        writer.Write(vRegUsage.Count);
                        for (int i = 0; i < vRegUsage.Count; i++)
                        {
                            writer.Write(vRegUsage[i].regSize);
                            writer.Write(vRegUsage[i].timesUsed);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error("Unable to write binary output file. {0}", e.Message);
                }
            }
            else // binary creating mode (not Usage-Mode)
            {
                string logOutput;
                bool success;
                byte[] bin = Asm4GCN.CompileForBin(lines, out gcnStmts, out binSize, out logOutput, out success);
                log.Append(logOutput);
                if (success)
                {
                    try
                    {
                        // Write bin to output file
                        File.WriteAllBytes(outputFilename, bin);

                        //// Display the output (for debugging only)
                        //using (BinaryWriter writer = new BinaryWriter(File.Open(OutputBin, FileMode.Create)))
                        //{
                        //    foreach (GcnStmt op in gcnStmts)
                        //    {
                        //        writer.Write(op.opCode.code);
                        //        if (op.opCode.literal.HasValue)
                        //            writer.Write((uint)op.opCode.literal);
                        //    }
                        //}
                    }
                    catch (Exception e)
                    {
                        log.Error("Unable to write binary output file. {0}", e.Message);
                    }
                }
                else
                    log.Error("{0} not generated because of errors.", outputFilename);
            }

            log.WriteLine("Time taken: {0} ms", startedAt.ElapsedMilliseconds);

            /////////// Display the opcode. ///////////
            //log.WriteLine("Starting Display the opcode: {0} ms", startedAt.ElapsedMilliseconds);
            if (doShowStmts)
            {
                log.WriteLine("ID |Line|Sz|Loc| OpCode |Params post Processing|       Source Line");
                for (int i = 0; i < gcnStmts.Count; i++)
                {
                    GcnStmt stmt = gcnStmts[i];
                    string sBefore = lines[stmt.srcLine - 1];
                    string sAfter = stmt.options;
                    sBefore = sBefore.Substring(0, Math.Min(30, sBefore.Length));
                    sAfter = sAfter.Substring(0, Math.Min(21, sAfter.Length));
                    sBefore = Regex.Replace(sBefore, @"\t|\n|\r", " ");
                    log.WriteLine("{0,3}|{1,4}|{2,2}|{3,3}|{4:X}| {5,-21}| {6,-30}", stmt.GcnStmtId, stmt.srcLine, stmt.opSize, stmt.locInBin, stmt.opCode.code, sAfter, sBefore);//  stmt.inst.id + " Options:" + stmt.options);
                }
            }


            //Console.WriteLine("Output file size in words (4 bytes): {0} ", new FileInfo(args[1]).Length);
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
