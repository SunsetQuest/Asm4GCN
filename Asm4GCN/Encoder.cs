// This file contains a the static GcnParcer class. The GcnParcer static class contains methods for each GCN 
// encoding format whose job is to convert a single statement line into its OpCode binary format. This is the
// core of the GCN assembler.
//
// Much credit goes to Daniel Bali for his GCN assembler posted at GitHub. I never built an assembler before 
// and Daniel’s project gave me the basic ideas on where to start. To some degree the outline of the code below is
// from Daniel's project.
// source: Daniel Bali, Dec 2013 https://github.com/ukasz/amd-gcn-isa-assembler/tree/master/src 
//
// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without any warranty.
using System;
using System.Text.RegularExpressions;

namespace GcnTools
{
    public static class Encoder
    {
        /// <summary>
        /// convertInstToBin() converts a GCN Asm statement into binary code. It also checks for problems.
        /// </summary>
        /// <param name="options">Everything after the instruction name itself. (less comments)</param>
        /// <param name="inst">The InstInfo information for the instruction found.</param>
        /// <param name="log">Location to write output.</param>
        /// <returns>Returns the 4 or 8 byte binary code in OpCode format.</returns>
        public static OpCode convertInstToBin(string options, InstInfo inst, Log log)
        {
            Log log1 = new Log(log.lineNum, true);
            OpCode opcode = new OpCode();
            switch (inst.encoding)
            {
                case ISA_Enc.DS: opcode = encodeDS(inst, options, log1); break;
                case ISA_Enc.EXP: opcode = encodeEXP(inst, options, log1);  break;
                case ISA_Enc.FLAT: opcode = encodeFLAT(inst, options, log1); break;
                case ISA_Enc.MIMG: opcode = encodeMIMG(inst, options, log1); break;
                case ISA_Enc.MTBUF: opcode = encodeMTBUF(inst, options, log1); break;
                case ISA_Enc.MUBUF: opcode = encodeMUBUF(inst, options, log1); break;
                case ISA_Enc.SMRD: opcode = encodeSMRD(inst, options, log1); break;
                case ISA_Enc.SOP1: opcode = encodeSOP1(inst, options, log1); break;
                case ISA_Enc.SOP2: opcode = encodeSOP2(inst, options, log1); break;
                case ISA_Enc.SOPC: opcode = encodeSOPC(inst, options, log1); break;
                case ISA_Enc.SOPK: opcode = encodeSOPK(inst, options, log1); break;
                case ISA_Enc.SOPP: opcode = encodeSOPP(inst, options, log1); break;
                case ISA_Enc.VINTRP: opcode = encodeVINTRP(inst, options, log1); break; 
                case ISA_Enc.VOP1: opcode = encodeVOP1(inst, options, log1); break;
                case ISA_Enc.VOP2: opcode = encodeVOP2(inst, options, log1); break;
                case ISA_Enc.VOP3a0: opcode = encodeVOP3a0(inst, options, log1); break;
                case ISA_Enc.VOP3a1: opcode = encodeVOP3a1(inst, options, log1); break;
                case ISA_Enc.VOP3a2: opcode = encodeVOP3a2(inst, options, log1); break;
                case ISA_Enc.VOP3a3: opcode = encodeVOP3a3(inst, options, log1); break;
                case ISA_Enc.VOP3b2: opcode = encodeVOP3b2(inst, options, log1); break;
                case ISA_Enc.VOP3b3: opcode = encodeVOP3b3(inst, options, log1); break;
                case ISA_Enc.VOP3bC: opcode = encodeVOP3bC(inst, options, log1); break;
                case ISA_Enc.VOPC: opcode = encodeVOPC(inst, options, log1); break;
                default: log1.Warning("unknown instruction or symbol '{0}'", inst.name); break;
            }

            // if we have some errors above and there is an alternate format then lets try that...
            if (log1.hasErrors && (inst.vop3Version > 0))
            {
                inst = ISA_DATA.ISA_Insts[inst.vop3Version];

                Log log2 = new Log(log.lineNum, true);
                switch (inst.encoding)
                {
                    case ISA_Enc.VOP3a0: opcode = encodeVOP3a0(inst, options, log2); break;
                    case ISA_Enc.VOP3a1: opcode = encodeVOP3a1(inst, options, log2); break;
                    case ISA_Enc.VOP3a2: opcode = encodeVOP3a2(inst, options, log2); break;
                    case ISA_Enc.VOP3a3: opcode = encodeVOP3a3(inst, options, log2); break;
                    case ISA_Enc.VOP3b2: opcode = encodeVOP3b2(inst, options, log2); break;
                    case ISA_Enc.VOP3b3: opcode = encodeVOP3b3(inst, options, log2); break;
                    case ISA_Enc.VOP3bC: opcode = encodeVOP3bC(inst, options, log2); break;
                    default: log2.Warning("unknown instruction or symbol '{0}'", inst.name); break;
                }

                if (log2.hasErrors)
                {
                    log.JoinLog(log1);
                    log.JoinLog(log2);
                }
                else
                    log.JoinLog(log2);
            }
            else
                log.JoinLog(log1);

            return opcode;
        }

        /// <summary>
        /// DS encoding format 
        /// |ENCODE(6)-26|--OP(8)-18--|GDS(1)-17|reserved(1)-16|OFFSET1(8)-8|OFFSET0(8)-0|
        /// |VDST(8)-56|DATA1(8)-48|DATA0(8)-40|ADDR(8)-32|
        /// </summary>
        private static OpCode encodeDS(InstInfo instr, string options, Log log)
        {
	        string[] args = Regex.Split(options, @"\s*[,\s]\s*");
	        
            if (args.Length < 4)
               log.Error("number of passed operands is too low");

	        // Setup arguments
	        string vdst_str	= args[0];
	        string addr_str	= args[1];
	        string data0_str= args[2];
	        string data1_str= args[3];
	       
            OpCode op_code = new OpCode{code = instr.opCode};

            uint vdst_val = ParseOperand.parseOnlyVGPR(vdst_str, 1, log);
            uint addr_op = ParseOperand.parseOnlyVGPR(addr_str, 2, log);
            uint data0_op = ParseOperand.parseOnlyVGPR(data0_str, 3, log);
            uint data1_op = ParseOperand.parseOnlyVGPR(data1_str, 4, log);

	        // Parse optional parameters
	        for (int i = 4; i < args.Length; i++)
	        {
		        if (args[i]== "gds")
			        op_code.code |= (1 << 17);
		        else if (args[i].StartsWith("offset"))
		        {
                    Match m = Regex.Match(args[i], @"offset(0|1):([0-9a-fxo]+)$");
                    uint val = 0;
                    if (m.Success)
                        val = ParseOperand.parseUnSignedNumber(m.Groups[2].Value, 16, i, log); //hack: sometimes this is 8 bits(not 16)
                    else
                        log.Error("incorrect offset format - example OFFSET0:123");

                    op_code.code |= val << (m.Groups[1].Value == "0" ? 0 : 8);
		        }
	        }

            op_code.literal = addr_op | data0_op << 8 | data1_op << 16 | vdst_val << 24;

            return op_code;
        }
        
        /// <summary>
        /// EXP encoding format 
        /// |ENCODING(6)-26|reserved(13)-13|VM(1)-12|DONE(1)-11|COMPR(1)-10|TGT(6)-4|EN(int,4)-0|
        /// |VSRC3(8)-56|VSRC2(8)-48|VSRC1(8)-40|VSRC0(8)-32|
        /// </summary>
        private static OpCode encodeEXP(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            // EN	    0	3	int	4	This bitmask determines which VSRC registers export data. When COMPR is 0,  VSRC0 only exports data when en0 is set to 1; VSRC1 when en1, VSRC2 when en2, and VSRC3 when en3. When COMPR is 1,  VSRC0 contains two 16-bit data and only exports when en0 is set to 1; VSRC1 only exports when en2 is set to 1; en1 and en3 are ignored when COMPR is 1.
            // TGT	    4	9	enum	6	Export target based on the enumeration below.  0–7: EXP_MRT = Output to color MRT 0. Increment from here for additional MRTs. There are EXP_NUM_MRT MRTs in total., 8: EXP_MRTZ = Output to Z., 9: EXP_NULL = Output to NULL.  12–15: EXP_POS = Output to position 0. Increment from here for additional positions. There are EXP_NUM_POS positions in total. , 32–63: EXP_PARAM = Output to parameter 0. Increment from here for additional parameters. There are EXP_NUM_PARAM parameters in total.]
            // COMPR	10	10	enum	1	Boolean. If true, data is exported in float16 format; if false, data is 32 bit.
            // DONE	    11	11	enum	1	If set, this is the last export of a given dataType. If this is set for a color export (PS only), then the valid mask must be present in the EXEC register.
            // VM	    12	12	enum	1   Mask contains valid-mask when set; otherwise, mask is just write-mask. Used only for pixel(mrt) exports.
            // resvd	13	25		    13	Reserved.
            // ENCODING	26	31	enum	6	Must be 1 1 1 1 1 0.
            // VSRC0	32	39	enum	8	VGPR of the first data to export.
            // VSRC1	40	47	enum	8	VGPR of the second data to export.
            // VSRC2	48	55	enum	8	VGPR of the third data to export.
            // VSRC3	56	63	enum	8	VGPR of the fourth data to export.
            log.Warning("EXP not implemented.");
            return new OpCode { code = instr.opCode };
        }

