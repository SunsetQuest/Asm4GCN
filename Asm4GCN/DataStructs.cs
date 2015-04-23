// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without warranty.

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
    public class GcnStmt
    {
        /// <summary>This is the index of the statement. </summary>
        public int GcnStmtId;
        /// <summary>The GCN ISA instruction/Operation for this operation.</summary>
        public InstInfo inst;
        /// <summary>This is all the text after the instruction itself.</summary>
        public string options;
        /// <summary>The size of the OpCode. It can be 4, 8, or 0 for unknown. Sometimes opSize is known before the opCode.</summary>
        public int opSize; // 0 = unknown
        /// <summary>The line number of the source file it was found it.</summary>
        public int srcLine;
        /// <summary>Holds the 4 or 8 bytes of the OP binary microcode.</summary>
        public OpCode opCode;
        /// <summary>This is the location (in words) of where this statement is in the bin output.</summary>
        public int locInBin;
    }

    /// <summary>AsmVar is a variable that is automatically assigned to a register. An asmVar must be declared and freed.</summary>
    public struct AsmVar
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
        public char type; //f, i, u, b
        /// <summary>Returns the register as a string such as 's17' or 'v9'</summary>
        public string RegAsString
        {
            get { return (isScaler ? "s" : "v") + regNo; }
        }
    }
}
