// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without warranty.

using System;
using System.Collections.Generic;
namespace GcnTools
{
    /// <summary>This is similar to a c/c++ #define.  The Assembler will first replace any #define names with their values.</summary>
    public struct Define
    {
        /// <summary>This is the name of the #define. When this name is found it will be replaced with the data field.</summary>
        public string name;
        /// <summary>If the define has any params these are listed here. The params will be replaced with
        /// the corresponding value when hit. Ex: #define car(NumOfSeats) My car has NumOfSeats seats.</summary>
        public string[] defParams;
        /// <summary>This is what the define will be replaced with.</summary>
        public string data;
    }

    /// <summary>GcnStmt represents a single asm instruction statement.</summary>
    public class Stmt
    {
        /// <summary>The statement text after removing comments and processing #defines.</summary>
        public string fullStmt;
        /// <summary>This is the index of the statement. </summary>
        public int GcnStmtId;
        /// <summary>The GCN ISA instruction/Operation for this operation.</summary>
        public InstInfo inst;
        /// <summary>This is all the text after the instruction itself.</summary>
        public string options;
        ///// <summary>Vars</summary>
        //public d variables;
        /// <summary>The word size of the OpCode. It can be 1, 2, or 0 for unknown.</summary>
        public int opSize; 
        /// <summary>The line number of the source file it was found it.</summary>
        public int lineNum;
        /// <summary>The number of statements before this one on the same line.</summary>
        public int lineDepth;
        /// <summary>Holds the 4 or 8 bytes of the OP binary microcode.</summary>
        public OpCode opCode;
        /// <summary>This is the location (in words) of where this statement is in the bin output.</summary>
        public int locInBin;
        /// <summary>Contains a list of vars with their located.</summary>
        public List<VariableUsageLoc> vars = new List<VariableUsageLoc>();
        /// <summary>A list of variables that are declared prior to this statement.</summary>
        public List<Variable> newVars = new List<Variable>();
        /// <summary>A list of variables that are freed after to this statement.</summary>
        public List<Variable> freeVars = new List<Variable>();
    }
    
}