        /// <summary>
        /// FLAT encoding format 
        /// |ENCODING(6)-26|reserved(1)-25|OP(7)-18|SLC(1)-17|GLC(1)-16|reserved(16)-0|
        /// |VDST(8)-56|TFE(1)-55|reserved(7)-48|DATA(8)-40|ADDR(8)-32|
        /// </summary>
        private static OpCode encodeFLAT(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            // OFFSET	0	7	int	    8	Unsigned eight-bit Dword offset to the address specified in SBASE.
            // IMM	    8	8	enum	1	Boolean.[IMM = 0:Specifies an SGPR address that supplies a Dword offset for the memory operation (see enumeration)., IMM = 1:Specifies an 8-bit unsigned Dword offset.]
            // SBASE	9	14	enum	6	Bits [6:1] of an aligned pair of SGPRs specifying {size[15:0], base[47:0]}, where base and size are in Dword units. The low-order bits are in the first SGPR.
            // SDST	    15	21	enum	7	Destination for instruction.
            // OP	    22	26	enum	5	
            // ENCODING	27	31	enum	5	Must be 1 1 0 0 0.
            // reserved	0	15		    16	Reserved
            // GLC	    16	16	enum	1	If set, operation is globally coherent.
            // SLC	    17	17	enum	1	System Level Coherent. When set, indicates that the operation is "system level coherent". This controls the L2 cache policy.
            // OP	    18	24	enum	7	0 - 7 reserved.
            // reserved	25	25		    1	Reserved
            // ENCODING	26	31	enum	6	Must be 1 1 0 1 1 1.
            // ADDR	    32	39	enum	8	Source of flat address VGPR.
            // DATA	    40	47	enum	8	Source data.
            // reserved	48	54		    7	Reserved
            // TFE	    55	55	enum	1	Texture Fail Enable. For partially resident textures.
            // VDST	    56	63	enum	8	Destination VGPR.
            
            
            string[] args = Regex.Split(options, @"\s*[,\s]\s*");

            if (args.Length < 3)
                log.Error("number of passed operands is too low");

            // Setup arguments
            string vdst_str = args[0];
            string addr_str = args[1];
            string data_str = args[2];

            OpCode op_code = new OpCode { code = instr.opCode };

            uint vdst_val = ParseOperand.parseOnlyVGPR(vdst_str, 1, log);
            uint addr_op = ParseOperand.parseOnlyVGPR(addr_str, 2, log);
            uint data_op = ParseOperand.parseOnlyVGPR(data_str, 3, log);

            // Parse optional parameters
            for (int i = 3; i < args.Length; i++)
            {
                if (args[i] == "slc")
                    op_code.code |= (1 << 17);
                else if (args[i] == "glc")
                    op_code.code |= (1 << 16);
                else if (args[i] == "tfe")
                    op_code.literal = (1 << 23);
                else
                    log.Error("unknown param for FLAT instruction", args[i]);
            }

            op_code.literal = addr_op | data_op << 8 | vdst_val << 24;

            return op_code;
        }

        /// <summary>
        /// MIMG encoding format 
        /// |ENCODING(6)-26|SLC(1)-25|OP(7)-18|LWE(1)-17|TFE(1)-16|R128(1)-15|DA(1)-14|GLC(1)-13|UNORM(1)-12|DMASK(4)-8|reserved(8)-0|
        /// |reserved(6)-58|SSAMP(5)-53|SRSRC(5)-48|VDATA(8)-40|VADDR(8)-32|
        /// </summary>
        private static OpCode encodeMIMG(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            // reserved	0	7		    8	Reserved.
            // DMASK	8	11	enum	4	Enable mask for image read/write data components. bit0 = red, 1 = green, 2 = blue, 3 = alpha. At least one bit must be on. Data is assumed to be packed into consecutive VGPRs.
            // UNORM	12	12	enum	1	When set to 1, forces the address to be un-normalized, regardless of T#. Must be set to 1 for image stores and atomics
            // GLC	    13	13	enum	1	If set, operation is globally coherent.
            // DA	    14	14	enum	1	Declare an Array [1=Kernel has declared this resource to be an array of texture maps. 0=Kernel has declared this resource to be a single texture map.]
            // R128	    15	15	enum	1	Texture resource size  1 = 128b, 0 = 256b.
            // TFE	    16	16	enum	1	Texture Fail Enable (for partially resident textures).
            // LWE	    17	17	enum	1	LOD Warning Enable (for partially resident textures).
            // OP	    18	24	enum	7	
            // SLC	    25	25	enum	1	System Level Coherent.
            // ENCODING	26	31	enum	6	Must be 1 1 1 1 0 0.
            // VADDR	32	39	enum	8	Address source. Can carry an offset or an index. Specifies the VGPR that holds the first of the image address values.
            // VDATA	40	47	enum	8	Vector GPR to which the result is written.
            // SRSRC	48	52	enum	5	Scalar GPR that specifies the resource constant, in units of four SGPRs.
            // SSAMP	53	57	enum	5	Scalar GPR that specifies the sampler constant, in units of four SGPRs.
            // reserved	58	63		    6	Reserved.

            
            string[] args = Regex.Split(options, @"\s*[,\s]\s*");
            if (args.Length < 4)
               log.Error("number of passed operands is too low");

            OpCode op_code = new OpCode { code = instr.opCode};

            uint vdata_op = ParseOperand.parseOnlyVGPR(args[0], 1, log);
            uint vaddr_op = ParseOperand.parseOnlyVGPR(args[1], 2, log);
            uint srsrc_op = ParseOperand.parseOnlySGPR(args[2], 3, log);
            uint ssamp_op = ParseOperand.parseOnlySGPR(args[3], 4, log);

            if ((srsrc_op & 0x03) != 0)
               log.Warning("SRSRC should be aligned by 4");
            srsrc_op >>= 2; // This field is missing 2 bits from LSB

            if ((ssamp_op & 0x03) != 0)
               log.Warning("SSAMP should be aligned by 4"); 
            ssamp_op >>= 2; // This field is missing 2 bits from LSB

            op_code.literal = vaddr_op | vdata_op << 8 | srsrc_op << 16 | ssamp_op << 21;

	        // Parse optional parameters
	        for (int i = 4; i < args.Length; i++)
	        {
                if (args[i] == "slc")
			        op_code.code |= (1 << 25);
		        else if (args[i] == "lwe")
			        op_code.code |= (1 << 17);
		        else if (args[i] == "tfe")
			        op_code.code |= (1 << 16);
		        else if (args[i] == "r128")
			        op_code.code |= (1 << 15);
		        else if (args[i] == "da")
			        op_code.code |= (1 << 14);
		        else if (args[i] == "glc")
			        op_code.code |= (1 << 13);
		        else if (args[i] == "unorm")
			        op_code.code |= (1 << 12);
		        else if (args[i].StartsWith("dmask:"))
                    op_code.code |= ParseOperand.parseUnSignedNumber(args[i].Remove(0, 6), 4, i, log) << 8;
                else
                   log.Warning("unknown parameter '{0}'", args[i]);
	        }

	        return op_code;
        }

        /// <summary>
        /// encodeBUF contains the core functionality for parsing MUBUF and MTBUF instructions.
        /// </summary>
        private static OpCode encodeBUF(ref InstInfo instr, string options, Log log, out Match m)
        {
            m = Regex.Match(options, @"^(?<VDATA>.*?)\s*,\s*(?<VADDR>.*?)\s*,\s*(?<SRSRC>.*?)\s*,\s*(?<SOFFSET>.*?)"
                +@"(?:[\s,]+(?:(?<ADDR64>addr64)|(?<GLC>glc)|(?<IDXEN>idxen)|(format:\[(?<FORMAT>.*?)\])|"
                +@"(?<OFFEN>offen)|(?<SLC>slc)|(?<TFE>tfe)|(?:(?:offset:)?(?<OFFSET>(?:0x)?[0-9a-f]+))|(?<UNKNOWN>.*?)))*$");

            OpCode op_code = new OpCode { code = instr.opCode };
            if (!m.Success)
            {
                log.Error("Unknown format for instruction on line {0}.");
                return op_code;
            }
            if (m.Groups["UNKNOWN"].Success)
                log.Warning("Ignoring unknown parameter '{0}'.", m.Groups["UNKNOWN"].Value);

            uint vdata_op = ParseOperand.parseOnlyVGPR(m.Groups["VDATA"].Value, 1, log);
            uint vaddr_op = ParseOperand.parseOnlyVGPR(m.Groups["VADDR"].Value, 2, log);
            uint srsrc_op = ParseOperand.parseOnlySGPR(m.Groups["SRSRC"].Value, 3, log);
            if ((srsrc_op & 0x03) != 0)
                log.Warning("SRSRC should be aligned by 4");
            srsrc_op >>= 2;                                         // This field is missing 2 bits from LSB

            uint soffset_op = ParseOperand.parseOnlySGPR(m.Groups["SOFFSET"].Value, 4, log, OpType.SGPR | OpType.M0 | OpType.INLINE);

            op_code.literal = vaddr_op | vdata_op << 8 | srsrc_op << 16 | soffset_op << 24 | ParseOperand.setBitOnFound(m, "SLC", 22, "TFE", 23);

            if (m.Groups["OFFSET"].Success)
                op_code.code |= ParseOperand.parseUnSignedNumber(m.Groups["OFFSET"].Value, 12, 7, log); 

            return op_code;
        }

        /// <summary>
        /// MTBUF encoding format 
        /// |ENCODING(6)-26|NFMT(3)-23|DFMT(4)-19|OP(3)-16|ADDR64(1)-15|GLC(1)-14|IDXEN(1)-13|OFFEN(1)-12|OFFSET(int,12)-0|
        /// |SOFFSET(8)-56|TFE(1)-55|SLC(1)-54|reserved(1)-53|SRSRC(5)-48|VDATA(8)-40|VADDR(8)-32|
        /// </summary>
        private static OpCode encodeMTBUF(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            // OFFSET	0	11	int	12	Unsigned byte offset.
            // OFFEN	12	12	enum	1	If set, send VADDR as an offset. If clear, use zero instead of an offset from a VGPR.
            // IDXEN	13	13	enum	1	If set, send VADDR as an index. If clear, treat the index as zero; 1 = Supply an index from VGPR (VADDR).  0 = Do not (index = 0).
            // GLC	    14	14	enum	1	Globally Coherent. Controls how reads and writes are handled by the L1 texture cache.
            // ADDR64	15	15	enum	1	If set, buffer address is 64-bits (base and size in resource is ignored).
            // OP	    16	18	enum	3	
            // DFMT	    19	22	enum	4	Data format for typed buffer.0
            // NFMT	    23	25	enum	3	Number format for typed buffer.0unorm,1snorm,2uscaled,3sscaled,4uint,5sint,6snorm_ogl,7float
            // Encoding	26	31	enum	6	Must be 1 1 1 0 1 0.
            // VADDR	32	39	enum	8	VGPR address source. Can carry an offset or an index or both (can read two successive VGPRs).
            // VDATA	40	47	enum	8	Vector GPR to read/write result to.
            // SRSRC	48	52	enum	5	Scalar GPR that specifies the resource constant, in units of four SGPRs.
            // reserved	53	53		    1	Reserved.
            // SLC	    54	54	enum	1	System Level Coherent.
            // TFE	    55	55	enum	1	Typed Memory Buffer Operation
            // SOFFSET	56	63	enum	8	Byte offset added to the memory address. Scalar or constant GPR containing the base offset. This is always sent.
            
            Match m;
            OpCode op_code = encodeBUF(ref instr, options, log, out m);

            if (m.Groups["LDS"].Success)
               log.Error("The LDS parameter is only allowed in BUFFER_* instructions.");

            if (m.Groups["FORMAT"].Success)
            {
                Match n = Regex.Match(m.Groups["FORMAT"].Value, @"buf_data_format_((?:\d{1,2}_)*(?:\d{1,2}))\s*,\s*buf_num_format_([a-z_]+)");
                if (!n.Success)
                   log.Error("invalid format in MTBUF");

                string[] dfmt_values = {
				    "8", "16", "8_8", "32", "16_16", "10_11_11", "11_11_10", "10_10_10_2", 
                    "2_10_10_10", "8_8_8_8", "32_32", "16_16_16_16", "32_32_32", "32_32_32_32" };

                string[] nfmt_values = {
				    "unorm", "snorm", "uscaled", "sscaled", "uint", "sint", "snorm_ogl", "float" };

                int dfmt = Array.FindIndex(dfmt_values, item => item == n.Groups[1].Value) + 1;
                if (dfmt < 0)
                   log.Error("invalid DFMT token format in MTBUF");

                int nfmt = Array.FindIndex(nfmt_values, item => item == n.Groups[2].Value);
                if (nfmt < 0)
                   log.Error("invalid NFMT token format in NFMT");

                op_code.code |= (uint)nfmt << 23 | (uint)dfmt << 19 | ParseOperand.setBitOnFound(m, "ADDR64", 15, "GLC", 14, "IDXEN", 13, "OFFEN", 12);
            }

            return op_code;
        }

        /// <summary> 
        /// MUBUF encoding format 
        /// |ENCODING(6)-26|reserved(1)-25|OP(7)-18|reserved(1)-17|LDS(1)-16|ADDR64(1)-15|GLC(1)-14|IDXEN(1)-13|OFFEN(1)-12|OFFSET(int,12)-0|
        /// |SOFFSET(8)-56|TFE(1)-55|SLC(1)-54|reserved(1)-53|SRSRC(5)-48|VDATA(8)-40|VADDR(8)-32|
        /// </summary>
        private static OpCode encodeMUBUF(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            // OFFSET	0	11	int	12	Unsigned byte offset.
            // OFFEN	12	12	enum	1	If set, send VADDR as an offset. If clear, use zero instead of an offset from a VGPR.
            // IDXEN	13	13	enum	1	If set, send VADDR as an index. If clear, treat the index as zero; 1 = Supply an index from VGPR (VADDR).  0 = Do not (index = 0).
            // GLC	    14	14	enum	1	    If set, operation is globally coherent. Controls how reads and writes are handled by the L1 texture cache.
            // ADDR64	15	15	enum	1	"Summary:If set, buffer address is 64-bits (base and size in resource is ignored). Details: Address size is 64-bit.
            // LDS	    16	16	enum	1	"MUBUF-ONLY:  0 = Return read-data to VGPRs.  1 = Return read-data to LDS instead of VGPRs."
            // reserved	17	17		    1	Reserved.
            // OP	    18	24	enum	7	
            // reserved	25	25		    1	Reserved.
            // ENCODING	26	31	enum	6	Must be 1 1 1 0 0 0.
            // VADDR	32	39	enum	8	Address of VGPR to supply first component of address (offset or index). When both index and offset are used, index is in the first VGPR, offset in the second. Can carry an offset or an index or both (can read two VGPRs).
            // VDATA	40	47	enum	8	Address of VGPR to supply first component of write data or receive first component of read-data. Vector GPR to read/write result to.
            // SRSRC	48	52	enum	5	Specifies which SGPR supplies T# (resource constant) in four or eight consecutive SGPRs. This field is missing the two LSBs of the SGPR address, since this address must be aligned to a multiple of four SGPRs
            // reserved	53	53		    1	Reserved.
            // SLC	    54	54	enum	1	System Level Coherent. When set, accesses are forced to miss in level 2 texture cache and are coherent with system memory.
            // TFE	    55	55	enum	1	Texel Fail Enable for PRT (partially resident textures). When set to 1, fetch can return a NACK that causes a VGPR write into DST+1 (first GPR after all fetch-dest GPRs)
            // SOFFSET	56	63	enum	8	SGPR to supply unsigned byte offset. Must be an SGPR, M0, or inline constant.
            
            Match m;
            OpCode op_code = encodeBUF(ref instr, options, log, out m);
            
            op_code.code |= ParseOperand.setBitOnFound(m, "LDS", 16, "ADDR64", 15, "GLC", 14, "IDXEN", 13, "OFFEN", 12);

            // The format parameter is not allowed
            if (m.Groups["FORMAT"].Success)
               log.Error("The FORMAT parameter is only allowed in TBUFFER_* instructions.");

            return op_code;
        }

        /// <summary>
        /// SMRD encoding format 
        /// |ENCODING(5)-27|OP(5)-22|SDST(7)-15|SBASE(6)-9|IMM(1)-8|OFFSET(int,8)-0|
        /// </summary>
        private static OpCode encodeSMRD(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            // OFFSET	0	7	int	    8	Unsigned eight-bit Dword offset to the address specified in SBASE.
            // IMM	    8	8	enum	1	Boolean.[IMM = 0:Specifies an SGPR address that supplies a Dword offset for the memory operation (see enumeration)., IMM = 1:Specifies an 8-bit unsigned Dword offset.]
            // SBASE	9	14	enum	6	Bits [6:1] of an aligned pair of SGPRs specifying {size[15:0], base[47:0]}, where base and size are in Dword units. The low-order bits are in the first SGPR.
            // SDST	    15	21	enum	7	Destination for instruction.
            // OP	    22	26	enum	5	
            // ENCODING	27	31	enum	5	Must be 1 1 0 0 0.
            
            if (instr.name == "s_dcache_inv")
                return (new OpCode { code = instr.opCode});

            if (instr.name == "s_memtime")
            {
                uint sgpr = ParseOperand.parseOnlySGPR(options, 0, log, OpType.SGPR);
                return (new OpCode { code = instr.opCode | (sgpr << 15) | (1 << 7)});
            }

            string[] args = Regex.Split(options, @"\s*[,\s]\s*");

            if ((args.Length < 2) | (args.Length > 4))
               log.Error("S_Load/S_Buffer should contain 2 to 4 arguments.");

            OpCode op_code = new OpCode { code = instr.opCode};

            // SDST (argument 0) - Destination for instruction.
            uint sdst_val = ParseOperand.parseOperand(args[0], OpType.SGPR | OpType.VCC | OpType.TRAP | OpType.M0 | OpType.EXEC, 1, log).value;

            // SBASE (argument 1) - Specifies the SGPR-pair that holds the base byte-address for the fetch.
            uint sbase_op = ParseOperand.parseOperand(args[1], OpType.SGPR | OpType.VCC | OpType.TRAP | OpType.M0 | OpType.EXEC, 2, log).reg;
            if ((sbase_op & 0x01) > 0)
                log.Error("S_Load/S_Buffer must contain an even reg number for the SBASE.");

            // OFFSET (argument 2) - Either holds a ubyte offset or points to a SGPR that contains a byte offset (inline constants not allowed)
            if (args.Length > 2)  // if we don't have an offset then lets not set anything (leave zero)
            {
                OpInfo offset = ParseOperand.parseOperand(args[2], OpType.SCALAR_SRC, 3, log);
                if ((offset.dataDisc & DataDesc.NEGITIVE) != 0)
                    log.Error("offset cannot be negative because it is unsigned");
                if (DataDesc.UINT.HasFlag(offset.dataDisc))
                {
                    // if larger then 256 then use literal value
                    if (offset.value < 256)
                        op_code.code |= (1 << 8) | offset.value;
                    else
                    {
                        op_code.code |= 255;
                        op_code.literal = offset.value;
                    }
                }
                else
                    op_code.code |= offset.reg;
            }

            op_code.code |= (sdst_val << 15) | ((sbase_op >> 1) << 9);

            return op_code;
        }

        /// <summary>
        /// SOP1 encoding format 
        /// |ENCODING(9)-23|SDST(7)-16|OP(8)-8|SSRC0(8)-0|
        /// </summary>
        private static OpCode encodeSOP1(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            // SSRC0	0	7	enum	8	Source 0. First operand for the instruction.
            // OP	    8	15	enum	8	0 – 2 reserved.
            // SDST	    16	22	enum	7	Scalar destination for instruction. Same codes as for SSRC0, above, except that this can use only codes 0 to 127.
            // ENCODING	23	31	enum	9	Must be 1 0 1 1 1 1 1 0 1.
            
            string[] args = Regex.Split(options, @"\s*[,\s]\s*");

            if (args.Length != 2)
                log.Error("SOP1 instructions should have 2 arguments.");

            OpCode op_code = new OpCode { code = instr.opCode };

            // SDST (arg 1) - Destination for instruction.
            uint sdst = ParseOperand.parseOperand(args[0], OpType.SCALAR_DST, 1, log).value;

            // SSRC0 (arg 2)
            OpInfo ssrc0 = ParseOperand.parseOperand(args[1], OpType.SCALAR_SRC, 2, log);
            if (ssrc0.flags.HasFlag(OpType.LITERAL))
                op_code.literal = ssrc0.value;

            op_code.code |= (sdst << 16) | ssrc0.reg;

            return op_code;
        }

        /// <summary>
        /// SOP2 encoding format 
        /// |ENCODING(2)-30|OP(7)-23|SDST(7)-16|SSRC1(8)-8|SSRC0(8)-0|
        /// </summary>
        private static OpCode encodeSOP2(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            // SSRC0	0	7	enum	8	Source 0. First operand for the instruction.
            // SSRC1	8	15	enum	8	Source 1. Second operand for instruction. 
            // SDST	    16	22	enum	7	Scalar destination for instruction. Same codes as for SSRC0, above, except that this can use only codes 0 to 127.
            // OP	    23	29	enum	7	Opcode.
            // ENCODING	30	31	enum	2	Must be 1 0.
            
            string[] args = Regex.Split(options, @"\s*[,\s]\s*");

            if (args.Length != 3)
               log.Error("SOP1 instructions should have 2 arguments.");

            OpCode op_code = new OpCode { code = instr.opCode };

            // SDST (argument 1)
            uint sdst_val = ParseOperand.parseOperand(args[0], OpType.SCALAR_DST, 1, log).value;

            // SSRC0 (argument 2)
            OpInfo ssrc0 = ParseOperand.parseOperand(args[1], OpType.SCALAR_SRC, 2, log);
            if (ssrc0.flags.HasFlag(OpType.LITERAL))
                op_code.literal = ssrc0.value;

            // SSRC1 (argument 3)
            OpInfo ssrc1 = ParseOperand.parseOperand(args[2], OpType.SCALAR_SRC, 3, log);
            if (ssrc1.flags.HasFlag(OpType.LITERAL))
                op_code.literal = ssrc1.value;

            if ((ssrc0.flags.HasFlag(OpType.LITERAL)) && (ssrc1.flags.HasFlag(OpType.LITERAL)))
               log.Error("cannot have two literals");

            op_code.code |= (sdst_val << 16) | (ssrc1.reg << 8) | ssrc0.reg;

            return op_code;
        }

        /// <summary>
        /// SOPC encoding format 
        /// |ENCODING(9)-23|OP(7)-16|SSRC1(8)-8|SSRC0(8)-0|
        /// </summary>
        private static OpCode encodeSOPC(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            // SSRC0	0	7	enum	8	Source 0. First operand for the instruction.
            // SSRC1	8	15	enum	8	Source 1. Second operand for instruction. Same codes as for SSRC0, above.
            // OP	    16	22	enum	7	
            // ENCODING	23	31	enum	9	Must be 1 0 1 1 1 1 1 1 0.
             
            string[] args = Regex.Split(options, @"\s*[,\s]\s*");

            if (args.Length != 2)
               log.Error("SOPC instructions should have 2 arguments.");

            OpCode op_code = new OpCode { code = instr.opCode };

            // SSRC0 (argument 1)
            OpInfo ssrc0 = ParseOperand.parseOperand(args[0], OpType.SCALAR_SRC, 1, log);
            if (ssrc0.flags.HasFlag(OpType.LITERAL))
                op_code.literal = ssrc0.value;

            // SSRC1 (argument 2)
            OpInfo ssrc1 = ParseOperand.parseOperand(args[1], OpType.SCALAR_SRC, 2, log);
            if (ssrc1.flags.HasFlag(OpType.LITERAL))
                op_code.literal = ssrc1.value;

            if ((ssrc0.flags.HasFlag(OpType.LITERAL)) && (ssrc1.flags.HasFlag(OpType.LITERAL)))
               log.Error("cannot have two literals");

            op_code.code |= (ssrc1.reg << 8) | ssrc0.reg;

            return op_code;
        }

        /// <summary>
        /// SOPK encoding format 
        /// |ENCODING(4)-28|OP(5)-23|SDST(7)-16|SIMM16(int,16)-0|
        /// </summary>
        private static OpCode encodeSOPK(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            // SIMM16	0	15	int	    16	
            // SDST	    16	22	enum	7	Scalar destination for instruction. Same codes as for SIMM16, above, except that this can use only codes 0 to 127. 16-bit integer input for opcode. Signedness is determined by opcode. 
            // OP	    23	27	enum	5	Opcode.
            // ENCODING	28	31	enum	4	Must be 1 0 1 1.
            
            string[] args = Regex.Split(options, @"\s*[,\s]\s*");

            if (args.Length < instr.opCtMin)
               log.Error("{0} should have at least {1} argument(s).", instr.name, instr.opCtMin);
            if (args.Length > instr.opCtMax)
               log.Error("{0} should have no more then {1} argument(s).", instr.name, instr.opCtMax);

            OpCode op_code = new OpCode { code = instr.opCode };

            uint immd = ParseOperand.parseSignedNumber(args[0], 16, 2, log);

            uint sdst = ParseOperand.parseOperand(args[1], OpType.SCALAR_DST, 1, log).reg;

            op_code.code |= (sdst << 16) | immd;

            return op_code;
        }
        
        /// <summary>
        /// SOPP encoding format 
        /// |ENCODING(9)-23|OP(7)-16|SIMM16(int,16)-0|
        /// </summary>
        private static OpCode encodeSOPP(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)

            if (instr.name == "s_waitcnt")
            {    
                uint VMCt= 0x0F/*0-15*/, LGKMCt= 0x0F/*0-15*/, EXPCt= 0x07/*0-7*/;

                //string regex = @"(?:(?:"+ 
                //@"vm_?(?:ct|cnt|count)=?\(?(?<vm>\d+)\)?|"+ //vmcnt(#),vm_ct(#), vm_count(#) vmcount(#)...
                //@"lgkm_?(?:ct|cnt|count)=?\((?<lgkm>\d+)\)|" + //vmcnt=#,vm_ct=#, vm_count=# vmcount=#...
                //@"exp_?(?:ct|cnt|count)=?\((?<exp>\d+)\)|" +
                //@")\s*,?\s*)+";

                MatchCollection matches = Regex.Matches(options, @"(vm|lgkm|exp)_?(?:ct|cnt|count)=?\(?(\d+)\)?");

                foreach (Match m in matches)
                {
                    string type = m.Groups[1].Value;
                    uint val = uint.Parse(m.Groups[2].Value);
                    switch (type)
                    {
                        case "lgkm":
                            LGKMCt = val;
                            if (LGKMCt > 31)
                                log.Warning("LGKM_CNT must be between 0 and 15.");
                            break;
                        case "vm":
                            VMCt = val;
                            if (VMCt > 15)
                                log.Warning("VM_CNT must be between 0 and 15.");
                            break;
                        case "exp":
                            EXPCt = val;
                            if (EXPCt > 7)
                                log.Warning("EXPCt must be between 0 and 7.");
                            break;
                    }
                }

                return new OpCode { code = instr.opCode | VMCt | (LGKMCt<<8) | (EXPCt<<4) };
            }

            OpInfo immd;
            if (options == "")
            {
                if (instr.opCtMin > 0)
                    log.Error("{0} expected argument(s).", instr.name);
                immd = new OpInfo();
            }
            else
            {
                string[] args = Regex.Split(options, @"\s*[,\s]\s*");

                if (args.Length != instr.opCtMax)
                    log.Error("{0} should have {1} argument(s).", instr.name, instr.opCtMax, instr.opCtMax);

                immd = ParseOperand.parseOperand(args[0], OpType.CONST, 1, log);
            }

            // Lets make sure the immediate is in range
            if (immd.dataDisc.HasFlag(DataDesc.FLOAT))
               log.Error("Float16 not supported here yet - use hex value instead");
            else if ((((immd.value & 0xffff8000) + 0x8000) & 0xffff7fff) != 0)
               log.Error("The immediate value seems to use more then 16 bits.");

            return new OpCode { code = instr.opCode | immd.value };
        }

        /// <summary>
        /// VINTRP encoding format 
        /// ENCODING(6)-26|VDST(8)-18|OP(2)-16|ATTR(6)-10|ATTRCHAN(2)-8|VSRC(8)-0|
        /// </summary>
        private static OpCode encodeVINTRP(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            // VSRC	    0	7	enum	8	Vector General-Purpose Registers (VGPR) containing the i/j coordinate by which to multiply one of the parameter components.
            // ATTRCHAN	8	9	enum	2	Attribute component to interpolate. See Section 10.1 on page 10-1.
            // ATTR	    10	15	int	    6	Attribute to interpolate.
            // OP	    16	17	enum	2	0:V_INTERP_P1_F32: D = P10 * S + P0; parameter interpolation. 1:V_INTERP_P2_F32: D = P20 * S + D; parameter interpolation. 2: V_INTERP_MOV_F32: D = {P10,P20,P0}[S]; parameter load. 3: reserved.
            // VDST	    18	25	enum	8	Vector General-Purpose Registers (VGPR 0-255) to which results are written, and, optionally, from which they are read when accumulating results.
            // ENCODING	26	31	enum	6	Must be 1 1 0 0 1 0.

            string attrchanString = "0";
            uint vdst = 0, vsrc = 0, attr = 0, attrchan = 0;

            if (instr.name == "v_interp_mov_f32")
            {
                Match m = Regex.Match(options, 
                    @"v(\d+)\s*[,\s]\s*(p?)(\d+)\s*[,\s]\s*" + // first two opps
                    @"(?:attr(\d+)\.([xyzw0123])|" + // for v_interp_mov_f32 v0, p0, attr0.z format
                    @"(?<5>\d+)\s*[,\s]\s*(?<4>\d+)\s*[,\s]\s*\[\s*m0\s*\])"); // for v_interp_mov_f32 v0, p0, 3, 0, [m0] format 

                if (!m.Success)
                    log.Error("v_interp_mov_f32 should be in the format 'v1, p20, attr0.x' or 'v1, p20, 0, 0, [m0]'.");
                else
                {
                    vdst = uint.Parse(m.Groups[1].Value);
                    vsrc = uint.Parse(m.Groups[3].Value);
                    attr = uint.Parse(m.Groups[4].Value);
                    attrchanString = m.Groups[5].Value;
                    
                    if (m.Groups[2].Value == "p")
                    {
                        if (vsrc == 10) vsrc = 0;
                        else if (vsrc == 20) vsrc = 1;
                        else if (vsrc == 0) vsrc = 2;
                    }

                    if (vsrc > 2)
                        log.Error("v_interp_mov_f32 p should be p10,p20,p0 or 0,1,2)");
                }
            }
            else if (instr.name == "v_interp_p1_f32")
            {
                Match m = Regex.Match(options, 
                    @"v(\d+)\s*[,\s]\s*(v)(\d+)\s*[,\s]\s*" + // first two opps
                    @"(?:attr(\d+)\.([xyzw0123])|" + // for v_interp_p1_f32 v0, p0, attr0.z format
                    @"(?<5>\d+)\s*[,\s]\s*(?<4>\d+)\s*[,\s]\s*\[\s*m0\s*\])"); // for v_interp_p1_f32 v0, v0, 3, 0, [m0] format 
                
                if (!m.Success)
                    log.Error("v_interp_p1_f32 should be in the format 'v2, v4, attr0.x' or 'v2, v4, 0, 0, [m0]'.");
                else
                {
                    vdst = uint.Parse(m.Groups[1].Value);
                    vsrc = uint.Parse(m.Groups[3].Value);
                    attr = uint.Parse(m.Groups[4].Value);
                    attrchanString = m.Groups[5].Value;
                    
                    if (vsrc > 255)
                        log.Error("v_interp_p1_f32 vsrc should be less then 256.");
                }
            }
            else if (instr.name == "v_interp_p2_f32")
            {
                Match m = Regex.Match(options, 
                    @"v(\d+)\s*[,\s]\s*(?:\[v(\d+)\]\s*[,\s]\s*)?v(\d+)\s*[,\s]\s*" + // first two opps 
                    @"(?:attr(\d+)\.([xyzw0123])|" + // for v_interp_p2_f32 v0, p0, attr0.z format
                    @"(?<5>\d+)\s*[,\s]\s*(?<4>\d+)\s*[,\s]\s*\[\s*m0\s*\])"); // for v_interp_p2_f32 v0, v0, 3, 0, [m0] format 
                
                if (!m.Success)
                    log.Error("v_interp_p2_f32 should be in the format 'v2, [v2], v4, attr0.x' or 'v2, v4, 0, 0, [m0]'.");
                else
                {
                    vdst = uint.Parse(m.Groups[1].Value);
                    vsrc = uint.Parse(m.Groups[3].Value);
                    attr = uint.Parse(m.Groups[4].Value);
                    attrchanString = m.Groups[5].Value;

                    if (vsrc > 255)
                        log.Error("v_interp_p2_f32 vsrc should be less then 256.");
                }

                if (m.Groups[2].Success)
                    if (m.Groups[1].Value != m.Groups[2].Value)
                        log.Error("The first two vector register numbers do not match. (i.e. v_interp_p2_f32 v7 [v7]...");
            }

            if (attrchanString == "x") attrchan = 0;
            else if (attrchanString == "y") attrchan = 1;
            else if (attrchanString == "z") attrchan = 2;
            else if (attrchanString == "w") attrchan = 3;
            else attrchan = uint.Parse(attrchanString);

            if (vdst > 255)
            {
                log.Error("{0} vdst should be less then 256.", instr.name);
                vdst = 0;
            }
            if (attr > 31)
            {
                log.Error("{0} attr should be 0 to 31.", instr.name);
                attr = 0;
            }
            if (attrchan > 3)
            {
                log.Error("{0} attrchan should be x,y,z,w or 0,1,2,3)", instr.name);
                attrchan = 0;
            }
            return new OpCode { code = instr.opCode | (vdst << 18) | (attr << 10) | (attrchan << 8) | vsrc };
        }

        /// <summary>
        /// VOP1 encoding format 
        /// |ENCODE(7)-25|VDST(8)-17|OP(8)-9|SRC0(9)-0|
        /// </summary>
        private static OpCode encodeVOP1(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            
            string[] args = Regex.Split(options, @"\s*[,\s]\s*");

            if (args.Length < instr.opCtMin)
               log.Error("{0} should have at least {1} argument(s).", instr.name, instr.opCtMin);
            if (args.Length > instr.opCtMax)
               log.Error("{0} should have no more then {1} argument(s).", instr.name, instr.opCtMax);

            OpCode op_code = new OpCode { code = instr.opCode };

            uint vdst;
            OpInfo vsrc0;
            if (instr.name == "v_readfirstlane_b32")
            {
                vdst = ParseOperand.parseOnlySGPR(args[0], 1, log);
                vsrc0 = ParseOperand.parseOperand(args[1], OpType.VGPR | OpType.LDS_DIRECT, 2, log);
            }
            else
            {
                vdst = ParseOperand.parseOnlyVGPR(args[0], 1, log);
                vsrc0 = ParseOperand.parseOperand(args[1], OpType.ALL, 2, log);
                if (vsrc0.flags.HasFlag(OpType.LITERAL))
                    op_code.literal = vsrc0.value;
            }

            op_code.code |= (vdst << 17) | vsrc0.reg;


            return op_code;
        }

        /// <summary>
        /// VOP2 encoding format 
        /// |Encode(1)-31|OP(6)-25|VDST(8)-17|VSRC1(8)-9|SRC0(9)-0|
        /// </summary>
        private static OpCode encodeVOP2(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            
            string[] args = Regex.Split(options, @"\s*[,\s]\s*");
            int argCt = args.Length;

            // handle "vcc" destination in VOP2.  example: v_add_a32 v0, vcc, v1, v2
            
            if (instr.OpNum.IsBetween<uint>(36, 43))
                if (args[1] == "vcc")
                {
                    for (int i = 2; i < args.Length; i++)
                        args[i - 1] = args[i];
                    argCt--;
                }
                else
                    log.Warning("{0} should specify VCC as output param for clarity.", instr.name);



            if (argCt < instr.opCtMin)
                log.Error("{0} should have at least {1} argument(s).", instr.name, instr.opCtMin);
            if (argCt > instr.opCtMax)
                log.Error("{0} should have no more then {1} argument(s).", instr.name, instr.opCtMax);


            OpCode op_code = new OpCode { code = instr.opCode};

            uint vdst, vsrc1; 
            OpInfo vsrc0;
            if (instr.name == "v_readlane_b32")
            {
                vdst = ParseOperand.parseOnlySGPR(args[0], 1, log, OpType.SCALAR_DST);
                vsrc0 = ParseOperand.parseOperand(args[1], OpType.VGPR | OpType.LDS_DIRECT, 2, log);
                vsrc1 = ParseOperand.parseOperand(args[2], OpType.SGPR | OpType.LDS_DIRECT, 3, log).reg;
            }
            else if (instr.name == "v_writelane_b32")
            {
                vdst = ParseOperand.parseOnlyVGPR(args[0], 1, log);
                vsrc0 = ParseOperand.parseOperand(args[1], OpType.SGPR | OpType.M0 | OpType.EXEC | OpType.CONST, 2, log);
                vsrc1 = ParseOperand.parseOperand(args[2], OpType.SGPR | OpType.M0, 3, log).reg;
            }
            else
            {
                vdst = ParseOperand.parseOnlyVGPR(args[0], 1, log);
                vsrc0 = ParseOperand.parseOperand(args[1], OpType.ALL, 2, log);
                if (vsrc0.flags.HasFlag(OpType.LITERAL))
                    op_code.literal = vsrc0.value;
                vsrc1 = ParseOperand.parseOnlyVGPR(args[2], 2, log);
            }

            op_code.code |= (vdst << 17) | (vsrc1 << 9) | vsrc0.reg;

            return op_code;
        }

        private static uint processOModFromRegEx(Match m, Log log)
        {
            uint val = 0;
            if (m.Groups["omod"].Success)
            {
                if (!m.Groups["omod"].Value.Contains("."))
                    log.Warning("omod or mul arguements should use use a period(.) in the number to show it is float");

                if (Regex.Match(m.Groups["omod"].Value, "^4.?0*$").Success)
                    val |= 1 << 58;
                else if (Regex.Match(m.Groups["omod"].Value, "^2.?0*$").Success)
                    val |= 2 << 58;
                else if (Regex.Match(m.Groups["omod"].Value, "^0?.50*$").Success)
                    val |= 3 << 58;
                else
                    log.Error("unknown omod format, valid examples: omod:2.0, omod:4.0, omod:0.5 "
                        +"mul(2.0) mul(4.0) mul(0.5) or mul=2.0 mul=4.0 mul=0.5");
            }
            return val;
        }

        /// <summary>
        /// VOP3a0 encoding format 
        /// specific version of VOP3a that is used for zero sources and zero destination
        /// |ENCODING(6)-26|OP(9)-17|reserved(5)-12|CLAMP(1)-11|ABS(3)-8|VDST(8)-0|
        /// |NEG(3)-61|OMOD(2)-59|SRC2(9)-50|SRC1(9)-41|SRC0(8)-32|
        /// </summary>
        private static OpCode encodeVOP3a0(InstInfo instr, string options, Log log)
        {
            if (Regex.Match(options, @"[^\s]+").Success) // catch anything unknown options to throw a warning
               log.Warning("{0} does not support any parameters. (ignoring)", instr.name);

            return new OpCode { code = instr.opCode }; ;
        }
        
        /// <summary>
        /// VOP3a1 encoding format 
        /// specific version of VOP3a that only uses one source
        /// |ENCODING(6)-26|OP(9)-17|reserved(5)-12|CLAMP(1)-11|ABS(3)-8|VDST(8)-0|
        /// |NEG(3)-61|OMOD(2)-59|SRC2(9)-50|SRC1(9)-41|SRC0(8)-32|
        /// </summary>
        private static OpCode encodeVOP3a1(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            
            OpCode op_code = new OpCode { code = instr.opCode };

            Match m = Regex.Match(options, @"
            (?<P0>.+?) \s*[,\s]\s*                          # Dest
            (?:(?<N1>neg)\((?<A1>abs)\((?<P1>.+?)\)\)|(?<N1>neg)\((?<P1>.+?)\)|(?<A1>abs)\((?<P1>.*?)\)|(?<P1>.+?)) # Src0
            (?:\s*[,\s]\s* (?:
                (?<CLAMP>clamp)                             # clamp option
                |(?:omod|mul)[:=\(](?<omod>[a-z0-9\.]+)\)?  # omod options like omod=2.0 omod:0.5 mul(2.0)
                |(?<Unknown>[^\s]+)                         # catch anything unknown options to throw error
                ))+
            ", RegexOptions.IgnorePatternWhitespace);

            if (!m.Success)
            {
               log.Error("Unable to decode parameters for single source VOP3a1 instruction '{0}'.", instr.name);
                return op_code;
            }

            if (!m.Groups["Unknown"].Success)
               log.Warning("Ignoring Unknown parameter information '{0}' for '{1}'.", m.Groups["Unknown"].Value, instr.name);

            uint vdst, vsrc0;
            if (instr.name == "V_READFIRSTLANE_B32")
            {
                vdst = ParseOperand.parseOnlySGPR(m.Groups["P0"].Value, 1, log);
                vsrc0 = ParseOperand.parseOperand(m.Groups["P1"].Value, OpType.VGPR | OpType.LDS_DIRECT, 2, log).reg;
            }
            else
            {
                vdst = ParseOperand.parseOnlyVGPR(m.Groups["P0"].Value, 1, log);
                vsrc0 = ParseOperand.parseOperand(m.Groups["P1"].Value, OpType.VOP3_SRC, 2, log).reg;
            }

            op_code.code |= vdst | ParseOperand.setBitOnFound(m, "A1", 8, "CLAMP", 11);
            op_code.literal = vsrc0 | ParseOperand.setBitOnFound(m, "N1", 60) | processOModFromRegEx(m, log );

            return op_code;
        }

        private static void Vector_2Src_ALU_Limitations(ref OpInfo vsrc0, ref OpInfo vsrc1, Log log)
        {
            // At most one SGPR can be read per instruction, but the value can be used for more than one operand.
            int SGPRCt = (vsrc0.flags.HasFlag(OpType.SGPR) ? 1 : 0) + (vsrc1.flags.HasFlag(OpType.SGPR) ? 1 : 0);
            int M0__Ct = (vsrc0.flags.HasFlag(OpType.M0) ? 1 : 0) + (vsrc1.flags.HasFlag(OpType.M0) ? 1 : 0);
            int lit_Ct = (vsrc0.flags.HasFlag(OpType.LITERAL) ? 1 : 0) + (vsrc1.flags.HasFlag(OpType.LITERAL) ? 1 : 0);

            if (SGPRCt > 1)
                log.Error("At most one SGPR can be read per instruction, but the value can be used for more than one operand.");

            // At most one literal constant can be used, and only when an SGPR or M0 is not used as a source.
            if (lit_Ct > ((SGPRCt + M0__Ct > 0) ? 0 : 1))
                log.Error("At most one literal constant can be used, and only when an SGPR or M0 is not used as a source.");

            // Only SRC0 can use LDS_DIRECT.
            if (vsrc1.flags.HasFlag(OpType.LDS_DIRECT))
                log.Error("Only SRC0 can use LDS_DIRECT.");
        }

        private static void Vector_3Src_ALU_Limitations(ref OpInfo vsrc0, ref OpInfo vsrc1, ref OpInfo vsrc2, Log log)
        {
            // At most one SGPR can be read per instruction, but the value can be used for more than one operand.
            int SGPRCt = (vsrc0.flags.HasFlag(OpType.SGPR) ? 1 : 0) + (vsrc1.flags.HasFlag(OpType.SGPR) ? 1 : 0) + (vsrc2.flags.HasFlag(OpType.SGPR) ? 1 : 0);
            int M0__Ct = (vsrc0.flags.HasFlag(OpType.M0) ? 1 : 0) + (vsrc1.flags.HasFlag(OpType.M0) ? 1 : 0) + (vsrc2.flags.HasFlag(OpType.M0) ? 1 : 0);
            int lit_Ct = (vsrc0.flags.HasFlag(OpType.LITERAL) ? 1 : 0) + (vsrc1.flags.HasFlag(OpType.LITERAL) ? 1 : 0) + (vsrc2.flags.HasFlag(OpType.LITERAL) ? 1 : 0);

            if (SGPRCt > 1)
                log.Error("At most one SGPR can be read per instruction, but the value can be used for more than one operand.");

            // At most one literal constant can be used, and only when an SGPR or M0 is not used as a source.
            if (lit_Ct > ((SGPRCt + M0__Ct > 0) ? 0 : 1))
                log.Error("At most one literal constant can be used, and only when an SGPR or M0 is not used as a source.");

            // Only SRC0 can use LDS_DIRECT.
            if (vsrc1.flags.HasFlag(OpType.LDS_DIRECT) | vsrc2.flags.HasFlag(OpType.LDS_DIRECT))
                log.Error("Only SRC0 can use LDS_DIRECT.");
        }
        
        /// <summary>
        /// VOP3a2 encoding format 
        /// specific version of VOP3a that uses two sources and 1 destination
        /// |ENCODING(6)-26|OP(9)-17|reserved(5)-12|CLAMP(1)-11|ABS(3)-8|VDST(8)-0|
        /// |NEG(3)-61|OMOD(2)-59|SRC2(9)-50|SRC1(9)-41|SRC0(8)-32|
        /// </summary>
        private static OpCode encodeVOP3a2(InstInfo instr, string options, Log log)
        {
            OpCode op_code = new OpCode { code = instr.opCode };

            Match m = Regex.Match(options, @"
            (?<P0>.+?) \s*[,\s]\s*                                                                                 # Dest
            (?:(?<N1>neg)\((?<A1>abs)\((?<P1>.+?)\)\)|(?<N1>neg)\((?<P1>.+?)\)|(?<A1>abs)\((?<P1>.*?)\)|(?<P1>.+?)) \s*[,\s]\s*  # Src0
            (?:(?<N2>neg)\((?<A2>abs)\((?<P2>.+?)\)\)|(?<N2>neg)\((?<P2>.+?)\)|(?<A2>abs)\((?<P2>.*?)\)|(?<P2>.+?)) # Src1
            (?:\s*[,\s]\s* (?:
                (?<CLAMP>clamp)          # clamp option
                |(?:omod|mul)[:=\(](?<omod>[a-z0-9\.]+)\)?   # omod options like omod=2.0 omod:0.5 mul(2.0)
                |(?<Unknown>[^\s]+)      # catch anything unknown options to throw error
                ))+
            ", RegexOptions.IgnorePatternWhitespace);
            

            if (!m.Success)
            {
               log.Error("Unable to decode parameters for single source VOP3a2 instruction '{0}'.", instr.name);
                return op_code;
            }

            if (!m.Groups["Unknown"].Success)
               log.Warning("Ignoring Unknown parameter information '{0}' for '{1}'.", m.Groups["Unknown"].Value, instr.name);

            uint vdst;
            OpInfo vsrc0, vsrc1;
            if (instr.name == "V_READLANE_B32")
            {
                vdst = ParseOperand.parseOnlySGPR(m.Groups["P0"].Value, 1, log);
                vsrc0 = ParseOperand.parseOperand(m.Groups["P1"].Value, OpType.VGPR | OpType.LDS_DIRECT, 2, log);
                vsrc1 = ParseOperand.parseOperand(m.Groups["P2"].Value, OpType.SGPR | OpType.LDS_DIRECT, 3, log);
            }
            else
            {
                vdst = ParseOperand.parseOnlyVGPR(m.Groups["P0"].Value, 1, log);
                vsrc0 = ParseOperand.parseOperand(m.Groups["P1"].Value, OpType.VOP3_SRC, 2, log);
                vsrc1 = ParseOperand.parseOperand(m.Groups["P2"].Value, OpType.VOP3_SRC, 3, log);
            }

            Vector_2Src_ALU_Limitations(ref vsrc0, ref vsrc1, log);

            op_code.code |= vdst | ParseOperand.setBitOnFound(m, "A1", 8, "A2", 9, "CLAMP", 11);
            op_code.literal = vsrc0.reg | vsrc1.reg << 9 | ParseOperand.setBitOnFound(m, "N1", 60, "N2", 61) | processOModFromRegEx(m, log);

            return op_code;
        }

        /// <summary>
        /// VOP3a3 encoding format 
        /// specific version of VOP3a that 3 sources 
        /// |ENCODING(6)-26|OP(9)-17|reserved(2)-15|SDST(7)-8|VDST(8)-0|
        /// |NEG(3)-61|OMOD(2)-59|SRC2(9)-50|SRC1(9)-41|SRC0(9)-32|        
        /// </summary>
        private static OpCode encodeVOP3a3(InstInfo instr, string options, Log log)
        {
            OpCode op_code = new OpCode { code = instr.opCode };

            Match m = Regex.Match(options, @"
            (?<P0>.+?) \s*[,\s]\s*                                                                                 # Dest
            (?:(?<N1>neg)\((?<A1>abs)\((?<P1>.+?)\)\)|(?<N1>neg)\((?<P1>.+?)\)|(?<A1>abs)\((?<P1>.*?)\)|(?<P1>.+?)) \s*[,\s]\s*  # Src0
            (?:(?<N2>neg)\((?<A2>abs)\((?<P2>.+?)\)\)|(?<N2>neg)\((?<P2>.+?)\)|(?<A1>abs)\((?<P2>.*?)\)|(?<P2>.+?)) \s*[,\s]\s*  # Src1
            (?:(?<N3>neg)\((?<A3>abs)\((?<P3>.+?)\)\)|(?<N3>neg)\((?<P3>.+?)\)|(?<A2>abs)\((?<P3>.*?)\)|(?<P3>.+?)) # Src2
            (?:\s*[,\s]\s* (?:
                (?<CLAMP>clamp)          # clamp option
                |(?:omod|mul)[:=\(](?<omod>[a-z0-9\.]+)\)?   # omod options like omod=2.0 omod:0.5 mul(2.0)
                |(?<Unknown>[^\s]+)      # catch anything unknown options to throw error
                ))+
            ", RegexOptions.IgnorePatternWhitespace);

            if (!m.Success)
            {
               log.Error("Unable to decode parameters for single source VOP3a3 instruction '{0}'.", instr.name);
                return op_code;
            }

            if (!m.Groups["Unknown"].Success)
               log.Warning("Ignoring Unknown parameter information '{0}' for '{1}'.", m.Groups["Unknown"].Value, instr.name);

            uint vdst = ParseOperand.parseOnlyVGPR(m.Groups["P0"].Value, 1, log);
            OpInfo vsrc0 = ParseOperand.parseOperand(m.Groups["P1"].Value, OpType.VOP3_SRC, 2, log);
            OpInfo vsrc1 = ParseOperand.parseOperand(m.Groups["P2"].Value, OpType.VOP3_SRC, 3, log);
            OpInfo vsrc2 = ParseOperand.parseOperand(m.Groups["P3"].Value, OpType.VOP3_SRC, 4, log);

            Vector_3Src_ALU_Limitations(ref vsrc0, ref vsrc1, ref vsrc2, log);

            op_code.code |= vdst | ParseOperand.setBitOnFound(m, "A1", 8, "A2", 9, "A3", 10, "CLAMP", 11);
            op_code.literal = vsrc0.reg | vsrc1.reg << 9 | vsrc2.reg << 18 
                | ParseOperand.setBitOnFound(m, "N1", 60, "N2", 61, "N3", 62) | processOModFromRegEx(m, log);

            return op_code;
        }

        /// <summary>
        /// VOP3b2 encoding format
        /// specific version of VOP3b that 2 sources
        /// |ENCODING(6)-26|OP(9)-17|reserved(2)-15|SDST(7)-8|VDST(8)-0|
        /// |NEG(3)-61|OMOD(2)-59|SRC2(9)-50|SRC1(9)-41|SRC0(9)-32|        
        /// </summary>
        private static OpCode encodeVOP3b2(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
            
            OpCode op_code = new OpCode { code = instr.opCode };

            Match m = Regex.Match(options, @"
            ((?<D0>.+?) \s*[,\s]\s*)??  # SDst (Optional)
            ((?<D1>.+?) \s*[,\s]\s*)    # VDst
            (?:(?<N1>neg)\((?<A1>abs)\((?<P1>.+?)\)\)|(?<N1>neg)\((?<P1>.+?)\)|(?<A1>abs)\((?<P1>.*?)\)|(?<P1>.+?)) \s*[,\s]\s*  # Src0
            (?:(?<N2>neg)\((?<A2>abs)\((?<P2>.+?)\)\)|(?<N2>neg)\((?<P2>.+?)\)|(?<A2>abs)\((?<P2>.*?)\)|(?<P2>.+?)) # Src1
            (?:\s*[,\s]\s* (?:
                (?:omod|mul)[:=\(](?<omod>[a-z0-9\.]+)\)?   # omod options like omod=2.0 omod:0.5 mul(2.0)
                |(?<Unknown>[^\s]+)      # catch anything unknown options to throw error
                ))+
            ", RegexOptions.IgnorePatternWhitespace);

            if (!m.Success)
            {
               log.Error("Unable to decode the 2 parameters for compare instruction '{0}'.", instr.name);
                return op_code;
            }

            if (!m.Groups["Unknown"].Success)
               log.Warning("Ignoring Unknown parameter '{0}' for instruction '{1}'.", m.Groups["Unknown"].Value, instr.name);
            
            if (m.Groups["A1"].Success | m.Groups["A2"].Success)
               log.Error("ABS() is not allowed for VOP3b instruction '{0}'.", instr.name);
            
            uint sdst;
            if (m.Groups["D0"].Success)
                sdst = ParseOperand.parseOperand(m.Groups["D0"].Value, OpType.SGPR | OpType.VCC | OpType.TRAP, 1, log).reg;
            else
                sdst = 106; // default to VCC

            uint vdst = ParseOperand.parseOnlyVGPR(m.Groups["D1"].Value, 1, log); //   uint vdst = ParseOperand.parseOperand(m.Groups["D1"].Value, log, 1, OpType.SGPR | OpType.VCC | OpType.TRAP).reg;
            OpInfo vsrc0 = ParseOperand.parseOperand(m.Groups["P1"].Value, OpType.VOP3_SRC, 2, log);
            OpInfo vsrc1 = ParseOperand.parseOperand(m.Groups["P2"].Value, OpType.VOP3_SRC, 3, log);

            Vector_2Src_ALU_Limitations(ref vsrc0, ref vsrc1, log);

            op_code.code |= vdst | (sdst << 8);
            op_code.literal = vsrc0.reg | vsrc1.reg << 9 | ParseOperand.setBitOnFound(m, "N1", 60, "N2", 61) | processOModFromRegEx(m, log);

            return op_code;
        }

        /// <summary>
        /// VOP3b3 encoding format
        /// specific version of VOP3b that 3 sources
        /// |ENCODING(6)-26|OP(9)-17|reserved(2)-15|SDST(7)-8|VDST(8)-0|
        /// |NEG(3)-61|OMOD(2)-59|SRC2(9)-50|SRC1(9)-41|SRC0(9)-32|        
        /// </summary>
        private static OpCode encodeVOP3b3(InstInfo instr, string options, Log log)
        {
            OpCode op_code = new OpCode { code = instr.opCode };

            Match m = Regex.Match(options, @"
            ((?<D0>.+?) \s*[,\s]\s*)??  # SDst (Optional)
            ((?<D1>.+?) \s*[,\s]\s*)    # VDst
            (?:(?<N1>neg)\((?<A1>abs)\((?<P1>.+?)\)\)|(?<N1>neg)\((?<P1>.+?)\)|(?<A1>abs)\((?<P1>.*?)\)|(?<P1>.+?)) \s*[,\s]\s*  # Src0
            (?:(?<N2>neg)\((?<A2>abs)\((?<P2>.+?)\)\)|(?<N2>neg)\((?<P2>.+?)\)|(?<A2>abs)\((?<P2>.*?)\)|(?<P2>.+?)) \s*[,\s]\s*  # Src1
            (?:(?<N3>neg)\((?<A3>abs)\((?<P3>.+?)\)\)|(?<N3>neg)\((?<P3>.+?)\)|(?<A3>abs)\((?<P3>.*?)\)|(?<P3>.+?)) # Src2
            (?:\s*[,\s]\s* (?:
                (?:omod|mul)[:=\(](?<omod>[a-z0-9\.]+)\)?   # omod options like omod=2.0 omod:0.5 mul(2.0)
                |(?<Unknown>[^\s]+)      # catch anything unknown options to throw error
                ))+
            ", RegexOptions.IgnorePatternWhitespace);

            if (!m.Success)
            {
               log.Error("Unable to decode the 3 to 4 parameters for instruction '{0}'.", instr.name);
                return op_code;
            }

            if (!m.Groups["Unknown"].Success)
               log.Warning("Ignoring Unknown parameter '{0}' for instruction '{1}'.", m.Groups["Unknown"].Value, instr.name);

            if (m.Groups["A1"].Success | m.Groups["A2"].Success | m.Groups["A3"].Success)
               log.Error("ABS() is not allowed for VOP3b instruction '{0}'.", instr.name);

            uint sdst;
            if (m.Groups["D0"].Success)
                sdst = ParseOperand.parseOperand(m.Groups["D0"].Value, OpType.SGPR | OpType.VCC | OpType.TRAP, 1, log).reg;
            else
                sdst = 106; // default to VCC

            uint vdst = ParseOperand.parseOperand(m.Groups["D1"].Value, OpType.SGPR | OpType.VCC | OpType.TRAP, 1, log).reg;
            OpInfo vsrc0 = ParseOperand.parseOperand(m.Groups["P1"].Value, OpType.VOP3_SRC, 2, log);
            OpInfo vsrc1 = ParseOperand.parseOperand(m.Groups["P2"].Value, OpType.VOP3_SRC, 3, log);
            OpInfo vsrc2 = ParseOperand.parseOperand(m.Groups["P3"].Value, OpType.VOP3_SRC, 4, log);

            Vector_3Src_ALU_Limitations(ref vsrc0, ref vsrc1, ref vsrc2, log);

            op_code.code |= vdst | (sdst << 8);
            op_code.literal = vsrc0.reg | vsrc1.reg << 9 | vsrc2.reg << 18 | ParseOperand.setBitOnFound(m, "N1", 60, "N2", 61, "N3", 61) | processOModFromRegEx(m, log);

            return op_code;
        }

        /// <summary>
        /// VOP3bC encoding format
        /// Specific version of VOP3b used for comparing 2 sources. 
        /// Notes: SDST defaults to VCC if unspecified; VDST not used; omod not used
        /// |ENCODING(6)-26|OP(9)-17|reserved(5)-12|CLAMP(1)-11|ABS(3)-8|VDST(8)-0|
        /// |NEG(3)-61|OMOD(2)-59|SRC2(9)-50|SRC1(9)-41|SRC0(8)-32|
        /// </summary>
        private static OpCode encodeVOP3bC(InstInfo instr, string options, Log log)
        {
            OpCode op_code = new OpCode { code = instr.opCode };

            Match m = Regex.Match(options, @"
            ((?<D0>.+?) \s*[,\s]\s*)??  # Optional SDst
            (?:(?<N1>neg)\((?<A1>abs)\((?<P1>.+?)\)\)|(?<N1>neg)\((?<P1>.+?)\)|(?<A1>abs)\((?<P1>.*?)\)|(?<P1>.+?)) \s*[,\s]\s*  # Src0
            (?:(?<N2>neg)\((?<A2>abs)\((?<P2>.+?)\)\)|(?<N2>neg)\((?<P2>.+?)\)|(?<A2>abs)\((?<P2>.*?)\)|(?<P2>.+?)) # Src1
            (?:\s*[,\s]\s* (?:
                (?<Unknown>[^\s]+)      # catch anything unknown options to throw error
                ))+
            ", RegexOptions.IgnorePatternWhitespace);

            if (!m.Success)
            {
               log.Error("Unable to decode VOP3b compare instruction '{0}'.", instr.name);
                return op_code;
            }

            if (!m.Groups["Unknown"].Success)
               log.Warning("Ignoring Unknown parameter '{0}' for instruction '{1}'.", m.Groups["Unknown"].Value, instr.name);

            if (m.Groups["A1"].Success | m.Groups["A2"].Success)
               log.Error("ABS() is not allowed for VOP3b instruction '{0}'.", instr.name);

            uint sdst;
            if (m.Groups["D0"].Success)
                sdst = ParseOperand.parseOperand(m.Groups["D0"].Value, OpType.SGPR | OpType.VCC | OpType.TRAP, 1, log).reg;
            else
                sdst = 106; // default to VCC

            OpInfo vsrc0 = ParseOperand.parseOperand(m.Groups["P1"].Value, OpType.VOP3_SRC, 2, log);
            OpInfo vsrc1 = ParseOperand.parseOperand(m.Groups["P2"].Value, OpType.VOP3_SRC, 3, log);

            Vector_2Src_ALU_Limitations(ref vsrc0, ref vsrc1, log);

            op_code.code |= (sdst << 8);
            op_code.literal = vsrc0.reg | vsrc1.reg << 9 | ParseOperand.setBitOnFound(m, "N1", 60, "N2", 61);

            return op_code;
        }
        
        /// <summary>
        /// VOPC encoding format 
        /// |ENCODE(7)-25|OP(8)-17|VSRC1(8)-9|SRC0(9)-0|
        /// </summary>
        private static OpCode encodeVOPC(InstInfo instr, string options, Log log)
        {
            // Field   beg end  dataType  size  notes  (from AMD ISA Manual)
	        
            string[] args = Regex.Split(options, @"\s*[,\s]\s*");

            if (args.Length != 2)
               log.Error("VOPC instructions should have two arguments.");

            OpCode op_code = new OpCode { code = instr.opCode};

            OpInfo vsrc0 = ParseOperand.parseOperand(args[0], OpType.ALL, 1, log);
            if (vsrc0.flags.HasFlag(OpType.LITERAL))
                op_code.literal = vsrc0.value;
            uint vsrc1 = ParseOperand.parseOnlyVGPR(args[1], 2, log);

            op_code.code |=  (vsrc1 << 9) | vsrc0.reg;

            return op_code;
        }
    }
}
