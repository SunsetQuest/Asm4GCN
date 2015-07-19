// This file contains the GCN's specifications.
//
// Information Source:
//   *AMD southern islands ISA manual (90%+ of the raw data came from this PDF)
//   *AMD Sea Island ISA Guide (Feb 2013 version) 
//   *Daniel Bali, Dec 2013; new link: https://github.com/ukasz/amd-gcn-isa-assembler/tree/master/src
// Thank you AMD and Daniel Bali for making your work available to the community.
//
// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without any warranty.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


//Important differences between Generation 2 and 3 GPUs
// Data Parallel ALU operations improve “Scan” and cross-lane operations.
// Scalar memory writes.
// In Generation 2, a kernel can read and WRITE to scalar data cache.
// Compute kernel context switching.
// Compute kernels now can be context-switched on and off the GPU.

//Summary of kernel instruction change from Generation 2 to 3
//  Modified many of the microcode formats: VOP3A, VOP3B, LDS, GDS MUBUF, MTBUF, MIMG, and EXP.
//  SMRD microcode format is replaced with SMEM, now supporting reads and writes.  [DONE]
//  VGPR Indexing for VALU instructions.

//New Instructions
//– Scalar Memory Writes.
//– S_CMP_EQ_U64, S_CMP_NE_U64.
//– 16-bit floating point VALU instructions.
//– “SDWA” – Sub Dword Addressing allows access to bytes and words of VGPRs in VALU instructions.
//– “DPP” – Data Parallel Processing allows VALU instructions to access data from neighboring lanes.
//– V_PERM_B32.
//– DS_PERMUTE_RTN_B32, DS_BPERMPUTE_RTN_B32.
// Removed Instructions
//– V_MAC_LEGACY_F32
//– V_CMPS* - now supported by V_CMP with the “clamp” bit set to 1.
//– V_MULLIT_F32.
//– V_{MIN, MAX, RCP, RSQ}_F32.
//– V_{LOG, RCP, RSQ}_CLAMP_F32.
//– V_{RCP, RSQ}_CLAMP_F64.
//– V_MUL_LO_I32 (it’s functionally identical to V_MUL_LO_U32).
//– All non-reverse shift instructions.
//– LDS and Memory atomics: MIN, MAX and CMPSWAP on F32 and F64 data.

//– snorm_lz (aka: snorm_ogl)
//– ubnorm
//– ubnorm_nz (aka: ubnorm_ogl)
//– ubint
//– ubscaled



namespace GcnTools
{
    /// <summary>
    /// Contains the Encoding bit format an instruction uses.
    /// Source: Daniel Bali, Dec 2013 (updated link: https://github.com/ukasz/amd-gcn-isa-assembler/blob/master/src/isa_instr.h) 
    /// </summary>
    public enum ISA_Enc
    {
        NONE,
        SOP1,
        SOP2,
        SOPK,
        SOPC,
        SOPP,
        SMEM,
        VOP1,
        VOP2,
        VOP3a0,
        VOP3a1,
        VOP3a2,
        VOP3a3,
        VOP3b2,
        VOP3b3,
        VOP3bC,   // VOP3 version of VOPC
        VOPC,
        VINTRP,
        DS,
        EXP,
        FLAT,
        MUBUF,
        MTBUF,
        MIMG 
    }

    /// <summary>
    /// Identifies the allowed types for an operand. For example an operand might be allowed to be an SGPR, VCC_LO, or VCC_HI.
    /// </summary>
    [Flags]
    public enum OpType
    {
        /// <summary>[0-103] GPRs.</summary>
        SGPR = (1 << 0),
        /// <summary>[104] Holds the low DWord of the flat-scratch memory descriptor.</summary>
        FLAT_SCR_LO = (1 << 2),
        /// <summary>[105] Holds the high DWord of the flat-scratch memory descriptor.</summary>
        FLAT_SCR_HI = (1 << 3),
        /// <summary>[106] VCC[31:0].</summary>
        VCC_LO = (1 << 4),
        /// <summary>[107] VCC[63:32].</summary>
        VCC_HI = (1 << 5),
        /// <summary>[108] Trap handler base address, [31:0].</summary>
        TBA_LO = (1 << 6),
        /// <summary>[109] Trap handler base address, [63:32].</summary>
        TBA_HI = (1 << 7),
        /// <summary>[110] Pointer to data in memory used by trap handler.</summary>
        TMA_LO = (1 << 8),
        /// <summary>[111] Pointer to data in memory used by trap handler.</summary>
        TMA_HI = (1 << 9),
        /// <summary>[112-123] ttmp0 to ttmp11; Trap handler temps (privileged). {ttmp1,ttmp0} = PC_save{hi,lo}.</summary>
        TTMP = (1 << 11),
        /// <summary>[124] Temporary memory register.</summary>
        M0 = (1 << 12),
        /// <summary>[126] exec[31:0].</summary>
        EXEC_LO = (1 << 14),
        /// <summary>[127] exec[63:32].</summary>
        EXEC_HI = (1 << 15),
        /// <summary>[128] Immediate (constant value 0).</summary>
        ZERO = (1 << 17),
        /// <summary>[129-192] 1 to 64 Positive integer values.</summary>
        INLINE_INT_POS = (1 << 18),
        /// <summary>[193-208] -1 to -16 Negative integer values.</summary>
        INLINE_INT_NEG = (1 << 19),
        /// <summary>[240,242,244,246] 240=0.5 242=1.0 244=2.0 246=4.0</summary>
        INLINE_FLOAT_POS = (1 << 20),
        /// <summary>[241,243,245,247] 241=-0.5 243=-1.0 245=-2.0 247=-4.0</summary>
        INLINE_FLOAT_NEG = (1 << 21),
        /// <summary>[251] { zeros, VCCZ }</summary>
        VCCZ = (1 << 22),
        /// <summary>[252] { zeros, EXECZ } </summary>
        EXECZ = (1 << 23),
        /// <summary>[253] { zeros, SCC } </summary>
        SCC = (1 << 24),
        /// <summary>[254] LDS Direct</summary>
        LDS_DIRECT = (1 << 25),
        /// <summary>[255] constant 32-bit constant from instruction stream.</summary>
        LITERAL = (1 << 26),
        /// <summary>[256-511]</summary>
        VGPR = (1 << 27),
        /// <summary>[125,248,249,250] Reserved by AMD</summary>
        Reserved = (1 << 29),

        ///////////////// REGISTER TYPE CLASSES ///////////////////
        /// <summary>[104-105] FLAT_SCR_LO | FLAT_SCR_HI</summary>
        FLAT_SCR = FLAT_SCR_LO | FLAT_SCR_HI,
        /// <summary>[106-107] VCC_LO | VCC_HI</summary>
        VCC = VCC_LO | VCC_HI,
        /// <summary>[108-109] TBA_LO | TBA_HI,</summary>
        TBA = TBA_LO | TBA_HI,
        /// <summary>[110-111] TMA_LO | TMA_HI</summary>
        TMA = TMA_LO | TMA_HI,
        /// <summary>[126-127] EXEC_LO | EXEC_HI</summary>
        EXEC = EXEC_LO | EXEC_HI,
        /// <summary>[128-208] ZERO | INLINE_INT_POS | INLINE_INT_NEG</summary>
        INLINE_INT = ZERO | INLINE_INT_POS | INLINE_INT_NEG,
        /// <summary>[128-192] ZERO | INLINE_INT_POS</summary>
        INLINE_UINT = ZERO | INLINE_INT_POS,
        /// <summary>[128,240-247] ZERO | INLINE_FLOAT_POS | INLINE_FLOAT_NEG</summary>
        INLINE_FLOAT = ZERO | INLINE_FLOAT_POS | INLINE_FLOAT_NEG,
        /// <summary>[128,240-247] ZERO | INLINE_INT_POS | INLINE_INT_NEG | INLINE_FLOAT_POS | INLINE_FLOAT_NEG</summary>
        INLINE = ZERO | INLINE_INT_POS | INLINE_INT_NEG | INLINE_FLOAT_POS | INLINE_FLOAT_NEG,


        /// <summary>VOP3 source choices(SCALAR_DST, INLINE, VCCZ, EXECZ, SCC, LDS_DIRECT or VGPR)</summary>
        TRAP = TBA | TMA | TTMP,
        /// <summary>any INLINE_INT or INLINE_FLOAT or LITERAL</summary>
        CONST = INLINE_INT | INLINE_FLOAT | LITERAL,
        /// <summary>[0-127] SGPR | FLAT_SCR | VCC | TBA | TMA | TTMP | M0 | EXEC</summary>
        SCALAR_DST = SGPR | FLAT_SCR | VCC | TBA | TMA | TTMP | M0 | EXEC,
        /// <summary>[0-255]</summary>
        SCALAR_SRC = SCALAR_DST | INLINE | VCCZ | EXECZ | SCC | LDS_DIRECT | LITERAL,
        /// <summary>VOP3 source choices(SCALAR_DST, INLINE, VCCZ, EXECZ, SCC, LDS_DIRECT or VGPR)</summary>
        VOP3_SRC =   SCALAR_DST | INLINE | VCCZ | EXECZ | SCC | LDS_DIRECT | VGPR,
        /// <summary>[0-511] SCALAR_SRC | VGPR</summary>
        ALL = SCALAR_SRC | VGPR
    }


    /// <summary>
    /// This class contains the raw data for AMD's GCN ISA(Instruction Set Architecture) such as instruction details, 
    /// reserved words, register aliases,
    /// </summary>
    public static class ISA_DATA
    {
        /// <summary>
        /// These are keywords found in GCN asm. This list does not include sRegAliases for example VCC or EXEC_LO.
        /// </summary>
        public static readonly String[] AsmReserveDWords = { "abs", "addr64", "clamp", "da", "dmask", "float", "format", 
                                                               "gds", "glc", "idxen", "lwe", "mul", "neg", "offen", 
                                                               "offset", "offset0", "offset1", "omod", "p0", "p10", "p20", "r128", "sint", 
                                                               "slc", "snorm", "snorm_ogl", "sscaled", "tfe", "uint", 
                                                               "unorm", "uscaled", "vm_cnt", "vm_count", "vm_ct", "vmcnt", 
                                                               "vmcount", "vmct" };

        /// <summary>
        /// This is a Dictionary of available instructions and all there information. This information is mainly from 
        /// AMD's GCN ISA documentation.
        /// </summary>
        static public readonly Dictionary<string, InstInfo> isa_inst_dic;

        /// <summary>
        /// This is a Dictionary of friendly names given to special register numbers. (e.g. vcc_hi is typically reg 107)
        /// </summary>
        static public readonly Dictionary<string, OpInfo> sRegAliases;
        static ISA_DATA()
        {
            sRegAliases = new Dictionary<string, OpInfo> {
                        {"flatscr", new OpInfo { reg = 104, value = 104, flags = OpType.FLAT_SCR }},
                        {"flatscr_lo", new OpInfo { reg = 104, value = 104, flags = OpType.FLAT_SCR_LO }},
                        {"flatscr_hi", new OpInfo { reg = 105, value = 105, flags = OpType.FLAT_SCR_HI }},
                        {"vcc", new OpInfo { reg = 106, value = 106, flags = OpType.VCC }},
                        {"vcc_lo", new OpInfo { reg = 106, value = 106, flags = OpType.VCC_LO }},
                        {"vcc_hi", new OpInfo { reg = 107, value = 107, flags = OpType.VCC_HI }},
                        {"tba", new OpInfo { reg = 108, value = 108, flags = OpType.TBA }},
                        {"tba_lo", new OpInfo { reg = 108, value = 108, flags = OpType.TBA_LO }},
                        {"tba_hi", new OpInfo { reg = 109, value = 109, flags = OpType.TBA_HI }},
                        {"tma", new OpInfo { reg = 110, value = 110, flags = OpType.TMA }},
                        {"tma_lo", new OpInfo { reg = 110, value = 110, flags = OpType.TMA_LO }},
                        {"tma_hi", new OpInfo { reg = 111, value = 111, flags = OpType.TMA_HI }},
                        {"t0", new OpInfo { reg = 112, value = 112, flags = OpType.TTMP }},
                        {"t1", new OpInfo { reg = 113, value = 113, flags = OpType.TTMP }},
                        {"t2", new OpInfo { reg = 114, value = 114, flags = OpType.TTMP }},
                        {"t3", new OpInfo { reg = 115, value = 115, flags = OpType.TTMP }},
                        {"t4", new OpInfo { reg = 116, value = 116, flags = OpType.TTMP }},
                        {"t5", new OpInfo { reg = 117, value = 117, flags = OpType.TTMP }},
                        {"t6", new OpInfo { reg = 118, value = 118, flags = OpType.TTMP }},
                        {"t7", new OpInfo { reg = 119, value = 119, flags = OpType.TTMP }},
                        {"t8", new OpInfo { reg = 120, value = 120, flags = OpType.TTMP }},
                        {"t9", new OpInfo { reg = 121, value = 121, flags = OpType.TTMP }},
                        {"t10", new OpInfo { reg = 122, value = 122, flags = OpType.TTMP }},
                        {"t11", new OpInfo { reg = 123, value = 123, flags = OpType.TTMP }},
                        {"ttmp0", new OpInfo { reg = 112, value = 112, flags = OpType.TTMP }},
                        {"ttmp1", new OpInfo { reg = 113, value = 113, flags = OpType.TTMP }},
                        {"ttmp2", new OpInfo { reg = 114, value = 114, flags = OpType.TTMP }},
                        {"ttmp3", new OpInfo { reg = 115, value = 115, flags = OpType.TTMP }},
                        {"ttmp4", new OpInfo { reg = 116, value = 116, flags = OpType.TTMP }},
                        {"ttmp5", new OpInfo { reg = 117, value = 117, flags = OpType.TTMP }},
                        {"ttmp6", new OpInfo { reg = 118, value = 118, flags = OpType.TTMP }},
                        {"ttmp7", new OpInfo { reg = 119, value = 119, flags = OpType.TTMP }},
                        {"ttmp8", new OpInfo { reg = 120, value = 120, flags = OpType.TTMP }},
                        {"ttmp9", new OpInfo { reg = 121, value = 121, flags = OpType.TTMP }},
                        {"ttmp10", new OpInfo { reg = 122, value = 122, flags = OpType.TTMP }},
                        {"ttmp11", new OpInfo { reg = 123, value = 123, flags = OpType.TTMP }},
                        {"m0", new OpInfo { reg = 124, value = 124, flags = OpType.M0 }},
                        {"exec", new OpInfo { reg = 126, value = 126, flags = OpType.EXEC }},
                        {"exec_lo", new OpInfo { reg = 126, value = 126, flags = OpType.EXEC_LO }},
                        {"exec_hi", new OpInfo { reg = 127, value = 127, flags = OpType.EXEC_HI }},
                        {"0.0", new OpInfo { reg = 128, value = 128, flags = OpType.ZERO }},
                        {"0", new OpInfo { reg = 128, value = 128, flags = OpType.ZERO }},
                        {"1", new OpInfo { reg = 129, value = 129, flags = OpType.INLINE_INT_POS }},
                        {"2", new OpInfo { reg = 130, value = 130, flags = OpType.INLINE_INT_POS }},
                        {"3", new OpInfo { reg = 131, value = 131, flags = OpType.INLINE_INT_POS }},
                        {"4", new OpInfo { reg = 132, value = 132, flags = OpType.INLINE_INT_POS }},
                        {"5", new OpInfo { reg = 133, value = 133, flags = OpType.INLINE_INT_POS }},
                        {"6", new OpInfo { reg = 134, value = 134, flags = OpType.INLINE_INT_POS }},
                        {"7", new OpInfo { reg = 135, value = 135, flags = OpType.INLINE_INT_POS }},
                        {"8", new OpInfo { reg = 136, value = 136, flags = OpType.INLINE_INT_POS }},
                        {"9", new OpInfo { reg = 137, value = 137, flags = OpType.INLINE_INT_POS }},
                        {"10", new OpInfo { reg = 138, value = 138, flags = OpType.INLINE_INT_POS }},
                        {"11", new OpInfo { reg = 139, value = 139, flags = OpType.INLINE_INT_POS }},
                        {"12", new OpInfo { reg = 140, value = 140, flags = OpType.INLINE_INT_POS }},
                        {"13", new OpInfo { reg = 141, value = 141, flags = OpType.INLINE_INT_POS }},
                        {"14", new OpInfo { reg = 142, value = 142, flags = OpType.INLINE_INT_POS }},
                        {"15", new OpInfo { reg = 143, value = 143, flags = OpType.INLINE_INT_POS }},
                        {"16", new OpInfo { reg = 144, value = 144, flags = OpType.INLINE_INT_POS }},
                        {"17", new OpInfo { reg = 145, value = 145, flags = OpType.INLINE_INT_POS }},
                        {"18", new OpInfo { reg = 146, value = 146, flags = OpType.INLINE_INT_POS }},
                        {"19", new OpInfo { reg = 147, value = 147, flags = OpType.INLINE_INT_POS }},
                        {"20", new OpInfo { reg = 148, value = 148, flags = OpType.INLINE_INT_POS }},
                        {"21", new OpInfo { reg = 149, value = 149, flags = OpType.INLINE_INT_POS }},
                        {"22", new OpInfo { reg = 150, value = 150, flags = OpType.INLINE_INT_POS }},
                        {"23", new OpInfo { reg = 151, value = 151, flags = OpType.INLINE_INT_POS }},
                        {"24", new OpInfo { reg = 152, value = 152, flags = OpType.INLINE_INT_POS }},
                        {"25", new OpInfo { reg = 153, value = 153, flags = OpType.INLINE_INT_POS }},
                        {"26", new OpInfo { reg = 154, value = 154, flags = OpType.INLINE_INT_POS }},
                        {"27", new OpInfo { reg = 155, value = 155, flags = OpType.INLINE_INT_POS }},
                        {"28", new OpInfo { reg = 156, value = 156, flags = OpType.INLINE_INT_POS }},
                        {"29", new OpInfo { reg = 157, value = 157, flags = OpType.INLINE_INT_POS }},
                        {"30", new OpInfo { reg = 158, value = 158, flags = OpType.INLINE_INT_POS }},
                        {"31", new OpInfo { reg = 159, value = 159, flags = OpType.INLINE_INT_POS }},
                        {"32", new OpInfo { reg = 160, value = 160, flags = OpType.INLINE_INT_POS }},
                        {"33", new OpInfo { reg = 161, value = 161, flags = OpType.INLINE_INT_POS }},
                        {"34", new OpInfo { reg = 162, value = 162, flags = OpType.INLINE_INT_POS }},
                        {"35", new OpInfo { reg = 163, value = 163, flags = OpType.INLINE_INT_POS }},
                        {"36", new OpInfo { reg = 164, value = 164, flags = OpType.INLINE_INT_POS }},
                        {"37", new OpInfo { reg = 165, value = 165, flags = OpType.INLINE_INT_POS }},
                        {"38", new OpInfo { reg = 166, value = 166, flags = OpType.INLINE_INT_POS }},
                        {"39", new OpInfo { reg = 167, value = 167, flags = OpType.INLINE_INT_POS }},
                        {"40", new OpInfo { reg = 168, value = 168, flags = OpType.INLINE_INT_POS }},
                        {"41", new OpInfo { reg = 169, value = 169, flags = OpType.INLINE_INT_POS }},
                        {"42", new OpInfo { reg = 170, value = 170, flags = OpType.INLINE_INT_POS }},
                        {"43", new OpInfo { reg = 171, value = 171, flags = OpType.INLINE_INT_POS }},
                        {"44", new OpInfo { reg = 172, value = 172, flags = OpType.INLINE_INT_POS }},
                        {"45", new OpInfo { reg = 173, value = 173, flags = OpType.INLINE_INT_POS }},
                        {"46", new OpInfo { reg = 174, value = 174, flags = OpType.INLINE_INT_POS }},
                        {"47", new OpInfo { reg = 175, value = 175, flags = OpType.INLINE_INT_POS }},
                        {"48", new OpInfo { reg = 176, value = 176, flags = OpType.INLINE_INT_POS }},
                        {"49", new OpInfo { reg = 177, value = 177, flags = OpType.INLINE_INT_POS }},
                        {"50", new OpInfo { reg = 178, value = 178, flags = OpType.INLINE_INT_POS }},
                        {"51", new OpInfo { reg = 179, value = 179, flags = OpType.INLINE_INT_POS }},
                        {"52", new OpInfo { reg = 180, value = 180, flags = OpType.INLINE_INT_POS }},
                        {"53", new OpInfo { reg = 181, value = 181, flags = OpType.INLINE_INT_POS }},
                        {"54", new OpInfo { reg = 182, value = 182, flags = OpType.INLINE_INT_POS }},
                        {"55", new OpInfo { reg = 183, value = 183, flags = OpType.INLINE_INT_POS }},
                        {"56", new OpInfo { reg = 184, value = 184, flags = OpType.INLINE_INT_POS }},
                        {"57", new OpInfo { reg = 185, value = 185, flags = OpType.INLINE_INT_POS }},
                        {"58", new OpInfo { reg = 186, value = 186, flags = OpType.INLINE_INT_POS }},
                        {"59", new OpInfo { reg = 187, value = 187, flags = OpType.INLINE_INT_POS }},
                        {"60", new OpInfo { reg = 188, value = 188, flags = OpType.INLINE_INT_POS }},
                        {"61", new OpInfo { reg = 189, value = 189, flags = OpType.INLINE_INT_POS }},
                        {"62", new OpInfo { reg = 190, value = 190, flags = OpType.INLINE_INT_POS }},
                        {"63", new OpInfo { reg = 191, value = 191, flags = OpType.INLINE_INT_POS }},
                        {"64", new OpInfo { reg = 192, value = 192, flags = OpType.INLINE_INT_POS }},
                        {"-1", new OpInfo { reg = 193, value = 193, flags = OpType.INLINE_INT_NEG }},
                        {"-2", new OpInfo { reg = 194, value = 194, flags = OpType.INLINE_INT_NEG }},
                        {"-3", new OpInfo { reg = 195, value = 195, flags = OpType.INLINE_INT_NEG }},
                        {"-4", new OpInfo { reg = 196, value = 196, flags = OpType.INLINE_INT_NEG }},
                        {"-5", new OpInfo { reg = 197, value = 197, flags = OpType.INLINE_INT_NEG }},
                        {"-6", new OpInfo { reg = 198, value = 198, flags = OpType.INLINE_INT_NEG }},
                        {"-7", new OpInfo { reg = 199, value = 199, flags = OpType.INLINE_INT_NEG }},
                        {"-8", new OpInfo { reg = 200, value = 200, flags = OpType.INLINE_INT_NEG }},
                        {"-9", new OpInfo { reg = 201, value = 201, flags = OpType.INLINE_INT_NEG }},
                        {"-10", new OpInfo { reg = 202, value = 202, flags = OpType.INLINE_INT_NEG }},
                        {"-11", new OpInfo { reg = 203, value = 203, flags = OpType.INLINE_INT_NEG }},
                        {"-12", new OpInfo { reg = 204, value = 204, flags = OpType.INLINE_INT_NEG }},
                        {"-13", new OpInfo { reg = 205, value = 205, flags = OpType.INLINE_INT_NEG }},
                        {"-14", new OpInfo { reg = 206, value = 206, flags = OpType.INLINE_INT_NEG }},
                        {"-15", new OpInfo { reg = 207, value = 207, flags = OpType.INLINE_INT_NEG }},
                        {"-16", new OpInfo { reg = 208, value = 208, flags = OpType.INLINE_INT_NEG }},
                        {"0.5", new OpInfo { reg = 240, value = 240, flags = OpType.INLINE_FLOAT_POS }},
                        {"-0.5", new OpInfo { reg = 241, value = 241, flags = OpType.INLINE_FLOAT_NEG }},
                        {"1.0", new OpInfo { reg = 242, value = 242, flags = OpType.INLINE_FLOAT_POS }},
                        {"-1.0", new OpInfo { reg = 243, value = 243, flags = OpType.INLINE_FLOAT_NEG }},
                        {"2.0", new OpInfo { reg = 244, value = 244, flags = OpType.INLINE_FLOAT_POS }},
                        {"-2.0", new OpInfo { reg = 245, value = 245, flags = OpType.INLINE_FLOAT_NEG }},
                        {"4.0", new OpInfo { reg = 246, value = 246, flags = OpType.INLINE_FLOAT_POS }},
                        {"-4.0", new OpInfo { reg = 247, value = 247, flags = OpType.INLINE_FLOAT_NEG }},
                        {".5", new OpInfo { reg = 240, value = 240, flags = OpType.INLINE_FLOAT_POS }},
                        {"-.5", new OpInfo { reg = 241, value = 241, flags = OpType.INLINE_FLOAT_NEG }},
                        {"1.", new OpInfo { reg = 242, value = 242, flags = OpType.INLINE_FLOAT_POS }},
                        {"-1.", new OpInfo { reg = 243, value = 243, flags = OpType.INLINE_FLOAT_NEG }},
                        {"2.", new OpInfo { reg = 244, value = 244, flags = OpType.INLINE_FLOAT_POS }},
                        {"-2.", new OpInfo { reg = 245, value = 245, flags = OpType.INLINE_FLOAT_NEG }},
                        {"4.", new OpInfo { reg = 246, value = 246, flags = OpType.INLINE_FLOAT_POS }},
                        {"-4.", new OpInfo { reg = 247, value = 247, flags = OpType.INLINE_FLOAT_NEG }},
                        {"vccz", new OpInfo { reg = 251, value = 251, flags = OpType.VCCZ }},
                        {"execz", new OpInfo { reg = 252, value = 252, flags = OpType.EXECZ }},
                        {"scc", new OpInfo { reg = 253, value = 253, flags = OpType.SCC }},
                        {"lds_direct", new OpInfo { reg = 254, value = 254, flags = OpType.LDS_DIRECT }}};


            // Setup inst dictionary for fast lookups
            isa_inst_dic = new Dictionary<string, InstInfo>(ISA_DATA.ISA_Insts.Count);
            foreach (var instr in ISA_DATA.ISA_Insts)
                isa_inst_dic.Add(instr.name, instr);
        }

        /// <summary>
        /// Converts a register number to an OpType. (e.g. Registers >255 are typically VGPR registers)
        /// </summary>
        public static OpType GetFlagsFromRegNum(uint reg, Log log)
        {
            if (reg < 104)
                return OpType.SGPR;
            else if (reg > 255)
                return OpType.VGPR;
            else if (reg >= 129 && reg <= 208)
                return (reg <= 192) ? OpType.INLINE_INT_POS : OpType.INLINE_INT_NEG;

            switch (reg)
            {
                case 104: return OpType.FLAT_SCR_LO;
                case 105: return OpType.FLAT_SCR_HI;
                case 106: return OpType.VCC_LO;
                case 107: return OpType.VCC_HI;
                case 108: return OpType.TBA_LO;
                case 109: return OpType.TBA_HI;
                case 110: return OpType.TMA_LO;
                case 111: return OpType.TMA_HI;
                case 112: return OpType.TMA_LO;
                case 113: return OpType.TMA_HI;
                case 114: return OpType.TTMP;
                case 115: return OpType.TTMP;
                case 116: return OpType.TTMP;
                case 117: return OpType.TTMP;
                case 118: return OpType.TTMP;
                case 119: return OpType.TTMP;
                case 120: return OpType.TTMP;
                case 121: return OpType.TTMP;
                case 122: return OpType.TTMP;
                case 123: return OpType.TTMP;
                case 124: return OpType.M0;
                case 125: return OpType.Reserved;
                case 126: return OpType.EXEC_LO;
                case 127: return OpType.EXEC_HI;
                case 128: return OpType.ZERO;
                case 240: return OpType.INLINE_FLOAT_POS;
                case 241: return OpType.INLINE_FLOAT_NEG;
                case 242: return OpType.INLINE_FLOAT_POS;
                case 243: return OpType.INLINE_FLOAT_NEG;
                case 244: return OpType.INLINE_FLOAT_POS;
                case 245: return OpType.INLINE_FLOAT_NEG;
                case 246: return OpType.INLINE_FLOAT_POS;
                case 247: return OpType.INLINE_FLOAT_NEG;
                case 248: return OpType.Reserved;
                case 249: return OpType.Reserved;
                case 250: return OpType.Reserved;
                case 251: return OpType.VCCZ;
                case 252: return OpType.EXECZ;
                case 253: return OpType.SCC;
                case 254: return OpType.LDS_DIRECT;
                case 255: return OpType.LITERAL;
                default: log.Error("Unknown value '{0}' in GetFlagsFromRegNum()", reg); return OpType.Reserved;
            }
        }

        /// <summary>
        /// Contains all the instructions from the AMD Sea Islands ISA Manual PDF
        /// Source: Much of the information is from AMD's Sea Island ISA Guide (Feb 2013, PDF) 
        /// </summary>
        public static readonly IList<InstInfo> ISA_Insts = new ReadOnlyCollection<InstInfo>(new[] {

new InstInfo(0000, "buffer_atomic_add", "v4[iu]", "v4[iu]", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst += src. Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 50, 0, 0xE0C80000, 0x0003),
new InstInfo(0001, "buffer_atomic_add_x2", "v4[iu]", "v4[iu]", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b, dst += src. Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 82, 0, 0xE1480000, 0x0003),
new InstInfo(0002, "buffer_atomic_and", "v4b", "v4b", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst &= src. Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 57, 0, 0xE0E40000, 0x0003),
new InstInfo(0003, "buffer_atomic_and_x2", "v4b", "v4b", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b, dst &= src. Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 89, 0, 0xE1640000, 0x0003),
new InstInfo(0004, "buffer_atomic_cmpswap", "v4[biu]", "v4[biu]", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst = (dst==cmp) ? src : dst. Returns previous value if glc==1. src comes from the first data-vgpr, cmp from the second. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 49, 0, 0xE0C40000, 0x0003),
new InstInfo(0005, "buffer_atomic_cmpswap_x2", "v8[biu]", "v8[biu]", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b, dst = (dst==cmp) ? src : dst. Returns previous value if glc==1. src comes from the first two data-vgprs, cmp from the second two. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 81, 0, 0xE1440000, 0x0003),
new InstInfo(0006, "buffer_atomic_dec", "v4[iu]", "v4[iu]", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst = ((dst==0 || (dst > src)) ? src : dst-1. Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 61, 0, 0xE0F40000, 0x0003),
new InstInfo(0007, "buffer_atomic_dec_x2", "v4[iu]", "v4[iu]", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b, dst = ((dst==0 || (dst > src)) ? src : dst-1. Returns previous value if glc==1. 93 5D0", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 93, 0, 0xE1740000, 0x0003),
new InstInfo(0008, "buffer_atomic_fcmpswap", "v4f", "v4f", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, , dst = (dst == cmp) ? src : dst, returns previous value if glc==1. Float compare swap (handles NaN/INF/denorm). src comes from the first data-vgpr; cmp from the second. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 62, 0, 0xE0F80000, 0x0003),
new InstInfo(0009, "buffer_atomic_fcmpswap_x2", "v4f", "v4f", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b, dst = (dst == cmp) ? src : dst, returns previous value if glc==1. Double compare swap (handles NaN/INF/denorm). src comes from the first two data-vgprs, cmp from the second two. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 94, 0, 0xE1780000, 0x0003),
new InstInfo(0010, "buffer_atomic_fmax", "v4f", "v4f", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst = (src > dst) ? src : dst, returns previous value if glc==1. float, handles NaN/INF/denorm. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 64, 0, 0xE1000000, 0x0003),
new InstInfo(0011, "buffer_atomic_fmax_x2", "v4f", "v4f", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b,  dst = (src > dst) ? src : dst, returns previous value if glc==1. Double, handles NaN/INF/denorm. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 96, 0, 0xE1800000, 0x0003),
new InstInfo(0012, "buffer_atomic_fmin", "v4f", "v4f", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst = (src < dst) ? src : dst,. Returns previous value if glc==1. float, handles NaN/INF/denorm. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 63, 0, 0xE0FC0000, 0x0003),
new InstInfo(0013, "buffer_atomic_fmin_x2", "v4f", "v4f", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b,  dst = (src < dst) ? src : dst, returns previous value if glc==1. Double, handles NaN/INF/denorm. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 95, 0, 0xE17C0000, 0x0003),
new InstInfo(0014, "buffer_atomic_inc", "v4[iu]", "v4[iu]", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst = (dst >= src) ? 0 : dst+1.Returns previous value if glc==1.", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 60, 0, 0xE0F00000, 0x0003),
new InstInfo(0015, "buffer_atomic_inc_x2", "v4[iu]", "v4[iu]", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b, dst = (dst >= src) ? 0 : dst+1.Returns previous value if glc==1.", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 92, 0, 0xE1700000, 0x0003),
new InstInfo(0016, "buffer_atomic_or", "v4b", "v4b", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst |= src. Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 58, 0, 0xE0E80000, 0x0003),
new InstInfo(0017, "buffer_atomic_or_x2", "v4b", "v4b", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b, dst |= src. Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 90, 0, 0xE1680000, 0x0003),
new InstInfo(0018, "buffer_atomic_smax", "v4i", "v4i", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst = (src > dst) ? src : dst (signed). Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 55, 0, 0xE0DC0000, 0x0003),
new InstInfo(0019, "buffer_atomic_smax_x2", "v4i", "v4i", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b, dst = (src > dst) ? src : dst (signed). Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 87, 0, 0xE15C0000, 0x0003),
new InstInfo(0020, "buffer_atomic_smin", "v4i", "v4i", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst = (src < dst) ? src : dst (signed). Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 53, 0, 0xE0D40000, 0x0003),
new InstInfo(0021, "buffer_atomic_smin_x2", "v4i", "v4i", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b, dst = (src < dst) ? src : dst (signed). Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 85, 0, 0xE1540000, 0x0003),
new InstInfo(0022, "buffer_atomic_sub", "v4[iu]", "v4[iu]", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst -= src. Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 51, 0, 0xE0CC0000, 0x0003),
new InstInfo(0023, "buffer_atomic_sub_x2", "v4[iu]", "v4[iu]", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b, dst -= src. Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 83, 0, 0xE14C0000, 0x0003),
new InstInfo(0024, "buffer_atomic_swap", "v4b", "v4b", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst=src, returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 48, 0, 0xE0C00000, 0x0003),
new InstInfo(0025, "buffer_atomic_swap_x2", "v8b", "v8b", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b, dst=src, returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 80, 0, 0xE1400000, 0x0003),
new InstInfo(0026, "buffer_atomic_umax", "v4u", "v4u", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst = (src > dst) ? src : dst (unsigned). Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 56, 0, 0xE0E00000, 0x0003),
new InstInfo(0027, "buffer_atomic_umax_x2", "v4u", "v4u", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b, dst = (src > dst) ? src : dst (unsigned). Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 88, 0, 0xE1600000, 0x0003),
new InstInfo(0028, "buffer_atomic_umin", "v4u", "v4u", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst = (src < dst) ? src : dst (unsigned). Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 54, 0, 0xE0D80000, 0x0003),
new InstInfo(0029, "buffer_atomic_umin_x2", "v4u", "v4u", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b, dst = (src < dst) ? src : dst (unsigned). Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 86, 0, 0xE1580000, 0x0003),
new InstInfo(0030, "buffer_atomic_xor", "v4b", "v4b", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"32b, dst ^= src. Returns previous value if glc==1. ", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 59, 0, 0xE0EC0000, 0x0003),
new InstInfo(0031, "buffer_atomic_xor_x2", "v4b", "v4b", "v4i", "s16b", "none", "16u", "24u", 6, 6, @"64b, dst ^= src. Returns previous value if glc==1.", @"Dst and Src0 might need to be the same", ISA_Enc.MUBUF, 91, 0, 0xE16C0000, 0x0003),
new InstInfo(0032, "buffer_load_dword", "v4b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Untyped buffer load Dword. ", @"", ISA_Enc.MUBUF, 12, 0, 0xE0300000, 0x0003),
new InstInfo(0033, "buffer_load_dwordx2", "v8b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Untyped buffer load 2 Dwords. ", @"", ISA_Enc.MUBUF, 13, 0, 0xE0340000, 0x0003),
new InstInfo(0034, "buffer_load_dwordx3", "v12b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Untyped buffer load 3 Dwords. ", @"", ISA_Enc.MUBUF, 15, 0, 0xE03C0000, 0x0003),
new InstInfo(0035, "buffer_load_dwordx4", "v16b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Untyped buffer load 4 Dwords. ", @"", ISA_Enc.MUBUF, 14, 0, 0xE0380000, 0x0003),
new InstInfo(0036, "buffer_load_format_x", "v4b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Untyped buffer load 1 Dword with format conversion. ", @"", ISA_Enc.MUBUF, 0, 0, 0xE0000000, 0x0001),
new InstInfo(0037, "buffer_load_format_xy", "v8b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Untyped buffer load 2 Dwords with format conversion. ", @"", ISA_Enc.MUBUF, 1, 0, 0xE0040000, 0x0001),
new InstInfo(0038, "buffer_load_format_xyz", "v12b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Untyped buffer load 3 Dwords with format conversion. ", @"", ISA_Enc.MUBUF, 2, 0, 0xE0080000, 0x0001),
new InstInfo(0039, "buffer_load_format_xyzw", "v16b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Untyped buffer load 4 Dwords with format conversion. ", @"", ISA_Enc.MUBUF, 3, 0, 0xE00C0000, 0x0001),
new InstInfo(0040, "buffer_load_sbyte", "v4b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Untyped buffer load signed byte. ", @"", ISA_Enc.MUBUF, 9, 0, 0xE0240000, 0x0001),
new InstInfo(0041, "buffer_load_sshort", "v4b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Untyped buffer load signed short. ", @"", ISA_Enc.MUBUF, 11, 0, 0xE02C0000, 0x0003),
new InstInfo(0042, "buffer_load_ubyte", "v4b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Untyped buffer load unsigned byte. ", @"", ISA_Enc.MUBUF, 8, 0, 0xE0200000, 0x0001),
new InstInfo(0043, "buffer_load_ushort", "v4b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Untyped buffer load unsigned short. ", @"", ISA_Enc.MUBUF, 10, 0, 0xE0280000, 0x0003),
new InstInfo(0044, "buffer_store_byte", "none", "v4i", "v4i", "s16b", "none", "16u", "24u", 5, 5, @"Untyped buffer store byte. ", @"", ISA_Enc.MUBUF, 24, 0, 0xE0600000, 0x0003),
new InstInfo(0045, "buffer_store_dword", "none", "v4i", "v4i", "s16b", "none", "16u", "24u", 5, 5, @"Untyped buffer store Dword. ", @"", ISA_Enc.MUBUF, 28, 0, 0xE0700000, 0x0003),
new InstInfo(0046, "buffer_store_dwordx2", "none", "v4i", "v4i", "s16b", "none", "16u", "24u", 5, 5, @"Untyped buffer store 2 Dwords. ", @"", ISA_Enc.MUBUF, 29, 0, 0xE0740000, 0x0003),
new InstInfo(0047, "buffer_store_dwordx3", "none", "v4i", "v4i", "s16b", "none", "16u", "24u", 5, 5, @"Untyped buffer store 3 Dwords. ", @"", ISA_Enc.MUBUF, 31, 0, 0xE07C0000, 0x0003),
new InstInfo(0048, "buffer_store_dwordx4", "none", "v4i", "v4i", "s16b", "none", "16u", "24u", 5, 5, @"Untyped buffer store 4 Dwords. ", @"", ISA_Enc.MUBUF, 30, 0, 0xE0780000, 0x0003),
new InstInfo(0049, "buffer_store_format_x", "none", "v4i", "v4i", "s16b", "none", "16u", "24u", 5, 5, @"Untyped buffer store 1 Dword with format conversion. ", @"", ISA_Enc.MUBUF, 4, 0, 0xE0100000, 0x0001),
new InstInfo(0050, "buffer_store_format_xy", "none", "v4i", "v4i", "s16b", "none", "16u", "24u", 5, 5, @"Untyped buffer store 2 Dwords with format conversion. ", @"", ISA_Enc.MUBUF, 5, 0, 0xE0140000, 0x0001),
new InstInfo(0051, "buffer_store_format_xyz", "none", "v4i", "v4i", "s16b", "none", "16u", "24u", 5, 5, @"Untyped buffer store 3 Dwords with format conversion. ", @"", ISA_Enc.MUBUF, 6, 0, 0xE0180000, 0x0001),
new InstInfo(0052, "buffer_store_format_xyzw", "none", "v4i", "v4i", "s16b", "none", "16u", "24u", 5, 5, @"Untyped buffer store 4 Dwords with format conversion. ", @"", ISA_Enc.MUBUF, 7, 0, 0xE01C0000, 0x0001),
new InstInfo(0053, "buffer_store_short", "none", "v4i", "v4i", "s16b", "none", "16u", "24u", 5, 5, @"Untyped buffer store short. ", @"", ISA_Enc.MUBUF, 26, 0, 0xE0680000, 0x0003),
new InstInfo(0054, "buffer_wbinvl1", "none", "none", "none", "none", "none", "none", "none", 0, 0, @"Write back and invalidate the shader L1 cache. Always returns ACK to shader. ", @"", ISA_Enc.MUBUF, 113, 0, 0xE1C40000, 0x0003),
new InstInfo(0055, "buffer_wbinvl1_vol", "none", "none", "none", "none", "none", "none", "none", 0, 0, @"Write back and invalidate the shader L1 cache only for lines of MTYPE SC and GC. Always returns ACK to shader. ", @"", ISA_Enc.MUBUF, 112, 0, 0xE1C00000, 0x0003),
new InstInfo(0056, "ds_add_rtn_u32", "v4u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint add.", @"", ISA_Enc.DS, 32, 0, 0xD8800000, 0x0003),
new InstInfo(0057, "ds_add_rtn_u64", "v8u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint add.", @"", ISA_Enc.DS, 96, 0, 0xD9800000, 0x0003),
new InstInfo(0058, "ds_add_src2_u32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = DS[A] + DS[B]; uint add.", @"", ISA_Enc.DS, 128, 0, 0xDA000000, 0x0003),
new InstInfo(0059, "ds_add_src2_u64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint add.", @"", ISA_Enc.DS, 192, 0, 0xDB000000, 0x0003),
new InstInfo(0060, "ds_add_u32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = DS[A] + D0; uint add.", @"", ISA_Enc.DS, 0, 0, 0xD8000000, 0x0003),
new InstInfo(0061, "ds_add_u64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint add.", @"", ISA_Enc.DS, 64, 0, 0xD9000000, 0x0003),
new InstInfo(0062, "ds_and_b32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = DS[A] & D0; Dword AND.", @"", ISA_Enc.DS, 9, 0, 0xD8240000, 0x0003),
new InstInfo(0063, "ds_and_b64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Dword AND.", @"", ISA_Enc.DS, 73, 0, 0xD9240000, 0x0003),
new InstInfo(0064, "ds_and_rtn_b32", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Dword AND.", @"", ISA_Enc.DS, 41, 0, 0xD8A40000, 0x0003),
new InstInfo(0065, "ds_and_rtn_b64", "v8b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Dword AND.", @"", ISA_Enc.DS, 105, 0, 0xD9A40000, 0x0003),
new InstInfo(0066, "ds_and_src2_b32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = DS[A] & DS[B]; Dword AND.", @"", ISA_Enc.DS, 137, 0, 0xDA240000, 0x0003),
new InstInfo(0067, "ds_and_src2_b64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Dword AND. ", @"", ISA_Enc.DS, 201, 0, 0xDB240000, 0x0003),
new InstInfo(0068, "ds_append", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Append one or more entries to a buffer.", @"", ISA_Enc.DS, 62, 0, 0xD8F80000, 0x0003),
new InstInfo(0069, "ds_cmpst_b32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = (DS[A] == D0 ? D1 : DS[A]); compare store.", @"", ISA_Enc.DS, 16, 0, 0xD8400000, 0x0003),
new InstInfo(0070, "ds_cmpst_b64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Compare store.", @"", ISA_Enc.DS, 80, 0, 0xD9400000, 0x0003),
new InstInfo(0071, "ds_cmpst_f32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = (DS[A] == D0 ? D1 : DS[A]); compare store with float rules.", @"", ISA_Enc.DS, 17, 0, 0xD8440000, 0x0003),
new InstInfo(0072, "ds_cmpst_f64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Compare store with float rules.", @"", ISA_Enc.DS, 81, 0, 0xD9440000, 0x0003),
new InstInfo(0073, "ds_cmpst_rtn_b32", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Compare store.", @"", ISA_Enc.DS, 48, 0, 0xD8C00000, 0x0003),
new InstInfo(0074, "ds_cmpst_rtn_b64", "v8b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Compare store.", @"", ISA_Enc.DS, 112, 0, 0xD9C00000, 0x0003),
new InstInfo(0075, "ds_cmpst_rtn_f32", "v4f", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Compare store with float rules.", @"", ISA_Enc.DS, 49, 0, 0xD8C40000, 0x0003),
new InstInfo(0076, "ds_cmpst_rtn_f64", "v8f", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Compare store with float rules.", @"", ISA_Enc.DS, 113, 0, 0xD9C40000, 0x0003),
new InstInfo(0077, "ds_condxchg32_rtn_b128", "v8b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Conditional write exchange.", @"", ISA_Enc.DS, 253, 0, 0xDBF40000, 0x0003),
new InstInfo(0078, "ds_condxchg32_rtn_b64", "v8b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Conditional write exchange. ", @"", ISA_Enc.DS, 126, 0, 0xD9F80000, 0x0003),
new InstInfo(0079, "ds_consume", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Consume entries from a buffer.", @"", ISA_Enc.DS, 61, 0, 0xD8F40000, 0x0003),
new InstInfo(0080, "ds_dec_rtn_u32", "v4u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint decrement.", @"", ISA_Enc.DS, 36, 0, 0xD8900000, 0x0003),
new InstInfo(0081, "ds_dec_rtn_u64", "v8u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint decrement.", @"", ISA_Enc.DS, 100, 0, 0xD9900000, 0x0003),
new InstInfo(0082, "ds_dec_src2_u32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = (DS[A] == 0 || DS[A] > DS[B] ? DS[B] : DS[A] - 1); uint decrement.", @"", ISA_Enc.DS, 132, 0, 0xDA100000, 0x0003),
new InstInfo(0083, "ds_dec_src2_u64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint decrement.", @"", ISA_Enc.DS, 196, 0, 0xDB100000, 0x0003),
new InstInfo(0084, "ds_dec_u32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = (DS[A] == 0 || DS[A] > D0 ? D0 : DS[A] - 1); uint decrement.", @"", ISA_Enc.DS, 4, 0, 0xD8100000, 0x0003),
new InstInfo(0085, "ds_dec_u64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint decrement.", @"", ISA_Enc.DS, 68, 0, 0xD9100000, 0x0003),
new InstInfo(0086, "ds_gws_barrier", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"GDS only.", @"", ISA_Enc.DS, 29, 0, 0xD8740000, 0x0003),
new InstInfo(0087, "ds_gws_init", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"GDS only.", @"", ISA_Enc.DS, 25, 0, 0xD8640000, 0x0003),
new InstInfo(0088, "ds_gws_sema_br", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"GDS only.", @"", ISA_Enc.DS, 27, 0, 0xD86C0000, 0x0003),
new InstInfo(0089, "ds_gws_sema_p", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"GDS only.", @"", ISA_Enc.DS, 28, 0, 0xD8700000, 0x0003),
new InstInfo(0090, "ds_gws_sema_release_all", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"GDS Only. Release all wavefronts waiting on this semaphore. ResourceID is in offset[4:0].", @"", ISA_Enc.DS, 24, 0, 0xD8600000, 0x0003),
new InstInfo(0091, "ds_gws_sema_v", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"GDS only.", @"", ISA_Enc.DS, 26, 0, 0xD8680000, 0x0003),
new InstInfo(0092, "ds_inc_rtn_u32", "v4u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint increment.", @"", ISA_Enc.DS, 35, 0, 0xD88C0000, 0x0003),
new InstInfo(0093, "ds_inc_rtn_u64", "v8u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint increment.", @"", ISA_Enc.DS, 99, 0, 0xD98C0000, 0x0003),
new InstInfo(0094, "ds_inc_src2_u32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = (DS[A] >= DS[B] ? 0 : DS[A] + 1); uint increment.", @"", ISA_Enc.DS, 131, 0, 0xDA0C0000, 0x0003),
new InstInfo(0095, "ds_inc_src2_u64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint increment.", @"", ISA_Enc.DS, 195, 0, 0xDB0C0000, 0x0003),
new InstInfo(0096, "ds_inc_u32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = (DS[A] >= D0 ? 0 : DS[A] + 1); uint increment.", @"", ISA_Enc.DS, 3, 0, 0xD80C0000, 0x0003),
new InstInfo(0097, "ds_inc_u64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint increment.", @"", ISA_Enc.DS, 67, 0, 0xD90C0000, 0x0003),
new InstInfo(0098, "ds_max_f32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = (D0 > DS[A]) ? D0 : DS[A]; float, handles NaN/INF/denorm.", @"", ISA_Enc.DS, 19, 0, 0xD84C0000, 0x0003),
new InstInfo(0099, "ds_max_f64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = (D0 > DS[A]) ? D0 : DS[A]; float, handles NaN/INF/denorm.", @"", ISA_Enc.DS, 83, 0, 0xD94C0000, 0x0003),
new InstInfo(0100, "ds_max_i32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = max(DS[A], D0); int max.", @"", ISA_Enc.DS, 6, 0, 0xD8180000, 0x0003),
new InstInfo(0101, "ds_max_i64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Int max.", @"", ISA_Enc.DS, 70, 0, 0xD9180000, 0x0003),
new InstInfo(0102, "ds_max_rtn_f32", "v4f", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = (D0 > DS[A]) ? D0 : DS[A]; float, handles NaN/INF/denorm.", @"", ISA_Enc.DS, 51, 0, 0xD8CC0000, 0x0003),
new InstInfo(0103, "ds_max_rtn_f64", "v8f", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = (D0 > DS[A]) ? D0 : DS[A]; float, handles NaN/INF/denorm.", @"", ISA_Enc.DS, 115, 0, 0xD9CC0000, 0x0003),
new InstInfo(0104, "ds_max_rtn_i32", "v4i", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Int max.", @"", ISA_Enc.DS, 38, 0, 0xD8980000, 0x0003),
new InstInfo(0105, "ds_max_rtn_i64", "v8i", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Int max.", @"", ISA_Enc.DS, 102, 0, 0xD9980000, 0x0003),
new InstInfo(0106, "ds_max_rtn_u32", "v4u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint max.", @"", ISA_Enc.DS, 40, 0, 0xD8A00000, 0x0003),
new InstInfo(0107, "ds_max_rtn_u64", "v8u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint max.", @"", ISA_Enc.DS, 104, 0, 0xD9A00000, 0x0003),
new InstInfo(0108, "ds_max_src2_f32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = (DS[B] > DS[A]) ? DS[B] : DS[A]; float, handles NaN/INF/denorm.", @"", ISA_Enc.DS, 147, 0, 0xDA4C0000, 0x0003),
new InstInfo(0109, "ds_max_src2_f64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}).  [A] = (D0 > DS[A]) ? D0 : DS[A]; float, handles NaN/INF/denorm.", @"", ISA_Enc.DS, 211, 0, 0xDB4C0000, 0x0003),
new InstInfo(0110, "ds_max_src2_i32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = max(DS[A], DS[B]); int max.", @"", ISA_Enc.DS, 134, 0, 0xDA180000, 0x0003),
new InstInfo(0111, "ds_max_src2_i64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Int max.", @"", ISA_Enc.DS, 198, 0, 0xDB180000, 0x0003),
new InstInfo(0112, "ds_max_src2_u32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = max(DS[A], DS[B]); uint maxw", @"", ISA_Enc.DS, 136, 0, 0xDA200000, 0x0003),
new InstInfo(0113, "ds_max_src2_u64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint max.", @"", ISA_Enc.DS, 200, 0, 0xDB200000, 0x0003),
new InstInfo(0114, "ds_max_u32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = max(DS[A], D0); uint max.", @"", ISA_Enc.DS, 8, 0, 0xD8200000, 0x0003),
new InstInfo(0115, "ds_max_u64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint max.", @"", ISA_Enc.DS, 72, 0, 0xD9200000, 0x0003),
new InstInfo(0116, "ds_min_f32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = (DS[A] < D1) ? D0 : DS[A]; float compare swap (handles NaN/INF/denorm).", @"", ISA_Enc.DS, 18, 0, 0xD8480000, 0x0003),
new InstInfo(0117, "ds_min_f64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = (D0 < DS[A]) ? D0 : DS[A]; float, handles NaN/INF/denorm.", @"", ISA_Enc.DS, 82, 0, 0xD9480000, 0x0003),
new InstInfo(0118, "ds_min_i32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = min(DS[A], D0); int min.", @"", ISA_Enc.DS, 5, 0, 0xD8140000, 0x0003),
new InstInfo(0119, "ds_min_i64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Int min.", @"", ISA_Enc.DS, 69, 0, 0xD9140000, 0x0003),
new InstInfo(0120, "ds_min_rtn_f32", "v4f", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = (DS[A] < D1) ? D0 : DS[A]; float compare swap (handles NaN/INF/denorm).", @"", ISA_Enc.DS, 50, 0, 0xD8C80000, 0x0003),
new InstInfo(0121, "ds_min_rtn_f64", "v8f", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = (D0 < DS[A]) ? D0 : DS[A]; float, handles NaN/INF/denorm.", @"", ISA_Enc.DS, 114, 0, 0xD9C80000, 0x0003),
new InstInfo(0122, "ds_min_rtn_i32", "v4i", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Int min.", @"", ISA_Enc.DS, 37, 0, 0xD8940000, 0x0003),
new InstInfo(0123, "ds_min_rtn_i64", "v8i", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Int min.", @"", ISA_Enc.DS, 101, 0, 0xD9940000, 0x0003),
new InstInfo(0124, "ds_min_rtn_u32", "v4u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint min.", @"", ISA_Enc.DS, 39, 0, 0xD89C0000, 0x0003),
new InstInfo(0125, "ds_min_rtn_u64", "v8u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint min.", @"", ISA_Enc.DS, 103, 0, 0xD99C0000, 0x0003),
new InstInfo(0126, "ds_min_src2_f32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = (DS[B] < DS[A]) ? DS[B] : DS[A]; float, handles NaN/INF/denorm.", @"", ISA_Enc.DS, 146, 0, 0xDA480000, 0x0003),
new InstInfo(0127, "ds_min_src2_f64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}).  [A] = (D0 < DS[A]) ? D0 : DS[A]; float, handles NaN/INF/denorm. ", @"", ISA_Enc.DS, 210, 0, 0xDB480000, 0x0003),
new InstInfo(0128, "ds_min_src2_i32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = min(DS[A], DS[B]); int min.", @"", ISA_Enc.DS, 133, 0, 0xDA140000, 0x0003),
new InstInfo(0129, "ds_min_src2_i64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Int min.", @"", ISA_Enc.DS, 197, 0, 0xDB140000, 0x0003),
new InstInfo(0130, "ds_min_src2_u32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = min(DS[A], DS[B]); uint min.", @"", ISA_Enc.DS, 135, 0, 0xDA1C0000, 0x0003),
new InstInfo(0131, "ds_min_src2_u64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint min.", @"", ISA_Enc.DS, 199, 0, 0xDB1C0000, 0x0003),
new InstInfo(0132, "ds_min_u32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = min(DS[A], D0); uint min.", @"", ISA_Enc.DS, 7, 0, 0xD81C0000, 0x0003),
new InstInfo(0133, "ds_min_u64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint min.", @"", ISA_Enc.DS, 71, 0, 0xD91C0000, 0x0003),
new InstInfo(0134, "ds_mskor_b32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = (DS[A] ^ ~D0) | D1; masked Dword OR.", @"", ISA_Enc.DS, 12, 0, 0xD8300000, 0x0003),
new InstInfo(0135, "ds_mskor_b64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Masked Dword XOR.", @"", ISA_Enc.DS, 76, 0, 0xD9300000, 0x0003),
new InstInfo(0136, "ds_mskor_rtn_b32", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Masked Dword OR.", @"", ISA_Enc.DS, 44, 0, 0xD8B00000, 0x0003),
new InstInfo(0137, "ds_mskor_rtn_b64", "v8b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Masked Dword XOR.", @"", ISA_Enc.DS, 108, 0, 0xD9B00000, 0x0003),
new InstInfo(0138, "ds_nop", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Do nothing.", @"", ISA_Enc.DS, 20, 0, 0xD8500000, 0x0003),
new InstInfo(0139, "ds_or_b32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = DS[A] | D0; Dword OR.", @"", ISA_Enc.DS, 10, 0, 0xD8280000, 0x0003),
new InstInfo(0140, "ds_or_b64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Dword OR.", @"", ISA_Enc.DS, 74, 0, 0xD9280000, 0x0003),
new InstInfo(0141, "ds_or_rtn_b32", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Dword OR.", @"", ISA_Enc.DS, 42, 0, 0xD8A80000, 0x0003),
new InstInfo(0142, "ds_or_rtn_b64", "v8b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Dword OR.", @"", ISA_Enc.DS, 106, 0, 0xD9A80000, 0x0003),
new InstInfo(0143, "ds_or_src2_b32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = DS[A] | DS[B]; Dword OR.", @"", ISA_Enc.DS, 138, 0, 0xDA280000, 0x0003),
new InstInfo(0144, "ds_or_src2_b64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Dword OR.", @"", ISA_Enc.DS, 202, 0, 0xDB280000, 0x0003),
new InstInfo(0145, "ds_ordered_count", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Increment an append counter. The operation is done in wavefront-creation order.", @"", ISA_Enc.DS, 63, 0, 0xD8FC0000, 0x0003),
new InstInfo(0146, "ds_read_b128", "v16b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Qword read.", @"", ISA_Enc.DS, 255, 0, 0xDBFC0000, 0x0003),
new InstInfo(0147, "ds_read_b32", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"R = DS[A]; Dword read.", @"", ISA_Enc.DS, 54, 0, 0xD8D80000, 0x0003),
new InstInfo(0148, "ds_read_b64", "v8b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Dword read.", @"", ISA_Enc.DS, 118, 0, 0xD9D80000, 0x0003),
new InstInfo(0149, "ds_read_b96", "v12b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Tri-dword read.", @"", ISA_Enc.DS, 254, 0, 0xDBF80000, 0x0003),
new InstInfo(0150, "ds_read_i16", "v4i", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"R = signext(DS[A][15:0]}; signed short read.", @"", ISA_Enc.DS, 59, 0, 0xD8EC0000, 0x0003),
new InstInfo(0151, "ds_read_i8", "v4i", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"R = signext(DS[A][7:0]}; signed byte read.", @"", ISA_Enc.DS, 57, 0, 0xD8E40000, 0x0003),
new InstInfo(0152, "ds_read_u16", "v4u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"R = {16’h0,DS[A][15:0]}; unsigned short read.", @"", ISA_Enc.DS, 60, 0, 0xD8F00000, 0x0003),
new InstInfo(0153, "ds_read_u8", "v4u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"R = {24’h0,DS[A][7:0]};unsigned byte read.", @"", ISA_Enc.DS, 58, 0, 0xD8E80000, 0x0003),
new InstInfo(0154, "ds_read2_b32", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"R = DS[ADDR+offset0*4], R+1 = DS[ADDR+offset1*4]. Read 2 Dwords.", @"", ISA_Enc.DS, 55, 0, 0xD8DC0000, 0x0003),
new InstInfo(0155, "ds_read2_b64", "v8b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"R = DS[ADDR+offset0*8], R+1 = DS[ADDR+offset1*8]. Read 2 Dwords", @"", ISA_Enc.DS, 119, 0, 0xD9DC0000, 0x0003),
new InstInfo(0156, "ds_read2st64_b32", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"R = DS[ADDR+offset0*4*64], R+1 = DS[ADDR+offset1*4*64]. Read 2 Dwords.", @"", ISA_Enc.DS, 56, 0, 0xD8E00000, 0x0003),
new InstInfo(0157, "ds_read2st64_b64", "v8b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"R = DS[ADDR+offset0*8*64], R+1 = DS[ADDR+offset1*8*64]. Read 2 Dwords.", @"", ISA_Enc.DS, 120, 0, 0xD9E00000, 0x0003),
new InstInfo(0158, "ds_rsub_rtn_u32", "v4u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint reverse subtract.", @"", ISA_Enc.DS, 34, 0, 0xD8880000, 0x0003),
new InstInfo(0159, "ds_rsub_rtn_u64", "v8u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint reverse subtract.", @"", ISA_Enc.DS, 98, 0, 0xD9880000, 0x0003),
new InstInfo(0160, "ds_rsub_src2_u32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = DS[B] - DS[A]; uint reverse subtract.", @"", ISA_Enc.DS, 130, 0, 0xDA080000, 0x0003),
new InstInfo(0161, "ds_rsub_src2_u64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint reverse subtract.", @"", ISA_Enc.DS, 194, 0, 0xDB080000, 0x0003),
new InstInfo(0162, "ds_rsub_u32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = D0 - DS[A]; uint reverse subtract.", @"", ISA_Enc.DS, 2, 0, 0xD8080000, 0x0003),
new InstInfo(0163, "ds_rsub_u64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint reverse subtract.", @"", ISA_Enc.DS, 66, 0, 0xD9080000, 0x0003),
new InstInfo(0164, "ds_sub_rtn_u32", "v4u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint subtract.", @"", ISA_Enc.DS, 33, 0, 0xD8840000, 0x0003),
new InstInfo(0165, "ds_sub_rtn_u64", "v8u", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint subtract.", @"", ISA_Enc.DS, 97, 0, 0xD9840000, 0x0003),
new InstInfo(0166, "ds_sub_src2_u32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = DS[A] - DS[B]; uint subtract.", @"", ISA_Enc.DS, 129, 0, 0xDA040000, 0x0003),
new InstInfo(0167, "ds_sub_src2_u64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint subtract.", @"", ISA_Enc.DS, 193, 0, 0xDB040000, 0x0003),
new InstInfo(0168, "ds_sub_u32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = DS[A] - D0; uint subtract.", @"", ISA_Enc.DS, 1, 0, 0xD8040000, 0x0003),
new InstInfo(0169, "ds_sub_u64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Uint subtract.", @"", ISA_Enc.DS, 65, 0, 0xD9040000, 0x0003),
new InstInfo(0170, "ds_swizzle_b32", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Swizzles input thread data based on offset mask and returns; note does not read or write the DS memory banks.", @"", ISA_Enc.DS, 53, 0, 0xD8D40000, 0x0003),
new InstInfo(0171, "ds_wrap_rtn_b32", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = (DS[A] >= D0) ? DS[A] - D0 : DS[A] + D1.", @"", ISA_Enc.DS, 52, 0, 0xD8D00000, 0x0003),
new InstInfo(0172, "ds_write_b128", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"{DS[A+3], DS[A+2], DS[A+1], DS[A]} = D0[127:0]; qword write.", @"", ISA_Enc.DS, 223, 0, 0xDB7C0000, 0x0003),
new InstInfo(0173, "ds_write_b16", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = D0[15:0]; short write.", @"", ISA_Enc.DS, 31, 0, 0xD87C0000, 0x0003),
new InstInfo(0174, "ds_write_b32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = D0; write a Dword.", @"", ISA_Enc.DS, 13, 0, 0xD8340000, 0x0003),
new InstInfo(0175, "ds_write_b64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Write.", @"", ISA_Enc.DS, 77, 0, 0xD9340000, 0x0003),
new InstInfo(0176, "ds_write_b8", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = D0[7:0]; byte write.", @"", ISA_Enc.DS, 30, 0, 0xD8780000, 0x0003),
new InstInfo(0177, "ds_write_b96", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"{DS[A+2], DS[A+1], DS[A]} = D0[95:0]; tri-dword write.", @"", ISA_Enc.DS, 222, 0, 0xDB780000, 0x0003),
new InstInfo(0178, "ds_write_src2_b32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = DS[B]; write Dword.", @"", ISA_Enc.DS, 140, 0, 0xDA300000, 0x0003),
new InstInfo(0179, "ds_write_src2_b64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = DS[B]; write Qword.", @"", ISA_Enc.DS, 204, 0, 0xDB300000, 0x0003),
new InstInfo(0180, "ds_write2_b32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[ADDR+offset0*4] = D0; DS[ADDR+offset1*4] = D1; write 2 Dwords.", @"", ISA_Enc.DS, 14, 0, 0xD8380000, 0x0003),
new InstInfo(0181, "ds_write2_b64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[ADDR+offset0*8] = D0; DS[ADDR+offset1*8] = D1; write 2 Dwords.", @"", ISA_Enc.DS, 78, 0, 0xD9380000, 0x0003),
new InstInfo(0182, "ds_write2st64_b32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[ADDR+offset0*4*64] = D0; DS[ADDR+offset1*4*64] = D1; write 2 Dwords.", @"", ISA_Enc.DS, 15, 0, 0xD83C0000, 0x0003),
new InstInfo(0183, "ds_write2st64_b64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[ADDR+offset0*8*64] = D0; DS[ADDR+offset1*8*64] = D1; write 2 Dwords.", @"", ISA_Enc.DS, 79, 0, 0xD93C0000, 0x0003),
new InstInfo(0184, "ds_wrxchg_rtn_b32", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Write exchange. Offset = {offset1,offset0}. A = ADDR+offset. D=DS[Addr]. DS[Addr]=D0.", @"", ISA_Enc.DS, 45, 0, 0xD8B40000, 0x0003),
new InstInfo(0185, "ds_wrxchg_rtn_b64", "v8b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Write exchange.", @"", ISA_Enc.DS, 109, 0, 0xD9B40000, 0x0003),
new InstInfo(0186, "ds_wrxchg2_rtn_b32", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Write exchange 2 separate Dwords.", @"", ISA_Enc.DS, 46, 0, 0xD8B80000, 0x0003),
new InstInfo(0187, "ds_wrxchg2_rtn_b64", "v8b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Write exchange relative.", @"", ISA_Enc.DS, 110, 0, 0xD9B80000, 0x0003),
new InstInfo(0188, "ds_wrxchg2st64_rtn_b32", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Write exchange 2 Dwords, stride 64.", @"", ISA_Enc.DS, 47, 0, 0xD8BC0000, 0x0003),
new InstInfo(0189, "ds_wrxchg2st64_rtn_b64", "v8b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Write echange 2 Dwords.", @"", ISA_Enc.DS, 111, 0, 0xD9BC0000, 0x0003),
new InstInfo(0190, "ds_xor_b32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"DS[A] = DS[A] ^ D0; Dword XOR.", @"", ISA_Enc.DS, 11, 0, 0xD82C0000, 0x0003),
new InstInfo(0191, "ds_xor_b64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Dword XOR.", @"", ISA_Enc.DS, 75, 0, 0xD92C0000, 0x0003),
new InstInfo(0192, "ds_xor_rtn_b32", "v4b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Dword XOR.", @"", ISA_Enc.DS, 43, 0, 0xD8AC0000, 0x0003),
new InstInfo(0193, "ds_xor_rtn_b64", "v8b", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Dword XOR.", @"", ISA_Enc.DS, 107, 0, 0xD9AC0000, 0x0003),
new InstInfo(0194, "ds_xor_src2_b32", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"B = A + 4*(offset1[7] ? {A[31],A[31:17]} : {offset1[6],offset1[6:0],offset0}). DS[A] = DS[A] ^ DS[B]; Dword XOR.", @"", ISA_Enc.DS, 139, 0, 0xDA2C0000, 0x0003),
new InstInfo(0195, "ds_xor_src2_b64", "none", "todo", "todo", "todo", "todo", "todo", "todo", 4, 7, @"Dword XOR.", @"", ISA_Enc.DS, 203, 0, 0xDB2C0000, 0x0003),
new InstInfo(0196, "flat_atomic_add", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b, dst += src. Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 50, 0, 0xDCC80000, 0x0003),
new InstInfo(0197, "flat_atomic_add_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b, dst += src. Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 82, 0, 0xDD480000, 0x0003),
new InstInfo(0198, "flat_atomic_and", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b, dst &= src. Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 57, 0, 0xDCE40000, 0x0003),
new InstInfo(0199, "flat_atomic_and_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b, dst &= src. Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 89, 0, 0xDD640000, 0x0003),
new InstInfo(0200, "flat_atomic_cmpswap", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b, dst = (dst==cmp) ? src : dst. Returns previous value if rtn==1.src comes from the first data-VGPR, cmp from the second.", @"", ISA_Enc.FLAT, 49, 0, 0xDCC40000, 0x0003),
new InstInfo(0201, "flat_atomic_cmpswap_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b, dst = (dst==cmp) ? src : dst. Returns previous value if rtn==1. src comes from the first two data-VGPRs, cmp from the second two.", @"", ISA_Enc.FLAT, 81, 0, 0xDD440000, 0x0003),
new InstInfo(0202, "flat_atomic_dec", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b, dst = ((dst==0 || (dst > src)) ? src : dst-1 (unsigned comparison). Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 61, 0, 0xDCF40000, 0x0003),
new InstInfo(0203, "flat_atomic_dec_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b, dst = ((dst==0 || (dst > src)) ? src : dst - 1. Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 93, 0, 0xDD740000, 0x0003),
new InstInfo(0204, "flat_atomic_fcmpswap", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b , dst = (dst == cmp) ? src : dst, returns previous value if rtn==1. Floating point compare-swap handles NaN/INF/denorm. src comes from the first data-VGPR; cmp from the second.", @"", ISA_Enc.FLAT, 62, 0, 0xDCF80000, 0x0003),
new InstInfo(0205, "flat_atomic_fcmpswap_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b , dst = (dst == cmp) ? src : dst, returns previous value if rtn==1. Double compare swap (handles NaN/INF/denorm). src comes from the first two data-VGPRs, cmp from the second two.", @"", ISA_Enc.FLAT, 94, 0, 0xDD780000, 0x0003),
new InstInfo(0206, "flat_atomic_fmax", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b , dst = (src > dst) ? src : dst, returns previous value if rtn==1. Floating point compare handles NaN/INF/denorm.", @"", ISA_Enc.FLAT, 64, 0, 0xDD000000, 0x0003),
new InstInfo(0207, "flat_atomic_fmax_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b , dst = (src > dst) ? src : dst, returns previous value if rtn==1. Double, handles NaN/INF/denorm.", @"", ISA_Enc.FLAT, 96, 0, 0xDD800000, 0x0003),
new InstInfo(0208, "flat_atomic_fmin", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b , dst = (src < dst) ? src : dst. Returns previous value if rtn==1. float, handles NaN/INF/denorm.", @"", ISA_Enc.FLAT, 63, 0, 0xDCFC0000, 0x0003),
new InstInfo(0209, "flat_atomic_fmin_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b , dst = (src < dst) ? src : dst, returns previous value if rtn==1. Double, handles NaN/INF/denorm.", @"", ISA_Enc.FLAT, 95, 0, 0xDD7C0000, 0x0003),
new InstInfo(0210, "flat_atomic_inc", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b, dst = (dst >= src) ? 0 : dst+1 (unsigned comparison). Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 60, 0, 0xDCF00000, 0x0003),
new InstInfo(0211, "flat_atomic_inc_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b, dst = (dst >= src) ? 0 : dst+1. Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 92, 0, 0xDD700000, 0x0003),
new InstInfo(0212, "flat_atomic_or", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b, dst |= src. Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 58, 0, 0xDCE80000, 0x0003),
new InstInfo(0213, "flat_atomic_or_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b, dst |= src. Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 90, 0, 0xDD680000, 0x0003),
new InstInfo(0214, "flat_atomic_smax", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b, dst = (src > dst) ? src : dst (signed comparison). Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 55, 0, 0xDCDC0000, 0x0003),
new InstInfo(0215, "flat_atomic_smax_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b, dst = (src > dst) ? src : dst (signed comparison). Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 87, 0, 0xDD5C0000, 0x0003),
new InstInfo(0216, "flat_atomic_smin", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b, dst = (src < dst) ? src : dst (signed comparison). Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 53, 0, 0xDCD40000, 0x0003),
new InstInfo(0217, "flat_atomic_smin_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b, dst = (src < dst) ? src : dst (signed comparison). Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 85, 0, 0xDD540000, 0x0003),
new InstInfo(0218, "flat_atomic_sub", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b, dst -= src. Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 51, 0, 0xDCCC0000, 0x0003),
new InstInfo(0219, "flat_atomic_sub_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b, dst -= src. Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 83, 0, 0xDD4C0000, 0x0003),
new InstInfo(0220, "flat_atomic_swap", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b. dst=src, returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 48, 0, 0xDCC00000, 0x0003),
new InstInfo(0221, "flat_atomic_swap_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b. dst=src, returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 80, 0, 0xDD400000, 0x0003),
new InstInfo(0222, "flat_atomic_umax", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b, dst = (src > dst) ? src : dst (unsigned comparison). Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 56, 0, 0xDCE00000, 0x0003),
new InstInfo(0223, "flat_atomic_umax_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b, dst = (src > dst) ? src : dst (unsigned comparison). Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 88, 0, 0xDD600000, 0x0003),
new InstInfo(0224, "flat_atomic_umin", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b, dst = (src < dst) ? src : dst (unsigned comparison). Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 54, 0, 0xDCD80000, 0x0003),
new InstInfo(0225, "flat_atomic_umin_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b, dst = (src < dst) ? src : dst (unsigned comparison). Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 86, 0, 0xDD580000, 0x0003),
new InstInfo(0226, "flat_atomic_xor", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 32b, dst ^= src. Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 59, 0, 0xDCEC0000, 0x0003),
new InstInfo(0227, "flat_atomic_xor_x2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"ATOMIC - 64b, dst ^= src. Returns previous value if rtn==1.", @"", ISA_Enc.FLAT, 91, 0, 0xDD6C0000, 0x0003),
new InstInfo(0228, "flat_load_dword", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"LOAD - Flat load Dword.", @"", ISA_Enc.FLAT, 12, 0, 0xDC300000, 0x0003),
new InstInfo(0229, "flat_load_dwordx2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"LOAD - Flat load 2 Dwords.", @"", ISA_Enc.FLAT, 13, 0, 0xDC340000, 0x0003),
new InstInfo(0230, "flat_load_dwordx3", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"LOAD - Flat load 3 Dwords.", @"", ISA_Enc.FLAT, 15, 0, 0xDC3C0000, 0x0003),
new InstInfo(0231, "flat_load_dwordx4", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"LOAD - Flat load 4 Dwords.", @"", ISA_Enc.FLAT, 14, 0, 0xDC380000, 0x0003),
new InstInfo(0232, "flat_load_sbyte", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"LOAD - Flat load signed byte. Sign extend to VGPR destination.", @"", ISA_Enc.FLAT, 9, 0, 0xDC240000, 0x0003),
new InstInfo(0233, "flat_load_sshort", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"LOAD - Flat load signed short. Sign extend to VGPR destination.", @"", ISA_Enc.FLAT, 11, 0, 0xDC2C0000, 0x0003),
new InstInfo(0234, "flat_load_ubyte", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"LOAD - Flat load unsigned byte. Zero extend to VGPR destination.", @"", ISA_Enc.FLAT, 8, 0, 0xDC200000, 0x0003),
new InstInfo(0235, "flat_load_ushort", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"LOAD - Flat load unsigned short. Zero extebd to VGPR destination.", @"", ISA_Enc.FLAT, 10, 0, 0xDC280000, 0x0003),
new InstInfo(0236, "flat_store_byte", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"STORE - Flat store byte.", @"", ISA_Enc.FLAT, 24, 0, 0xDC600000, 0x0003),
new InstInfo(0237, "flat_store_dword", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"STORE - Flat store Dword.", @"", ISA_Enc.FLAT, 28, 0, 0xDC700000, 0x0003),
new InstInfo(0238, "flat_store_dwordx2", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"STORE - Flat store 2 Dwords.", @"", ISA_Enc.FLAT, 29, 0, 0xDC740000, 0x0003),
new InstInfo(0239, "flat_store_dwordx3", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"STORE - Flat store 3 Dwords.", @"", ISA_Enc.FLAT, 31, 0, 0xDC7C0000, 0x0003),
new InstInfo(0240, "flat_store_dwordx4", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"STORE - Flat store 4 Dwords.", @"", ISA_Enc.FLAT, 30, 0, 0xDC780000, 0x0003),
new InstInfo(0241, "flat_store_short", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 3, 6, @"STORE - Flat store short.", @"", ISA_Enc.FLAT, 26, 0, 0xDC680000, 0x0003),
new InstInfo(0242, "image_atomic_add", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst += src. Returns previous value if glc==1.", @"", ISA_Enc.MIMG, 17, 0, 0xF0440000, 0x0003),
new InstInfo(0243, "image_atomic_and", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst &= src. Returns previous value if glc==1.", @"", ISA_Enc.MIMG, 24, 0, 0xF0600000, 0x0003),
new InstInfo(0244, "image_atomic_cmpswap", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst = (dst==cmp) ? src : dst. Returns previous value if glc==1.", @"", ISA_Enc.MIMG, 16, 0, 0xF0400000, 0x0003),
new InstInfo(0245, "image_atomic_dec", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst = ((dst==0 || (dst > src)) ? src : dst-1. Returns previous value if glc==1.", @"", ISA_Enc.MIMG, 28, 0, 0xF0700000, 0x0003),
new InstInfo(0246, "image_atomic_fcmpswap", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst = (dst == cmp) ? src : dst, returns previous value of dst if glc==1 - double and float atomic compare swap. Obeys floating point compare rules for special values.", @"", ISA_Enc.MIMG, 29, 0, 0xF0740000, 0x0003),
new InstInfo(0247, "image_atomic_fmax", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst = (src > dst) ? src : dst, returns previous value of dst if glc==1 - double and float atomic min (handles NaN/INF/denorm).", @"", ISA_Enc.MIMG, 31, 0, 0xF07C0000, 0x0003),
new InstInfo(0248, "image_atomic_fmin", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst = (src < dst) ? src : dst, returns previous value of dst if glc==1 - double and float atomic min (handles NaN/INF/denorm).", @"", ISA_Enc.MIMG, 30, 0, 0xF0780000, 0x0003),
new InstInfo(0249, "image_atomic_inc", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst = (dst >= src) ? 0 : dst+1. Returns previous value if glc==1.", @"", ISA_Enc.MIMG, 27, 0, 0xF06C0000, 0x0003),
new InstInfo(0250, "image_atomic_or", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst |= src. Returns previous value if glc==1.", @"", ISA_Enc.MIMG, 25, 0, 0xF0640000, 0x0003),
new InstInfo(0251, "image_atomic_smax", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst = (src > dst) ? src : dst (signed). Returns previous value if glc==1.", @"", ISA_Enc.MIMG, 22, 0, 0xF0580000, 0x0003),
new InstInfo(0252, "image_atomic_smin", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst = (src < dst) ? src : dst (signed). Returns previous value if glc==1.", @"", ISA_Enc.MIMG, 20, 0, 0xF0500000, 0x0003),
new InstInfo(0253, "image_atomic_sub", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst -= src. Returns previous value if glc==1.", @"", ISA_Enc.MIMG, 18, 0, 0xF0480000, 0x0003),
new InstInfo(0254, "image_atomic_swap", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst=src, returns previous value if glc==1.", @"", ISA_Enc.MIMG, 15, 0, 0xF03C0000, 0x0003),
new InstInfo(0255, "image_atomic_umax", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst = (src > dst) ? src : dst (unsigned). Returns previous value if glc==1.", @"", ISA_Enc.MIMG, 23, 0, 0xF05C0000, 0x0003),
new InstInfo(0256, "image_atomic_umin", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst = (src < dst) ? src : dst (unsigned). Returns previous value if glc==1.", @"", ISA_Enc.MIMG, 21, 0, 0xF0540000, 0x0003),
new InstInfo(0257, "image_atomic_xor", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"dst ^= src. Returns previous value if glc==1.", @"", ISA_Enc.MIMG, 26, 0, 0xF0680000, 0x0003),
new InstInfo(0258, "image_gather4", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"gather 4 single component elements (2x2).", @"", ISA_Enc.MIMG, 64, 0, 0xF1000000, 0x0003),
new InstInfo(0259, "image_gather4_b", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"gather 4 single component elements (2x2) with user bias.", @"", ISA_Enc.MIMG, 67, 0, 0xF10C0000, 0x0003),
new InstInfo(0260, "image_gather4_b_cl", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"gather 4 single component elements (2x2) with user bias and clamp.", @"", ISA_Enc.MIMG, 68, 0, 0xF1100000, 0x0003),
new InstInfo(0261, "image_gather4_b_cl_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"GATHER4_B_CL, with user offsets.", @"", ISA_Enc.MIMG, 86, 0, 0xF1580000, 0x0003),
new InstInfo(0262, "image_gather4_b_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"GATHER4_B, with user offsets.", @"", ISA_Enc.MIMG, 85, 0, 0xF1540000, 0x0003),
new InstInfo(0263, "image_gather4_c", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"gather 4 single component elements (2x2) with PCF.", @"", ISA_Enc.MIMG, 70, 0, 0xF1180000, 0x0003),
new InstInfo(0264, "image_gather4_c_b", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"gather 4 single component elements (2x2) with user bias and PCF.", @"", ISA_Enc.MIMG, 77, 0, 0xF1340000, 0x0003),
new InstInfo(0265, "image_gather4_c_b_cl", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"gather 4 single component elements (2x2) with user bias, clamp and PCF.", @"", ISA_Enc.MIMG, 78, 0, 0xF1380000, 0x0003),
new InstInfo(0266, "image_gather4_c_b_cl_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"GATHER4_B_CL, with user offsets.", @"", ISA_Enc.MIMG, 94, 0, 0xF1780000, 0x0003),
new InstInfo(0267, "image_gather4_c_b_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"GATHER4_B, with user offsets.", @"", ISA_Enc.MIMG, 93, 0, 0xF1740000, 0x0003),
new InstInfo(0268, "image_gather4_c_cl", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"gather 4 single component elements (2x2) with user LOD clamp and PCF.", @"", ISA_Enc.MIMG, 71, 0, 0xF11C0000, 0x0003),
new InstInfo(0269, "image_gather4_c_cl_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"GATHER4_C_CL, with user offsets.", @"", ISA_Enc.MIMG, 89, 0, 0xF1640000, 0x0003),
new InstInfo(0270, "image_gather4_c_l", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"gather 4 single component elements (2x2) with user LOD and PCF.", @"", ISA_Enc.MIMG, 76, 0, 0xF1300000, 0x0003),
new InstInfo(0271, "image_gather4_c_l_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"GATHER4_C_L, with user offsets.", @"", ISA_Enc.MIMG, 92, 0, 0xF1700000, 0x0003),
new InstInfo(0272, "image_gather4_c_lz", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"gather 4 single component elements (2x2) at level 0, with PCF.", @"", ISA_Enc.MIMG, 79, 0, 0xF13C0000, 0x0003),
new InstInfo(0273, "image_gather4_c_lz_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"GATHER4_C_LZ, with user offsets.", @"", ISA_Enc.MIMG, 95, 0, 0xF17C0000, 0x0003),
new InstInfo(0274, "image_gather4_c_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"GATHER4_C, with user offsets.", @"", ISA_Enc.MIMG, 88, 0, 0xF1600000, 0x0003),
new InstInfo(0275, "image_gather4_cl", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"gather 4 single component elements (2x2) with user LOD clamp.", @"", ISA_Enc.MIMG, 65, 0, 0xF1040000, 0x0003),
new InstInfo(0276, "image_gather4_cl_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"GATHER4_CL, with user offsets.", @"", ISA_Enc.MIMG, 81, 0, 0xF1440000, 0x0003),
new InstInfo(0277, "image_gather4_l", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"gather 4 single component elements (2x2) with user LOD.", @"", ISA_Enc.MIMG, 66, 0, 0xF1080000, 0x0003),
new InstInfo(0278, "image_gather4_l_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"GATHER4_L, with user offsets.", @"", ISA_Enc.MIMG, 84, 0, 0xF1500000, 0x0003),
new InstInfo(0279, "image_gather4_lz", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"gather 4 single component elements (2x2) at level 0.", @"", ISA_Enc.MIMG, 69, 0, 0xF1140000, 0x0003),
new InstInfo(0280, "image_gather4_lz_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"GATHER4_LZ, with user offsets.", @"", ISA_Enc.MIMG, 87, 0, 0xF15C0000, 0x0003),
new InstInfo(0281, "image_gather4_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"GATHER4, with user offsets.", @"", ISA_Enc.MIMG, 80, 0, 0xF1400000, 0x0003),
new InstInfo(0282, "image_get_lod", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"Return calculated LOD.", @"", ISA_Enc.MIMG, 96, 0, 0xF1800000, 0x0003),
new InstInfo(0283, "image_get_resinfo", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"return resource info. No sampler.", @"", ISA_Enc.MIMG, 14, 0, 0xF0380000, 0x0003),
new InstInfo(0284, "image_load", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"Image memory load with format conversion specified in T#. No sampler.", @"", ISA_Enc.MIMG, 0, 0, 0xF0000000, 0x0003),
new InstInfo(0285, "image_load_mip", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"Image memory load with user-supplied mip level. No sampler.", @"", ISA_Enc.MIMG, 1, 0, 0xF0040000, 0x0003),
new InstInfo(0286, "image_load_mip_pck", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"Image memory load with user-supplied mip level, no format conversion. No sampler.", @"", ISA_Enc.MIMG, 4, 0, 0xF0100000, 0x0003),
new InstInfo(0287, "image_load_mip_pck_sgn", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"Image memory load with user-supplied mip level, no format conversion and with sign extension. No sampler.", @"", ISA_Enc.MIMG, 5, 0, 0xF0140000, 0x0003),
new InstInfo(0288, "image_load_pck", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"Image memory load with no format conversion. No sampler.", @"", ISA_Enc.MIMG, 2, 0, 0xF0080000, 0x0003),
new InstInfo(0289, "image_load_pck_sgn", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"Image memory load with no format conversion and sign extension. No sampler.", @"", ISA_Enc.MIMG, 3, 0, 0xF00C0000, 0x0003),
new InstInfo(0290, "image_sample", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"sample texture map.", @"", ISA_Enc.MIMG, 32, 0, 0xF0800000, 0x0003),
new InstInfo(0291, "image_sample_b", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"sample texture map, with lod bias.", @"", ISA_Enc.MIMG, 37, 0, 0xF0940000, 0x0003),
new InstInfo(0292, "image_sample_b_cl", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"sample texture map, with LOD clamp specified in shader, with lod bias.", @"", ISA_Enc.MIMG, 38, 0, 0xF0980000, 0x0003),
new InstInfo(0293, "image_sample_b_cl_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_O, with LOD clamp specified in shader, with lod bias.", @"", ISA_Enc.MIMG, 54, 0, 0xF0D80000, 0x0003),
new InstInfo(0294, "image_sample_b_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_O, with lod bias.", @"", ISA_Enc.MIMG, 53, 0, 0xF0D40000, 0x0003),
new InstInfo(0295, "image_sample_c", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"sample texture map, with PCF.", @"", ISA_Enc.MIMG, 40, 0, 0xF0A00000, 0x0003),
new InstInfo(0296, "image_sample_c_b", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C, with lod bias.", @"", ISA_Enc.MIMG, 45, 0, 0xF0B40000, 0x0003),
new InstInfo(0297, "image_sample_c_b_cl", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C, with LOD clamp specified in shader, with lod bias.", @"", ISA_Enc.MIMG, 46, 0, 0xF0B80000, 0x0003),
new InstInfo(0298, "image_sample_c_b_cl_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C_O, with LOD clamp specified in shader, with lod bias.", @"", ISA_Enc.MIMG, 62, 0, 0xF0F80000, 0x0003),
new InstInfo(0299, "image_sample_c_b_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C_O, with lod bias.", @"", ISA_Enc.MIMG, 61, 0, 0xF0F40000, 0x0003),
new InstInfo(0300, "image_sample_c_cd", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C, with user derivatives (LOD per quad).", @"", ISA_Enc.MIMG, 106, 0, 0xF1A80000, 0x0003),
new InstInfo(0301, "image_sample_c_cd_cl", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C, with LOD clamp specified in shader, with user derivatives (LOD per quad).", @"", ISA_Enc.MIMG, 107, 0, 0xF1AC0000, 0x0003),
new InstInfo(0302, "image_sample_c_cd_cl_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C_O, with LOD clamp specified in shader, with user derivatives (LOD per quad).", @"", ISA_Enc.MIMG, 111, 0, 0xF1BC0000, 0x0003),
new InstInfo(0303, "image_sample_c_cd_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C_O, with user derivatives (LOD per quad).", @"", ISA_Enc.MIMG, 110, 0, 0xF1B80000, 0x0003),
new InstInfo(0304, "image_sample_c_cl", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C, with LOD clamp specified in shader.", @"", ISA_Enc.MIMG, 41, 0, 0xF0A40000, 0x0003),
new InstInfo(0305, "image_sample_c_cl_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C_O, with LOD clamp specified in shader.", @"", ISA_Enc.MIMG, 57, 0, 0xF0E40000, 0x0003),
new InstInfo(0306, "image_sample_c_d", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C, with user derivatives.", @"", ISA_Enc.MIMG, 42, 0, 0xF0A80000, 0x0003),
new InstInfo(0307, "image_sample_c_d_cl", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C, with LOD clamp specified in shader, with user derivatives.", @"", ISA_Enc.MIMG, 43, 0, 0xF0AC0000, 0x0003),
new InstInfo(0308, "image_sample_c_d_cl_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C_O, with LOD clamp specified in shader, with user derivatives.", @"", ISA_Enc.MIMG, 59, 0, 0xF0EC0000, 0x0003),
new InstInfo(0309, "image_sample_c_d_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C_O, with user derivatives.", @"", ISA_Enc.MIMG, 58, 0, 0xF0E80000, 0x0003),
new InstInfo(0310, "image_sample_c_l", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C, with user LOD.", @"", ISA_Enc.MIMG, 44, 0, 0xF0B00000, 0x0003),
new InstInfo(0311, "image_sample_c_l_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C_O, with user LOD.", @"", ISA_Enc.MIMG, 60, 0, 0xF0F00000, 0x0003),
new InstInfo(0312, "image_sample_c_lz", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C, from level 0.", @"", ISA_Enc.MIMG, 47, 0, 0xF0BC0000, 0x0003),
new InstInfo(0313, "image_sample_c_lz_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C_O, from level 0.", @"", ISA_Enc.MIMG, 63, 0, 0xF0FC0000, 0x0003),
new InstInfo(0314, "image_sample_c_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_C with user specified offsets.", @"", ISA_Enc.MIMG, 56, 0, 0xF0E00000, 0x0003),
new InstInfo(0315, "image_sample_cd", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"sample texture map, with user derivatives (LOD per quad)", @"", ISA_Enc.MIMG, 104, 0, 0xF1A00000, 0x0003),
new InstInfo(0316, "image_sample_cd_cl", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"sample texture map, with LOD clamp specified in shader, with user derivatives (LOD per quad).", @"", ISA_Enc.MIMG, 105, 0, 0xF1A40000, 0x0003),
new InstInfo(0317, "image_sample_cd_cl_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_O, with LOD clamp specified in shader, with user derivatives (LOD per quad).", @"", ISA_Enc.MIMG, 109, 0, 0xF1B40000, 0x0003),
new InstInfo(0318, "image_sample_cd_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_O, with user derivatives (LOD per quad).", @"", ISA_Enc.MIMG, 108, 0, 0xF1B00000, 0x0003),
new InstInfo(0319, "image_sample_cl", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"sample texture map, with LOD clamp specified in shader.", @"", ISA_Enc.MIMG, 33, 0, 0xF0840000, 0x0003),
new InstInfo(0320, "image_sample_cl_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_O with LOD clamp specified in shader.", @"", ISA_Enc.MIMG, 49, 0, 0xF0C40000, 0x0003),
new InstInfo(0321, "image_sample_d", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"sample texture map, with user derivatives.", @"", ISA_Enc.MIMG, 34, 0, 0xF0880000, 0x0003),
new InstInfo(0322, "image_sample_d_cl", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"sample texture map, with LOD clamp specified in shader, with user derivatives.", @"", ISA_Enc.MIMG, 35, 0, 0xF08C0000, 0x0003),
new InstInfo(0323, "image_sample_d_cl_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_O, with LOD clamp specified in shader, with user derivatives.", @"", ISA_Enc.MIMG, 51, 0, 0xF0CC0000, 0x0003),
new InstInfo(0324, "image_sample_d_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_O, with user derivatives.", @"", ISA_Enc.MIMG, 50, 0, 0xF0C80000, 0x0003),
new InstInfo(0325, "image_sample_l", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"sample texture map, with user LOD.", @"", ISA_Enc.MIMG, 36, 0, 0xF0900000, 0x0003),
new InstInfo(0326, "image_sample_l_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_O, with user LOD.", @"", ISA_Enc.MIMG, 52, 0, 0xF0D00000, 0x0003),
new InstInfo(0327, "image_sample_lz", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"sample texture map, from level 0.", @"", ISA_Enc.MIMG, 39, 0, 0xF09C0000, 0x0003),
new InstInfo(0328, "image_sample_lz_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"SAMPLE_O, from level 0.", @"", ISA_Enc.MIMG, 55, 0, 0xF0DC0000, 0x0003),
new InstInfo(0329, "image_sample_o", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"sample texture map, with user offsets.", @"", ISA_Enc.MIMG, 48, 0, 0xF0C00000, 0x0003),
new InstInfo(0330, "image_store", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"Image memory store with formatconversion specified in T#. No sampler.", @"", ISA_Enc.MIMG, 8, 0, 0xF0200000, 0x0003),
new InstInfo(0331, "image_store_mip", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"Image memory store with format conversion specified in T# to user specified mip level. No sampler.", @"", ISA_Enc.MIMG, 9, 0, 0xF0240000, 0x0003),
new InstInfo(0332, "image_store_mip_pck", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"Image memory store of packed data without format conversion to user-supplied mip level. No sampler.", @"", ISA_Enc.MIMG, 11, 0, 0xF02C0000, 0x0003),
new InstInfo(0333, "image_store_pck", "todo", "todo", "todo", "todo", "todo", "todo", "todo", 0, 0, @"Image memory store of packed data without format conversion. No sampler.", @"", ISA_Enc.MIMG, 10, 0, 0xF0280000, 0x0003),
new InstInfo(0334, "s_abs_i32", "s4[iu]", "s4i", "none", "none", "none", "none", "none", 2, 2, @"D.i = abs(S0.i). SCC=1 if result is non-zero.", @"", ISA_Enc.SOP1, 52, 0, 0xBE803400, 0x0081),
new InstInfo(0335, "s_absdiff_i32", "s4i", "s4i", "s4i", "none", "none", "none", "none", 3, 3, @"D.i = abs(S0.i - S1.i). SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 44, 0, 0x96000000, 0x0081),
new InstInfo(0336, "s_add_i32", "s4i", "s4i", "s4i", "none", "none", "none", "none", 3, 3, @"D.u = S0.i + S1.i. SCC = signed overflow.", @"", ISA_Enc.SOP2, 2, 0, 0x81000000, 0x0081),
new InstInfo(0337, "s_add_u32", "s4u", "s4u", "s4u", "none", "none", "none", "none", 3, 3, @"D.u = S0.u + S1.u. SCC = unsigned carry out.", @"", ISA_Enc.SOP2, 0, 0, 0x80000000, 0x0081),
new InstInfo(0338, "s_addc_u32", "s4u", "s4u", "s4u", "scc", "none", "none", "none", 4, 4, @"D.u = S0.u + S1.u + SCC. SCC = unsigned carry-out.", @"", ISA_Enc.SOP2, 4, 0, 0x82000000, 0x0181),
new InstInfo(0339, "s_addk_i32", "s4i", "none", "none", "none", "none", "s16", "none", 2, 2, @"D.i = D.i + signext(SIMM16). SCC = signed overflow.", @"", ISA_Enc.SOPK, 15, 0, 0xB7800000, 0x0080),
new InstInfo(0340, "s_and_b32", "s4b", "s4b", "s4b", "none", "none", "none", "none", 3, 3, @"D.u = S0.u & S1.u. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 14, 0, 0x87000000, 0x0081),
new InstInfo(0341, "s_and_b64", "s8b", "s8b", "s8b", "none", "none", "none", "none", 3, 3, @"D.u = S0.u & S1.u. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 15, 0, 0x87800000, 0x0081),
new InstInfo(0342, "s_and_saveexec_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.u = EXEC, EXEC = S0.u & EXEC. SCC = 1 if the new value of EXEC is non-zero.", @"", ISA_Enc.SOP1, 36, 0, 0xBE802400, 0x0099),
new InstInfo(0343, "s_andn2_b32", "s4b", "s4b", "s4b", "none", "none", "none", "none", 3, 3, @"D.u = S0.u & ~S1.u. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 20, 0, 0x8A000000, 0x0081),
new InstInfo(0344, "s_andn2_b64", "s8b", "s8b", "s8b", "none", "none", "none", "none", 3, 3, @"D.u = S0.u & ~S1.u. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 21, 0, 0x8A800000, 0x0081),
new InstInfo(0345, "s_andn2_saveexec_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.u = EXEC, EXEC = S0.u & ~EXEC. SCC = 1 if the new value of EXEC is non-zero.", @"", ISA_Enc.SOP1, 39, 0, 0xBE802700, 0x0099),
new InstInfo(0346, "s_ashr_i32", "s4i", "s4i", "s4i", "none", "none", "none", "none", 3, 3, @"D.i = signext(S0.i) >> S1.i[4:0]. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 34, 0, 0x91000000, 0x0081),
new InstInfo(0347, "s_ashr_i64", "s8i", "s8i", "s8i", "none", "none", "none", "none", 3, 3, @"D.i = signext(S0.i) >> S1.i[5:0]. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 35, 0, 0x91800000, 0x0081),
new InstInfo(0348, "s_barrier", "none", "none", "none", "none", "none", "none", "none", 0, 0, @"Sync waves within a work-group.", @"", ISA_Enc.SOPP, 10, 0, 0xBF8A0000, 0x0000),
new InstInfo(0349, "s_bcnt0_i32_b32", "s4u", "s4b", "none", "none", "none", "none", "none", 2, 2, @"D.i = CountZeroBits(S0.u). SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP1, 13, 0, 0xBE800D00, 0x0081),
new InstInfo(0350, "s_bcnt0_i32_b64", "s4u", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.i = CountZeroBits(S0.u). SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP1, 14, 0, 0xBE800E00, 0x0081),
new InstInfo(0351, "s_bcnt1_i32_b32", "s4u", "s4b", "none", "none", "none", "none", "none", 2, 2, @"D.i = CountOneBits(S0.u). SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP1, 15, 0, 0xBE800F00, 0x0081),
new InstInfo(0352, "s_bcnt1_i32_b64", "s4u", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.i = CountOneBits(S0.u). SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP1, 16, 0, 0xBE801000, 0x0081),
new InstInfo(0353, "s_bfe_i32", "s4i", "s4i", "s4i", "none", "none", "none", "none", 3, 3, @"Replace description text with:<br>DX11 Unsigned bitfield extract. Extract a contiguous range of bits from 32-bit source. <br>SRC0 = input data<br>SRC1 = the lowest bit position to select<br>SRC2 = the width of the bit field<br>Returns the bit starting at 'offset' and ending at 'offset+width-1'.<br>The final result is sign-extended.<br>If (src2[4:0] == 0)<br>   dst = 0;<br>Else if (src2[4:0] + src1[4:0] < 32)<br>   dst = (src0 << (32-src1[4:0] - src2{4:0])) >>> (32 - src2[4:0])<br>Else <br>   dst = src0 >>> src1[4:0]<br>>>> means arithmetic shift right.<br>SCC = 1 if result is non-zero. Test sign-extended result.", @"", ISA_Enc.SOP2, 40, 0, 0x94000000, 0x0081),
new InstInfo(0354, "s_bfe_i64", "s8i", "s8i", "s8i", "none", "none", "none", "none", 3, 3, @"Bit field extract. S0 is data, S1[5:0] is field offset, S1[22:16] is field width. D.i = (S0.u >> S1.u[5:0]) & ((1 << S1.u[22:16]) - 1). SCC = 1 if result is non-zero. Test sign-extended result.", @"", ISA_Enc.SOP2, 42, 0, 0x95000000, 0x0081),
new InstInfo(0355, "s_bfe_u32", "s4u", "s4u", "s4u", "none", "none", "none", "none", 3, 3, @"DX11 Unsigned bitfield extract. Extract a contiguous range of bits from 32-bit source. <br>SRC0 = input data<br>SRC1 = the lowest bit position to select<br>SRC2 = the width of the bit field<br>Returns the bit starting at 'offset' and ending at 'offset+width-1'.<br>If (src2[4:0] == 0)<br>   dst = 0;<br>Else if (src2[4:0] + src1[4:0] < 32) {<br>   dst = (src0 << (32-src1[4:0] - src2{4:0])) >> (32 - src2[4:0])<br>Else <br>   dst = src0 >> src1[4:0]<br>SCC = 1 if result is non-zero. Test sign-extended result.", @"", ISA_Enc.SOP2, 39, 0, 0x93800000, 0x0081),
new InstInfo(0356, "s_bfe_u64", "s8u", "s8u", "s8u", "none", "none", "none", "none", 3, 3, @"Bit field extract. S0 is data, S1[4:0] is field offset, S1[22:16] is field width. D.u = (S0.u >> S1.u[5:0]) & ((1 << S1.u[22:16]) - 1). SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 41, 0, 0x94800000, 0x0081),
new InstInfo(0357, "s_bfm_b32", "s4b", "s4b", "s4b", "none", "none", "none", "none", 3, 3, @"D.u = ((1 << S0.u[4:0]) - 1) << S1.u[4:0]; bitfield mask.", @"", ISA_Enc.SOP2, 36, 0, 0x92000000, 0x0001),
new InstInfo(0358, "s_bfm_b64", "s8b", "s8b", "s8b", "none", "none", "none", "none", 3, 3, @"D.u = ((1 << S0.u[5:0]) - 1) << S1.u[5:0]; bitfield mask.", @"", ISA_Enc.SOP2, 37, 0, 0x92800000, 0x0001),
new InstInfo(0359, "s_bitcmp0_b32", "scc", "s4b", "s1u", "none", "none", "none", "none", 3, 3, @"SCC = (S0.u[S1.u[4:0]] == 0).", @"", ISA_Enc.SOPC, 12, 0, 0xBF0C0000, 0x0081),
new InstInfo(0360, "s_bitcmp0_b64", "scc", "s8b", "s1u", "none", "none", "none", "none", 3, 3, @"SCC = (S0.u[S1.u[5:0]] == 0).", @"", ISA_Enc.SOPC, 14, 0, 0xBF0E0000, 0x0081),
new InstInfo(0361, "s_bitcmp1_b32", "scc", "s4b", "s1u", "none", "none", "none", "none", 3, 3, @"SCC = (S0.u[S1.u[4:0]] == 1).", @"", ISA_Enc.SOPC, 13, 0, 0xBF0D0000, 0x0081),
new InstInfo(0362, "s_bitcmp1_b64", "scc", "s8b", "s1u", "none", "none", "none", "none", 3, 3, @"SCC = (S0.u[S1.u[5:0]] == 1).", @"", ISA_Enc.SOPC, 15, 0, 0xBF0F0000, 0x0081),
new InstInfo(0363, "s_bitset0_b32", "s4[biu]", "s4b", "none", "none", "none", "none", "none", 2, 2, @"D.u[S0.u[4:0]] = 0.", @"", ISA_Enc.SOP1, 27, 0, 0xBE801B00, 0x0001),
new InstInfo(0364, "s_bitset0_b64", "s4[biu]", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.u[S0.u[5:0]] = 0.", @"", ISA_Enc.SOP1, 28, 0, 0xBE801C00, 0x0001),
new InstInfo(0365, "s_bitset1_b32", "s4[biu]", "s4b", "none", "none", "none", "none", "none", 2, 2, @"D.u[S0.u[4:0]] = 1.", @"", ISA_Enc.SOP1, 29, 0, 0xBE801D00, 0x0001),
new InstInfo(0366, "s_bitset1_b64", "s4[biu]", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.u[S0.u[5:0]] = 1.", @"", ISA_Enc.SOP1, 30, 0, 0xBE801E00, 0x0001),
new InstInfo(0367, "s_branch", "none", "none", "none", "none", "none", "16i", "none", 1, 1, @"PC = PC + signext(SIMM16 * 4) + 4.", @"", ISA_Enc.SOPP, 2, 0, 0xBF820000, 0x0600),
new InstInfo(0368, "s_brev_b32", "s4b", "s4b", "none", "none", "none", "none", "none", 2, 2, @"D.u = S0.u[0:31] (reverse bits).", @"", ISA_Enc.SOP1, 11, 0, 0xBE800B00, 0x0081),
new InstInfo(0369, "s_brev_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.u = S0.u[0:63] (reverse bits).", @"", ISA_Enc.SOP1, 12, 0, 0xBE800C00, 0x0081),
new InstInfo(0370, "s_buffer_load_dword", "s4b", "s4u", "none", "none", "none", "none", "none", 2, 3, @"Read one Dword from read-only memory describe by a buffer a constant (V#) through the constant cache (kcache).<br>m_offset = IMM ? OFFSET : SGPR[OFFSET] <br>m_base = { SGPR[SBASE * 2 +1][15:0], SGPR[SBASE] }<br>m_stride = SGPR[SBASE * 2 +1][31:16]<br>m_num_records = SGPR[SBASE * 2 + 2]<br>m_size = (m_stride == 0) ? 1 : m_num_records<br>m_addr = (SGPR[SBASE * 2] + m_offset) & ~0x3<br>SGPR[SDST] = read_dword_from_kcache(m_base, m_offset, m_size)", @"", ISA_Enc.SMEM, 8, 0, 0xC2000000, 0x0001),
new InstInfo(0371, "s_buffer_load_dwordx16", "s64b", "s4u", "none", "none", "none", "none", "none", 2, 3, @"Read 16 Dwords from read-only memory describe by a buffer a constant (V#) through the constant cache (kcache).<br>m_offset = IMM ? OFFSET : SGPR[OFFSET] <br>m_base = { SGPR[SBASE * 2 +1][15:0], SGPR[SBASE * 2] }<br>m_stride = SGPR[SBASE * 2 +1][31:16]<br>m_num_records = SGPR[SBASE * 2 + 2]<br>m_size = (m_stride == 0) ? 1 : m_num_records<br>m_addr = (SGPR[SBASE * 2] + m_offset) & ~0x3<br>SGPR[SDST] = read_dword_from_kcache(m_base, m_offset, m_size)<br>SGPR[SDST + 1] = read_dword_from_kcache(m_base, m_offset + 4, m_size)<br>SGPR[SDST + 2] = read_dword_from_kcache(m_base, m_offset + 8, m_size)<br>. . .<br>SGPR[SDST + 15] = read_dword_from_kcache(m_base, m_offset + 60, m_size)", @"", ISA_Enc.SMEM, 12, 0, 0xC3000000, 0x0001),
new InstInfo(0372, "s_buffer_load_dwordx2", "s8b", "s4u", "none", "none", "none", "none", "none", 2, 3, @"Read two Dwords from read-only memory describe by a buffer a constant (V#) through the constant cache (kcache).<br>m_offset = IMM ? OFFSET : SGPR[OFFSET] <br>m_base = { SGPR[SBASE * 2 +1][15:0], SGPR[SBASE * 2] }<br>m_stride = SGPR[SBASE * 2 +1][31:16]<br>m_num_records = SGPR[SBASE * 2 + 2]<br>m_size = (m_stride == 0) ? 1 : m_num_records<br>m_addr = (SGPR[SBASE * 2] + m_offset) & ~0x3<br>SGPR[SDST] = read_dword_from_kcache(m_base, m_offset, m_size)<br>SGPR[SDST + 1] = read_dword_from_kcache(m_base, m_offset + 4, m_size)", @"", ISA_Enc.SMEM, 9, 0, 0xC2400000, 0x0001),
new InstInfo(0373, "s_buffer_load_dwordx4", "s16b", "s4u", "none", "none", "none", "none", "none", 2, 3, @"Read four Dwords from read-only memory describe by a buffer a constant (V#) through the constant cache (kcache).<br>m_offset = IMM ? OFFSET : SGPR[OFFSET] <br>m_base = { SGPR[SBASE * 2 +1][15:0], SGPR[SBASE * 2] }<br>m_stride = SGPR[SBASE * 2 +1][31:16]<br>m_num_records = SGPR[SBASE * 2 + 2]<br>m_size = (m_stride == 0) ? 1 : m_num_records<br>m_addr = (SGPR[SBASE * 2] + m_offset) & ~0x3<br>SGPR[SDST] = read_dword_from_kcache(m_base, m_offset, m_size)<br>SGPR[SDST + 1] = read_dword_from_kcache(m_base, m_offset + 4, m_size)<br>SGPR[SDST + 2] = read_dword_from_kcache(m_base, m_offset + 8, m_size)<br>SGPR[SDST + 3] = read_dword_from_kcache(m_base, m_offset + 12, m_size)", @"", ISA_Enc.SMEM, 10, 0, 0xC2800000, 0x0001),
new InstInfo(0374, "s_buffer_load_dwordx8", "s32b", "s4u", "none", "none", "none", "none", "none", 2, 3, @"Read eight Dwords from read-only memory describe by a buffer a constant (V#) through the constant cache (kcache).<br>m_offset = IMM ? OFFSET : SGPR[OFFSET] <br>m_base = { SGPR[SBASE * 2 +1][15:0], SGPR[SBASE * 2] }<br>m_stride = SGPR[SBASE * 2 +1][31:16]<br>m_num_records = SGPR[SBASE * 2 + 2]<br>m_size = (m_stride == 0) ? 1 : m_num_records<br>m_addr = (SGPR[SBASE * 2] + m_offset) & ~0x3<br>SGPR[SDST] = read_dword_from_kcache(m_base, m_offset, m_size)<br>SGPR[SDST + 1] = read_dword_from_kcache(m_base, m_offset + 4, m_size)<br>SGPR[SDST + 2] = read_dword_from_kcache(m_base, m_offset + 8, m_size)<br>. . .<br>SGPR[SDST + 7] = read_dword_from_kcache(m_base, m_offset + 28, m_size)", @"", ISA_Enc.SMEM, 11, 0, 0xC2C00000, 0x0001),
new InstInfo(0375, "s_cbranch_cdbgsys", "none", "none", "none", "none", "none", "16i", "none", 1, 1, @"Conditional branch when the SYStem debug bit is set.<br>if(conditional_debug_system != 0) then PC = PC + signext(SIMM16 * 4) + 4; else NOP.", @"", ISA_Enc.SOPP, 23, 0, 0xBF970000, 0x0600),
new InstInfo(0376, "s_cbranch_cdbgsys_and_user", "none", "none", "none", "none", "none", "16i", "none", 1, 1, @"Conditional branch when both the SYStem and USER debug bits are set.<br>if(conditional_debug_system && conditional_debug_user) then PC = PC + signext(SIMM16 * 4) + 4; else NOP.", @"", ISA_Enc.SOPP, 26, 0, 0xBF9A0000, 0x0600),
new InstInfo(0377, "s_cbranch_cdbgsys_or_user", "none", "none", "none", "none", "none", "16i", "none", 1, 1, @"Conditional branch when either the SYStem or USER debug bits are set.<br>if(conditional_debug_system || conditional_debug_user) then PC = PC + signext(SIMM16 * 4) + 4; else NOP.", @"", ISA_Enc.SOPP, 25, 0, 0xBF990000, 0x0600),
new InstInfo(0378, "s_cbranch_cdbguser", "none", "none", "none", "none", "none", "16i", "none", 1, 1, @"Conditional branch when the USER debug bit is set.<br>if(conditional_debug_user != 0) then PC = PC + signext(SIMM16 * 4) + 4; else NOP.", @"", ISA_Enc.SOPP, 24, 0, 0xBF980000, 0x0600),
new InstInfo(0379, "s_cbranch_execnz", "none", "none", "none", "none", "none", "16i", "none", 1, 1, @"if(EXEC != 0) then PC = PC + signext(SIMM16 * 4) + 4; else nop.", @"", ISA_Enc.SOPP, 9, 0, 0xBF890000, 0x0618),
new InstInfo(0380, "s_cbranch_execz", "none", "none", "none", "none", "none", "16i", "none", 1, 1, @"if(EXEC == 0) then PC = PC + signext(SIMM16 * 4) + 4; else nop.", @"", ISA_Enc.SOPP, 8, 0, 0xBF880000, 0x0618),
new InstInfo(0381, "s_cbranch_g_fork", "s8b", "s8b", "s8b", "none", "none", "none", "none", 3, 3, @"Conditional branch using branch stack. Arg0 = compare mask (VCC or any SGPR), Arg1 = 64-bit byte address of target instruction. See Section 4.6, on page 4-4.", @"", ISA_Enc.SOP2, 43, 0, 0x95800000, 0x0219),
new InstInfo(0382, "s_cbranch_i_fork", "s16b", "s4u", "none", "none", "none", "s16", "none", 3, 3, @"Conditional branch using branch-stack. Arg0(sdst) = compare mask (VCC or any SGPR), SIMM16 = signed DWORD branch offset relative to next instruction. See Section 4.6, on page 4-4.S_CBRANCH_I_FORK arg0, #target_addr_offset[17:2]// target_addr_offset is a 16b signed immediate offset;  “PC” in this pseudo-code is pointing to the cbranch_*_fork instruction  mask_pass = SGPR[arg0] & exec  mask_fail = ~SGPR[arg0] & exec  if (mask_pass == exec)    PC += 4 + target_addr_offset  else if (mask_fail == exec)    PC += 4  else if (bitcount(mask_fail) < bitcount(mask_pass))    exec = mask_fail    SGPR[CSP*4] = { (pc + 4 + target_addr_offset), mask_pass }    CSP++    PC += 4  else    exec = mask_pass    SGPR[CSP*4] = { (pc+4), mask_fail }    CSP++    PC += 4 + target_addr_offset", @"", ISA_Enc.SOPK, 17, 0, 0xB8800000, 0x0618),
new InstInfo(0383, "s_cbranch_join", "none", "s16b", "none", "none", "none", "none", "none", 1, 1, @"Conditional branch join point. Arg0 = saved CSP value. No dest. See Section 4.6, on page 4-4.Format: S_CBRANCH_JOIN arg0if (CSP == SGPR[arg0])// SGPR[arg0] holds the CSP value when the FORK startedPC += 4// this is the 2nd time to JOIN: continue with pgmelseCSP --// this is the 1st time to JOIN: jump to other FORK path{PC, EXEC} = SGPR[CSP*4] // read 128-bits from 4 consecutive SGPRs", @"", ISA_Enc.SOP1, 50, 0, 0xBE803200, 0x0619),
new InstInfo(0384, "s_cbranch_scc0", "none", "none", "none", "none", "none", "16i", "none", 1, 1, @"if(SCC == 0) then PC = PC + signext(SIMM16 * 4) + 4; else nop.", @"", ISA_Enc.SOPP, 4, 0, 0xBF840000, 0x0700),
new InstInfo(0385, "s_cbranch_scc1", "none", "none", "none", "none", "none", "16i", "none", 1, 1, @"if(SCC == 1) then PC = PC + signext(SIMM16 * 4) + 4; else nop.", @"", ISA_Enc.SOPP, 5, 0, 0xBF850000, 0x0700),
new InstInfo(0386, "s_cbranch_vccnz", "none", "none", "none", "none", "none", "16i", "none", 1, 1, @"if(VCC != 0) then PC = PC + signext(SIMM16 * 4) + 4; else nop.", @"", ISA_Enc.SOPP, 7, 0, 0xBF870000, 0x0640),
new InstInfo(0387, "s_cbranch_vccz", "none", "none", "none", "none", "none", "16i", "none", 1, 1, @"if(VCC == 0) then PC = PC + signext(SIMM16 * 4) + 4; else nop.", @"", ISA_Enc.SOPP, 6, 0, 0xBF860000, 0x0640),
new InstInfo(0388, "s_cmov_b32", "s4b", "s4b", "none", "none", "none", "none", "none", 2, 2, @"if(SCC) D.u = S0.u; else NOP.", @"", ISA_Enc.SOP1, 5, 0, 0xBE800500, 0x0181),
new InstInfo(0389, "s_cmov_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"if(SCC) D.u = S0.u; else NOP.", @"", ISA_Enc.SOP1, 6, 0, 0xBE800600, 0x0181),
new InstInfo(0390, "s_cmovk_i32", "s4i", "none", "none", "none", "none", "s16", "none", 2, 2, @"if (SCC) D.i = signext(SIMM16); else NOP.", @"", ISA_Enc.SOPK, 2, 0, 0xB1000000, 0x0080),
new InstInfo(0391, "s_cmp_eq_i32", "scc", "s4[iu]", "s4[iu]", "none", "none", "none", "none", 3, 3, @"SCC = (S0.i == S1.i).", @"uint only works here if values are <2147483648(aka top bit must be 0). If larger is needed use U32 version. ", ISA_Enc.SOPC, 0, 0, 0xBF000000, 0x0081),
new InstInfo(0392, "s_cmp_eq_u32", "scc", "s4u", "s4u", "none", "none", "none", "none", 3, 3, @"SCC = (S0.u == S1.u).", @"", ISA_Enc.SOPC, 6, 0, 0xBF060000, 0x0081),
new InstInfo(0393, "s_cmp_ge_i32", "scc", "s4[iu]", "s4[iu]", "none", "none", "none", "none", 3, 3, @"SCC = (S0.i >= S1.i).", @"uint only works here if values are <2147483648(aka top bit must be 0). If larger is needed use U32 version. ", ISA_Enc.SOPC, 3, 0, 0xBF030000, 0x0081),
new InstInfo(0394, "s_cmp_ge_u32", "scc", "s4u", "s4u", "none", "none", "none", "none", 3, 3, @"SCC = (S0.u >= S1.u).", @"", ISA_Enc.SOPC, 9, 0, 0xBF090000, 0x0081),
new InstInfo(0395, "s_cmp_gt_i32", "scc", "s4[iu]", "s4[iu]", "none", "none", "none", "none", 3, 3, @"SCC = (S0.i > S1.i).", @"uint only works here if values are <2147483648(aka top bit must be 0). If larger is needed use U32 version. ", ISA_Enc.SOPC, 2, 0, 0xBF020000, 0x0081),
new InstInfo(0396, "s_cmp_gt_u32", "scc", "s4u", "s4u", "none", "none", "none", "none", 3, 3, @"SCC = (S0.u > S1.u).", @"", ISA_Enc.SOPC, 8, 0, 0xBF080000, 0x0081),
new InstInfo(0397, "s_cmp_le_i32", "scc", "s4[iu]", "s4[iu]", "none", "none", "none", "none", 3, 3, @"SCC = (S0.i <= S1.i).", @"uint only works here if values are <2147483648(aka top bit must be 0). If larger is needed use U32 version. ", ISA_Enc.SOPC, 5, 0, 0xBF050000, 0x0081),
new InstInfo(0398, "s_cmp_le_u32", "scc", "s4u", "s4u", "none", "none", "none", "none", 3, 3, @"SCC = (S0.u <= S1.u).", @"", ISA_Enc.SOPC, 11, 0, 0xBF0B0000, 0x0081),
new InstInfo(0399, "s_cmp_lg_i32", "scc", "s4[iu]", "s4[iu]", "none", "none", "none", "none", 3, 3, @"SCC = (S0.i != S1.i).", @"uint only works here if values are <2147483648(aka top bit must be 0). If larger is needed use U32 version. ", ISA_Enc.SOPC, 1, 0, 0xBF010000, 0x0081),
new InstInfo(0400, "s_cmp_lg_u32", "scc", "s4u", "s4u", "none", "none", "none", "none", 3, 3, @"SCC = (S0.u != S1.u).", @"", ISA_Enc.SOPC, 7, 0, 0xBF070000, 0x0081),
new InstInfo(0401, "s_cmp_lt_i32", "scc", "s4[iu]", "s4[iu]", "none", "none", "none", "none", 3, 3, @"SCC = (S0.i < S1.i).", @"uint only works here if values are <2147483648(aka top bit must be 0). If larger is needed use U32 version. ", ISA_Enc.SOPC, 4, 0, 0xBF040000, 0x0081),
new InstInfo(0402, "s_cmp_lt_u32", "scc", "s4u", "s4u", "none", "none", "none", "none", 3, 3, @"SCC = (S0.u < S1.u).", @"", ISA_Enc.SOPC, 10, 0, 0xBF0A0000, 0x0081),
new InstInfo(0403, "s_cmpk_eq_i32", "s4i", "none", "none", "none", "none", "s16", "none", 2, 2, @"SCC = (D.i == signext(SIMM16)).", @"", ISA_Enc.SOPK, 3, 0, 0xB1800000, 0x0080),
new InstInfo(0404, "s_cmpk_eq_u32", "s4u", "none", "none", "none", "none", "u16", "none", 2, 2, @"SCC = (D.u == SIMM16).", @"", ISA_Enc.SOPK, 9, 0, 0xB4800000, 0x0080),
new InstInfo(0405, "s_cmpk_ge_i32", "s4i", "none", "none", "none", "none", "s16", "none", 2, 2, @"SCC = (D.i >= signext(SIMM16)).", @"", ISA_Enc.SOPK, 6, 0, 0xB3000000, 0x0080),
new InstInfo(0406, "s_cmpk_ge_u32", "s4u", "none", "none", "none", "none", "u16", "none", 2, 2, @"SCC = (D.u >= SIMM16).", @"", ISA_Enc.SOPK, 12, 0, 0xB6000000, 0x0080),
new InstInfo(0407, "s_cmpk_gt_i32", "s4i", "none", "none", "none", "none", "s16", "none", 2, 2, @"SCC = (D.i > signext(SIMM16)).", @"", ISA_Enc.SOPK, 5, 0, 0xB2800000, 0x0080),
new InstInfo(0408, "s_cmpk_gt_u32", "s4u", "none", "none", "none", "none", "u16", "none", 2, 2, @"SCC = (D.u > SIMM16).", @"", ISA_Enc.SOPK, 11, 0, 0xB5800000, 0x0080),
new InstInfo(0409, "s_cmpk_le_i32", "s4i", "none", "none", "none", "none", "s16", "none", 2, 2, @"SCC = (D.i <= signext(SIMM16)).", @"", ISA_Enc.SOPK, 8, 0, 0xB4000000, 0x0080),
new InstInfo(0410, "s_cmpk_le_u32", "s4u", "none", "none", "none", "none", "u16", "none", 2, 2, @"D.u = SCC = (D.u <= SIMM16).", @"", ISA_Enc.SOPK, 14, 0, 0xB7000000, 0x0080),
new InstInfo(0411, "s_cmpk_lg_i32", "s4i", "none", "none", "none", "none", "s16", "none", 2, 2, @"SCC = (D.i != signext(SIMM16)).", @"", ISA_Enc.SOPK, 4, 0, 0xB2000000, 0x0080),
new InstInfo(0412, "s_cmpk_lg_u32", "s4u", "none", "none", "none", "none", "u16", "none", 2, 2, @"SCC = (D.u != SIMM16).", @"", ISA_Enc.SOPK, 10, 0, 0xB5000000, 0x0080),
new InstInfo(0413, "s_cmpk_lt_i32", "s4i", "none", "none", "none", "none", "s16", "none", 2, 2, @"SCC = (D.i < signext(SIMM16)).", @"", ISA_Enc.SOPK, 7, 0, 0xB3800000, 0x0080),
new InstInfo(0414, "s_cmpk_lt_u32", "s4u", "none", "none", "none", "none", "u16", "none", 2, 2, @"SCC = (D.u < SIMM16).", @"", ISA_Enc.SOPK, 13, 0, 0xB6800000, 0x0080),
new InstInfo(0415, "s_cselect_b32", "s4b", "s4b", "s4b", "scc", "none", "none", "none", 4, 4, @"D.u = SCC ? S0.u : S1.u.", @"", ISA_Enc.SOP2, 10, 0, 0x85000000, 0x0101),
new InstInfo(0416, "s_cselect_b64", "s8b", "s8b", "s8b", "scc", "none", "none", "none", 4, 4, @"D.u = SCC ? S0.u : S1.u.", @"", ISA_Enc.SOP2, 11, 0, 0x85800000, 0x0101),
new InstInfo(0417, "s_dcache_inv", "none", "none", "none", "none", "none", "none", "none", 0, 0, @"Invalidate entire L1 constant cache.", @"", ISA_Enc.SMEM, 31, 0, 0xC7C00000, 0x0000),
new InstInfo(0418, "s_dcache_inv_vol", "none", "none", "none", "none", "none", "none", "none", 0, 0, @"Invalidate all volatile lines in L1 constant cache.", @"", ISA_Enc.SMEM, 29, 0, 0xC7400000, 0x0000),
new InstInfo(0419, "s_decperflevel", "none", "none", "none", "none", "none", "4u", "none", 1, 1, @"Decrement performance counter specified in SIMM16[3:0] by 1.", @"", ISA_Enc.SOPP, 21, 0, 0xBF950000, 0x0000),
new InstInfo(0420, "s_endpgm", "none", "none", "none", "none", "none", "none", "none", 0, 0, @"End of program; terminate wavefront.", @"", ISA_Enc.SOPP, 1, 0, 0xBF810000, 0x0000),
new InstInfo(0421, "s_ff0_i32_b32", "s4i", "s4b", "none", "none", "none", "none", "none", 2, 2, @"D.i = FindFirstZero(S0.u) from LSB; if no zeros, return -1.", @"", ISA_Enc.SOP1, 17, 0, 0xBE801100, 0x0001),
new InstInfo(0422, "s_ff0_i32_b64", "s4i", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.i = FindFirstZero(S0.u) from LSB; if no zeros, return -1.", @"", ISA_Enc.SOP1, 18, 0, 0xBE801200, 0x0001),
new InstInfo(0423, "s_ff1_i32_b32", "s4i", "s4b", "none", "none", "none", "none", "none", 2, 2, @"D.i = FindFirstOne(S0.u) from LSB; if no ones, return -1.", @"", ISA_Enc.SOP1, 19, 0, 0xBE801300, 0x0001),
new InstInfo(0424, "s_ff1_i32_b64", "s4i", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.i = FindFirstOne(S0.u) from LSB; if no ones, return -1.", @"", ISA_Enc.SOP1, 20, 0, 0xBE801400, 0x0001),
new InstInfo(0425, "s_flbit_i32", "s4i", "s4i", "none", "none", "none", "none", "none", 2, 2, @"D.i = Find first bit opposite of sign bit from MSB. If S0 == -1, return -1.", @"", ISA_Enc.SOP1, 23, 0, 0xBE801700, 0x0001),
new InstInfo(0426, "s_flbit_i32_b32", "s4i", "s4b", "none", "none", "none", "none", "none", 2, 2, @"D.i = FindFirstOne(S0.u) from MSB; if no ones, return -1.", @"", ISA_Enc.SOP1, 21, 0, 0xBE801500, 0x0001),
new InstInfo(0427, "s_flbit_i32_b64", "s4i", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.i = FindFirstOne(S0.u) from MSB; if no ones, return -1.", @"", ISA_Enc.SOP1, 22, 0, 0xBE801600, 0x0001),
new InstInfo(0428, "s_flbit_i32_i64", "s4i", "s8i", "none", "none", "none", "none", "none", 2, 2, @"D.i = Find first bit opposite of sign bit from MSB. If S0 == -1, return -1.", @"", ISA_Enc.SOP1, 24, 0, 0xBE801800, 0x0001),
new InstInfo(0429, "s_getpc_b64", "s8u", "none", "none", "none", "none", "none", "none", 1, 1, @"D.u = PC + 4; destination receives the byte address of the next instruction.", @"", ISA_Enc.SOP1, 31, 0, 0xBE801F00, 0x0400),
new InstInfo(0430, "s_getreg_b32", "s4b", "none", "none", "none", "none", "b16", "none", 2, 2, @"D.u = hardware register. Read some or all of a hardware register into the LSBs of D. See Table 5.7 on page 5-7. SIMM16 = {size[4:0], offset[4:0], hwRegId[5:0]}; offset is in the range from 0 to 31, size is in the range from 1 to 32.", @"", ISA_Enc.SOPK, 18, 0, 0xB9000000, 0x0000),
new InstInfo(0431, "s_icache_inv", "none", "none", "none", "none", "none", "none", "none", 0, 0, @"Invalidate entire L1 instruction cache.", @"", ISA_Enc.SOPP, 19, 0, 0xBF930000, 0x0000),
new InstInfo(0432, "s_incperflevel", "none", "none", "none", "none", "none", "4u", "none", 1, 1, @"Increment performance counter specified in SIMM16[3:0] by 1.", @"", ISA_Enc.SOPP, 20, 0, 0xBF940000, 0x0000),
new InstInfo(0433, "s_load_dword", "s4b", "s4u", "none", "none", "none", "none", "none", 2, 3, @"Read two Dwords from read-only constant memory through the constant cache (kcache).<br>m_offset = IMM ? OFFSET : SGPR[OFFSET] <br>m_addr = (SGPR[SBASE * 2] + m_offset) & ~0x3<br>SGPR[SDST] = read_dword_from_kcache(m_addr)<br>SGPR[SDST+1] = read_dword_from_kcache(m_addr+4)", @"", ISA_Enc.SMEM, 0, 0, 0xC0000000, 0x0001),
new InstInfo(0434, "s_load_dwordx16", "s64b", "s4u", "none", "none", "none", "none", "none", 2, 3, @"Read 16 Dwords from read-only constant memory through the constant cache (kcache).<br>m_offset = IMM ? OFFSET : SGPR[OFFSET] <br>m_addr = (SGPR[SBASE * 2] + m_offset) & ~0x3<br>SGPR[SDST] = read_dword_from_kcache(m_addr)<br>SGPR[SDST+1] = read_dword_from_kcache(m_addr+4)<br>SGPR[SDST+2] = read_dword_from_kcache(m_addr+8)<br>. . .<br>SGPR[SDST+15] = read_dword_from_kcache(m_addr+60)", @"", ISA_Enc.SMEM, 4, 0, 0xC1000000, 0x0001),
new InstInfo(0435, "s_load_dwordx2", "s8b", "s4u", "none", "none", "none", "none", "none", 2, 3, @"Read two Dwords from read-only constant memory through the constant cache (kcache).<br>m_offset = IMM ? OFFSET : SGPR[OFFSET] <br>m_addr = (SGPR[SBASE * 2] + m_offset) & ~0x3<br>SGPR[SDST] = read_dword_from_kcache(m_addr)<br>SGPR[SDST+1] = read_dword_from_kcache(m_addr+4)", @"", ISA_Enc.SMEM, 1, 0, 0xC0400000, 0x0001),
new InstInfo(0436, "s_load_dwordx4", "s16b", "s4u", "none", "none", "none", "none", "none", 2, 3, @"Read four Dwords from read-only constant memory through the constant cache (kcache).<br>m_offset = IMM ? OFFSET : SGPR[OFFSET] <br>m_addr = (SGPR[SBASE * 2] + m_offset) & ~0x3<br>SGPR[SDST] = read_dword_from_kcache(m_addr)<br>SGPR[SDST+1] = read_dword_from_kcache(m_addr+4)<br>SGPR[SDST+2] = read_dword_from_kcache(m_addr+8)<br>SGPR[SDST+3] = read_dword_from_kcache(m_addr+12)", @"", ISA_Enc.SMEM, 2, 0, 0xC0800000, 0x0001),
new InstInfo(0437, "s_load_dwordx8", "s32b", "s4u", "none", "none", "none", "none", "none", 2, 3, @"Read eight Dwords from read-only constant memory through the constant cache (kcache).<br>m_offset = IMM ? OFFSET : SGPR[OFFSET] <br>m_addr = (SGPR[SBASE * 2] + m_offset) & ~0x3<br>SGPR[SDST] = read_dword_from_kcache(m_addr)<br>SGPR[SDST+1] = read_dword_from_kcache(m_addr+4)<br>SGPR[SDST+2] = read_dword_from_kcache(m_addr+8)<br>. . .<br>SGPR[SDST+7] = read_dword_from_kcache(m_addr+28)", @"", ISA_Enc.SMEM, 3, 0, 0xC0C00000, 0x0001),
new InstInfo(0438, "s_lshl_b32", "s4b", "s4b", "s4b", "none", "none", "none", "none", 3, 3, @"D.u = S0.u << S1.u[4:0]. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 30, 0, 0x8F000000, 0x0081),
new InstInfo(0439, "s_lshl_b64", "s8b", "s8b", "s8b", "none", "none", "none", "none", 3, 3, @"D.u = S0.u << S1.u[5:0]. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 31, 0, 0x8F800000, 0x0081),
new InstInfo(0440, "s_lshr_b32", "s4b", "s4b", "s4b", "none", "none", "none", "none", 3, 3, @"D.u = S0.u >> S1.u[4:0]. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 32, 0, 0x90000000, 0x0081),
new InstInfo(0441, "s_lshr_b64", "s8b", "s8b", "s8b", "none", "none", "none", "none", 3, 3, @"D.u = S0.u >> S1.u[5:0]. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 33, 0, 0x90800000, 0x0081),
new InstInfo(0442, "s_max_i32", "s4i", "s4i", "s4i", "none", "none", "none", "none", 3, 3, @"D.i = (S0.i > S1.i) ? S0.i : S1.i. SCC = 1 if S0 is max.", @"", ISA_Enc.SOP2, 8, 0, 0x84000000, 0x0081),
new InstInfo(0443, "s_max_u32", "s4u", "s4u", "s4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0.u > S1.u) ? S0.u : S1.u. SCC = 1 if S0 is max.", @"", ISA_Enc.SOP2, 9, 0, 0x84800000, 0x0081),
new InstInfo(0444, "s_memtime", "s8u", "none", "none", "none", "none", "none", "none", 1, 1, @"Return current 64-bit timestamp.This 'time' is a free-running clock counter based on the shader core clock.", @"", ISA_Enc.SMEM, 30, 0, 0xC7800000, 0x0000),
new InstInfo(0445, "s_min_i32", "s4i", "s4i", "s4i", "none", "none", "none", "none", 3, 3, @"D.i = (S0.i < S1.i) ? S0.i : S1.i. SCC = 1 if S0 is min.", @"", ISA_Enc.SOP2, 6, 0, 0x83000000, 0x0081),
new InstInfo(0446, "s_min_u32", "s4u", "s4u", "s4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0.u < S1.u) ? S0.u : S1.u. SCC = 1 if S0 is min.", @"", ISA_Enc.SOP2, 7, 0, 0x83800000, 0x0081),
new InstInfo(0447, "s_mov_b32", "s4b", "s4b", "none", "none", "none", "none", "none", 2, 2, @"D.u = S0.u.", @"", ISA_Enc.SOP1, 3, 0, 0xBE800300, 0x0001),
new InstInfo(0448, "s_mov_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"Du = S0.u.", @"", ISA_Enc.SOP1, 4, 0, 0xBE800400, 0x0001),
new InstInfo(0449, "s_mov_fed_b32", "s4b", "s4b", "none", "none", "none", "none", "none", 2, 2, @"D.u = S0.u, introduce edc double error upon write to dest sgpr.", @"", ISA_Enc.SOP1, 53, 0, 0xBE803500, 0x0001),
new InstInfo(0450, "s_movk_i32", "s4i", "none", "none", "none", "none", "s16", "none", 2, 2, @"D.i = signext(SIMM16).", @"", ISA_Enc.SOPK, 0, 0, 0xB0000000, 0x0000),
new InstInfo(0451, "s_movreld_b32", "s4b", "s4b", "none", "none", "none", "none", "none", 2, 2, @"SGPR[D.u + M0.u] = SGPR[S0.u].", @"", ISA_Enc.SOP1, 48, 0, 0xBE803000, 0x0001),
new InstInfo(0452, "s_movreld_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"SGPR[D.u + M0.u] = SGPR[S0.u]. M0 and D.u must be even.", @"", ISA_Enc.SOP1, 49, 0, 0xBE803100, 0x0001),
new InstInfo(0453, "s_movrels_b32", "s4b", "s4b", "none", "none", "none", "none", "none", 2, 2, @"SGPR[D.u] = SGPR[S0.u + M0.u].", @"", ISA_Enc.SOP1, 46, 0, 0xBE802E00, 0x0001),
new InstInfo(0454, "s_movrels_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"SGPR[D.u] = SGPR[S0.u + M0.u]. M0 and S0.u must be even.", @"", ISA_Enc.SOP1, 47, 0, 0xBE802F00, 0x0001),
new InstInfo(0455, "s_mul_i32", "s4i", "s4i", "s4i", "none", "none", "none", "none", 3, 3, @"D.i = S0.i * S1.i.", @"", ISA_Enc.SOP2, 38, 0, 0x93000000, 0x0001),
new InstInfo(0456, "s_mulk_i32", "s4i", "none", "none", "none", "none", "s16", "none", 2, 2, @"D.i = D.i * signext(SIMM16). SCC = overflow.", @"", ISA_Enc.SOPK, 16, 0, 0xB8000000, 0x0080),
new InstInfo(0457, "s_nand_b32", "s4b", "s4b", "s4b", "none", "none", "none", "none", 3, 3, @"D.u = ~(S0.u & S1.u). SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 24, 0, 0x8C000000, 0x0081),
new InstInfo(0458, "s_nand_b64", "s8b", "s8b", "s8b", "none", "none", "none", "none", 3, 3, @"D.u = ~(S0.u & S1.u). SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 25, 0, 0x8C800000, 0x0081),
new InstInfo(0459, "s_nand_saveexec_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.u = EXEC, EXEC = ~(S0.u & EXEC). SCC = 1 if the new value of EXEC is non-zero.", @"", ISA_Enc.SOP1, 41, 0, 0xBE802900, 0x0099),
new InstInfo(0460, "s_nop", "none", "none", "none", "none", "none", "3u", "none", 1, 1, @"Do nothing. Repeat NOP 1..8 times based on SIMM16[2:0]. 0 = 1 time, 7 = 8 times.", @"", ISA_Enc.SOPP, 0, 0, 0xBF800000, 0x0000),
new InstInfo(0461, "s_nor_b32", "s4b", "s4b", "s4b", "none", "none", "none", "none", 3, 3, @"D.u = ~(S0.u | S1.u). SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 26, 0, 0x8D000000, 0x0081),
new InstInfo(0462, "s_nor_b64", "s8b", "s8b", "s8b", "none", "none", "none", "none", 3, 3, @"D.u = ~(S0.u | S1.u). SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 27, 0, 0x8D800000, 0x0081),
new InstInfo(0463, "s_nor_saveexec_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.u = EXEC, EXEC = ~(S0.u | EXEC). SCC = 1 if the new value of EXEC is non-zero.", @"", ISA_Enc.SOP1, 42, 0, 0xBE802A00, 0x0099),
new InstInfo(0464, "s_not_b32", "s4b", "s4b", "none", "none", "none", "none", "none", 2, 2, @"D.u = ~S0.u SCC = 1 if result non-zero.", @"", ISA_Enc.SOP1, 7, 0, 0xBE800700, 0x0081),
new InstInfo(0465, "s_not_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.u = ~S0.u SCC = 1 if result non-zero.", @"", ISA_Enc.SOP1, 8, 0, 0xBE800800, 0x0081),
new InstInfo(0466, "s_or_b32", "s4b", "s4b", "s4b", "none", "none", "none", "none", 3, 3, @"D.u = S0.u | S1.u. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 16, 0, 0x88000000, 0x0081),
new InstInfo(0467, "s_or_b64", "s8b", "s8b", "s8b", "none", "none", "none", "none", 3, 3, @"D.u = S0.u | S1.u. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 17, 0, 0x88800000, 0x0081),
new InstInfo(0468, "s_or_saveexec_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.u = EXEC, EXEC = S0.u | EXEC. SCC = 1 if the new value of EXEC is non-zero.", @"", ISA_Enc.SOP1, 37, 0, 0xBE802500, 0x0099),
new InstInfo(0469, "s_orn2_b32", "s4b", "s4b", "s4b", "none", "none", "none", "none", 3, 3, @"D.u = S0.u | ~S1.u. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 22, 0, 0x8B000000, 0x0081),
new InstInfo(0470, "s_orn2_b64", "s8b", "s8b", "s8b", "none", "none", "none", "none", 3, 3, @"D.u = S0.u | ~S1.u. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 23, 0, 0x8B800000, 0x0081),
new InstInfo(0471, "s_orn2_saveexec_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.u = EXEC, EXEC = S0.u | ~EXEC. SCC = 1 if the new value of EXEC is non-zero.", @"", ISA_Enc.SOP1, 40, 0, 0xBE802800, 0x0099),
new InstInfo(0472, "s_quadmask_b32", "s4b", "s4b", "none", "none", "none", "none", "none", 2, 2, @"D.u = QuadMask(S0.u). D[0] = OR(S0[3:0]), D[1] = OR(S0[7:4]) .... SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP1, 44, 0, 0xBE802C00, 0x0081),
new InstInfo(0473, "s_quadmask_b64", "s4b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.u = QuadMask(S0.u). D[0] = OR(S0[3:0]), D[1] = OR(S0[7:4]) .... SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP1, 45, 0, 0xBE802D00, 0x0081),
new InstInfo(0474, "s_rfe_b64", "none", "s8u", "none", "none", "none", "none", "none", 1, 1, @"Return from Exception; PC = S0.u. This instruction sets PRIV to 0.", @"", ISA_Enc.SOP1, 34, 0, 0xBE802200, 0x0201),
new InstInfo(0475, "s_sendmsg", "none", "none", "none", "none", "none", "16b", "none", 1, 1, @"Send a message.", @"", ISA_Enc.SOPP, 16, 0, 0xBF900000, 0x0000),
new InstInfo(0476, "s_sendmsghalt", "none", "none", "none", "none", "none", "none", "none", 0, 0, @"Send a message and then HALT.", @"", ISA_Enc.SOPP, 17, 0, 0xBF910000, 0x0000),
new InstInfo(0477, "s_sethalt", "none", "none", "none", "none", "none", "1b", "none", 1, 1, @"set HALT bit to value of SIMM16[0]. 1=halt, 0=resume. Halt is ignored while priv=1.", @"", ISA_Enc.SOPP, 13, 0, 0xBF8D0000, 0x0000),
new InstInfo(0478, "s_setkill", "none", "none", "none", "none", "none", "1b", "none", 1, 1, @"Set KILL bit to value of SIMM16[0].", @"", ISA_Enc.SOPP, 11, 0, 0xBF8B0000, 0x0000),
new InstInfo(0479, "s_setpc_b64", "none", "s8u", "none", "none", "none", "none", "none", 1, 1, @"PC = S0.u; S0.u is a byte address of the instruction to jump to.", @"", ISA_Enc.SOP1, 32, 0, 0xBE802000, 0x0201),
new InstInfo(0480, "s_setprio", "none", "none", "none", "none", "none", "2u", "none", 1, 1, @"User-settable wave priority. The priority value is indicated in the two LSBs of the SIMM field. 0 = lowest, 3 = highest.", @"", ISA_Enc.SOPP, 15, 0, 0xBF8F0000, 0x0000),
new InstInfo(0481, "s_setreg_b32", "s4b", "none", "none", "none", "none", "b16", "none", 2, 2, @"Hardware register = D.u. Write some or all of the LSBs of D into a hardware register (note that D is a source SGPR). See Table 5.7 on page 5-7.<br>SIMM16 = {size[4:0], offset[4:0], hwRegId[5:0]}; offset is in the range from 0 to 31, size is in the range from 1 to 32.", @"", ISA_Enc.SOPK, 19, 0, 0xB9800000, 0x0000),
new InstInfo(0482, "s_setreg_imm32_b32", "s4b", "none", "none", "none", "none", "b16", "none", 2, 2, @"This instruction uses a 32-bit literal constant. Write some or all of the LSBs of SIMM32 into a hardware register.<br>SIMM16 = {size[4:0], offset[4:0], hwRegId[5:0]}; offset is 0-31, size is 1-32.", @"", ISA_Enc.SOPK, 21, 0, 0xBA800000, 0x0002),
new InstInfo(0483, "s_setvskip", "scc", "s8b", "s1u", "none", "none", "none", "none", 3, 3, @"VSKIP = S0.u[S1.u[4:0]]. Extract one bit from the SSRC0 SGPR, and use that bit to enable or disable VSKIP mode. In some cases, VSKIP mode can be used to skip over sections of code more quickly than branching. When VSKIP is enabled, the following instruction types are not executed: Vector ALU, Vector Memory, LDS, GDS, and Export.", @"", ISA_Enc.SOPC, 16, 0, 0xBF100000, 0x0001),
new InstInfo(0484, "s_sext_i32_i16", "s4i", "s8i", "none", "none", "none", "none", "none", 2, 2, @"D.i = signext(S0.i[15:0]).", @"", ISA_Enc.SOP1, 26, 0, 0xBE801A00, 0x0001),
new InstInfo(0485, "s_sext_i32_i8", "s4i", "s4i", "none", "none", "none", "none", "none", 2, 2, @"D.i = signext(S0.i[7:0]).", @"", ISA_Enc.SOP1, 25, 0, 0xBE801900, 0x0001),
new InstInfo(0486, "s_sleep", "none", "none", "none", "none", "none", "3u", "none", 1, 1, @"Cause a wave to sleep for approximately 64*SIMM16[2:0] clocks.", @"", ISA_Enc.SOPP, 14, 0, 0xBF8E0000, 0x0000),
new InstInfo(0487, "s_sub_i32", "s4i", "s4i", "s4i", "none", "none", "none", "none", 3, 3, @"D.u = S0.i - S1.i. SCC = borrow.", @"", ISA_Enc.SOP2, 3, 0, 0x81800000, 0x0081),
new InstInfo(0488, "s_sub_u32", "s4u", "s4u", "s4u", "none", "none", "none", "none", 3, 3, @"D.u = S0.u - S1.u. SCC = unsigned carry out.", @"", ISA_Enc.SOP2, 1, 0, 0x80800000, 0x0081),
new InstInfo(0489, "s_subb_u32", "s4u", "s4u", "s4u", "scc", "none", "none", "none", 4, 4, @"D.u = S0.u - S1.u - SCC. SCC = unsigned carry-out.", @"", ISA_Enc.SOP2, 5, 0, 0x82800000, 0x0181),
new InstInfo(0490, "s_swappc_b64", "s8u", "s8u", "none", "none", "none", "none", "none", 2, 2, @"D.u = PC + 4; PC = S0.u.", @"", ISA_Enc.SOP1, 33, 0, 0xBE802100, 0x0601),
new InstInfo(0491, "s_trap", "none", "none", "none", "none", "none", "8u", "none", 1, 1, @"Enter the trap handler.  TrapID = SIMM16[7:0]. Wait for all instructions to complete, save {pc_rewind,trapID,pc} into ttmp0,1; load TBA into PC, set PRIV=1 and continue. A trapID of zero is not allowed.", @"", ISA_Enc.SOPP, 18, 0, 0xBF920000, 0x0000),
new InstInfo(0492, "s_ttracedata", "none", "none", "none", "none", "none", "16i", "none", 1, 1, @"Send M0 as user data to thread-trace.", @"", ISA_Enc.SOPP, 22, 0, 0xBF960000, 0x0000),
new InstInfo(0493, "s_waitcnt", "none", "none", "none", "none", "none", "16b", "none", 1, 1, @"Wait for count of outstanding lds, vector-memory and export/vmem-write-data to be at or below the specified levels. simm16[3:0] = vmcount, simm16[6:4] = export/mem-write-data count, simm16[12:8] = LGKM_cnt (scalar-mem/GDS/LDS count). See Section 4.4, on page 4-2.", @"", ISA_Enc.SOPP, 12, 0, 0xBF8C0000, 0x0000),
new InstInfo(0494, "s_wqm_b32", "s4b", "s4b", "none", "none", "none", "none", "none", 2, 2, @"D.u = WholeQuadMode(S0.u). SCC = 1 if result is non-zero.<br>Apply whole quad mode to the bitmask specified in SSRC0. Whole quad mode checks each group of four bits in the bitmask; if any bit is set to 1, all four bits are set to 1 in the result. This operation is repeated for the entire bitmask.", @"", ISA_Enc.SOP1, 9, 0, 0xBE800900, 0x0001),
new InstInfo(0495, "s_wqm_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.u = WholeQuadMode(S0.u). SCC = 1 if result is non-zero.<br>Apply whole quad mode to the bitmask specified in SSRC0. Whole quad mode checks each group of four bits in the bitmask; if any bit is set to 1, all four bits are set to 1 in the result. This operation is repeated for the entire bitmask.", @"", ISA_Enc.SOP1, 10, 0, 0xBE800A00, 0x0001),
new InstInfo(0496, "s_xnor_b32", "s4b", "s4b", "s4b", "none", "none", "none", "none", 3, 3, @"D.u = ~(S0.u ^ S1.u). SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 28, 0, 0x8E000000, 0x0081),
new InstInfo(0497, "s_xnor_b64", "s8b", "s8b", "s8b", "none", "none", "none", "none", 3, 3, @"D.u = ~(S0.u ^ S1.u). SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 29, 0, 0x8E800000, 0x0081),
new InstInfo(0498, "s_xnor_saveexec_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.u = EXEC, EXEC = ~(S0.u ^ EXEC). SCC = 1 if the new value of EXEC is non-zero.", @"", ISA_Enc.SOP1, 43, 0, 0xBE802B00, 0x0099),
new InstInfo(0499, "s_xor_b32", "s4b", "s4b", "s4b", "none", "none", "none", "none", 3, 3, @"D.u = S0.u ^ S1.u. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 18, 0, 0x89000000, 0x0081),
new InstInfo(0500, "s_xor_b64", "s8b", "s8b", "s8b", "none", "none", "none", "none", 3, 3, @"D.u = S0.u ^ S1.u. SCC = 1 if result is non-zero.", @"", ISA_Enc.SOP2, 19, 0, 0x89800000, 0x0081),
new InstInfo(0501, "s_xor_saveexec_b64", "s8b", "s8b", "none", "none", "none", "none", "none", 2, 2, @"D.u = EXEC, EXEC = S0.u ^ EXEC. SCC = 1 if the new value of EXEC is non-zero.", @"", ISA_Enc.SOP1, 38, 0, 0xBE802600, 0x0099),
new InstInfo(0502, "tbuffer_load_format_x", "v4b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Typed buffer load 1 Dword with format conversion. ", @"", ISA_Enc.MTBUF, 0, 0, 0xE8000000, 0x0003),
new InstInfo(0503, "tbuffer_load_format_xy", "v8b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Typed buffer load 2 Dwords with format conversion. ", @"", ISA_Enc.MTBUF, 1, 0, 0xE8010000, 0x0003),
new InstInfo(0504, "tbuffer_load_format_xyz", "v12b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Typed buffer load 3 Dwords with format conversion. ", @"", ISA_Enc.MTBUF, 2, 0, 0xE8020000, 0x0003),
new InstInfo(0505, "tbuffer_load_format_xyzw", "v16b", "v4i", "s16b", "none", "none", "16u", "24u", 5, 5, @"Typed buffer load 4 Dwords with format conversion. ", @"", ISA_Enc.MTBUF, 3, 0, 0xE8030000, 0x0003),
new InstInfo(0506, "tbuffer_store_format_x", "none", "v4b", "v4i", "s16b", "none", "16u", "24u", 5, 5, @"Typed buffer store 1 Dword with format conversion. ", @"", ISA_Enc.MTBUF, 4, 0, 0xE8040000, 0x0003),
new InstInfo(0507, "tbuffer_store_format_xy", "none", "v8b", "v4i", "s16b", "none", "16u", "24u", 5, 5, @"Typed buffer store 2 Dwords with format conversion. ", @"", ISA_Enc.MTBUF, 5, 0, 0xE8050000, 0x0003),
new InstInfo(0508, "tbuffer_store_format_xyz", "none", "v12b", "v4i", "s16b", "none", "16u", "24u", 5, 5, @"Typed buffer store 3 Dwords with format conversion. ", @"", ISA_Enc.MTBUF, 6, 0, 0xE8060000, 0x0003),
new InstInfo(0509, "tbuffer_store_format_xyzw", "none", "v16b", "v4i", "s16b", "none", "16u", "24u", 5, 5, @"Typed buffer store 4 Dwords with format conversion. ", @"", ISA_Enc.MTBUF, 96, 0, 0xE8070000, 0x0003),
new InstInfo(0510, "v_add_f32", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"Floating-point add.<br>D.f = S0.f + S1.f. ", @"", ISA_Enc.VOP2, 3, 511, 0x06000000, 0x0005),
new InstInfo(0511, "v_add_f32_ext", "v8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"Floating-point add.<br>D.f = S0.f + S1.f. ", @"", ISA_Enc.VOP3a2, 259, 0, 0xD2060000, 0x0007),
new InstInfo(0512, "v_add_f64", "v8f", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"Double-precision floating-point add.<br>Floating-point 64-bit add. Adds two double-precision numbers in the YX or WZ elements of the source operands, src0 and src1, and outputs a double-precision value to the same elements of the destination operand. No carry or borrow beyond the 64-bit values is performed. The operation occupies two slots in an instruction group. Double result written to 2 consecutive gpr registers, instruction dest specifies lesser of the two.<br>D.d = S0.d + S1.d.<br>These properties hold true for this instruction:<br>(A + B) == (B + A)<br>(A - B) == (A + -B)<br>(A + -A) = +zero", @"", ISA_Enc.VOP3a2, 356, 0, 0xD2C80000, 0x0007),
new InstInfo(0513, "v_add_i32", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"Unsigned integer add based on signed or unsigned integer components. Produces an unsigned carry out in VCC or a scalar register.<br>D.u = S0.u + S1.u; VCC=carry-out (VOP3:sgpr=carry-out). ", @"", ISA_Enc.VOP2, 37, 514, 0x4A000000, 0x0025),
new InstInfo(0514, "v_add_i32_ext", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"Unsigned integer add based on signed or unsigned integer components. Produces an unsigned carry out in VCC or a scalar register.<br>D.u = S0.u + S1.u; VCC=carry-out (VOP3:sgpr=carry-out). ", @"", ISA_Enc.VOP3b2, 293, 0, 0xD24A0000, 0x0027),
new InstInfo(0515, "v_addc_u32", "v4u", "v4u", "v4u", "vcc", "none", "none", "none", 4, 4, @"Integer add based on unsigned integer components, with carry in. Produces a carry out in VCC or a scalar register.<br>Output carry bit of unsigned integer ADD. <br>D.u = S0.u + S1.u + VCC; VCC=carry-out (VOP3:sgpr=carry-out, S2.u=carry-in). ", @"", ISA_Enc.VOP2, 40, 516, 0x50000000, 0x0065),
new InstInfo(0516, "v_addc_u32_ext", "v4u", "v4u", "v4u", "v1u", "none", "none", "none", 4, 4, @"Integer add based on unsigned integer components, with carry in. Produces a carry out in VCC or a scalar register.<br>Output carry bit of unsigned integer ADD. <br>D.u = S0.u + S1.u + VCC; VCC=carry-out (VOP3:sgpr=carry-out, S2.u=carry-in). ", @"", ISA_Enc.VOP3b3, 296, 0, 0xD2500000, 0x0067),
new InstInfo(0517, "v_alignbit_b32", "v4b", "v4b", "v4b", "v1u", "none", "none", "none", 4, 4, @"Bit align. Arbitrarily align 32 bits within 64 into a GPR.<br>D.u = ({S0,S1} >> S2.u[4:0]) & 0xFFFFFFFF. ", @"", ISA_Enc.VOP3a3, 334, 0, 0xD29C0000, 0x0007),
new InstInfo(0518, "v_alignbyte_b32", "v4b", "v4b", "v4b", "v4b", "none", "none", "none", 4, 4, @"Byte align. <br>dst = ({src0, src1} >> (8 * src2[1:0])) & 0xFFFFFFFF;<br>D.u = ({S0,S1} >> (8*S2.u[4:0])) & 0xFFFFFFFF. ", @"", ISA_Enc.VOP3a3, 335, 0, 0xD29E0000, 0x0007),
new InstInfo(0519, "v_and_b32", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Logical bit-wise AND.<br>D.u = S0.u & S1.u. ", @"", ISA_Enc.VOP2, 27, 520, 0x36000000, 0x0005),
new InstInfo(0520, "v_and_b32_ext", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Logical bit-wise AND.<br>D.u = S0.u & S1.u. ", @"", ISA_Enc.VOP3a2, 283, 0, 0xD2360000, 0x0007),
new InstInfo(0521, "v_ashr_i32", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"Scalar Arithmetic Shift Right. The sign bit is shifted into the vacated locations. <br>D.i = S0.i >> S1.i[4:0]. ", @"", ISA_Enc.VOP2, 23, 522, 0x2E000000, 0x0005),
new InstInfo(0522, "v_ashr_i32_ext", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"Scalar Arithmetic Shift Right. The sign bit is shifted into the vacated locations. <br>D.i = S0.i >> S1.i[4:0]. ", @"", ISA_Enc.VOP3a2, 279, 0, 0xD22E0000, 0x0007),
new InstInfo(0523, "v_ashr_i64", "v8i", "v8i", "v1u", "none", "none", "none", "none", 3, 3, @"D = S0.u >> S1.u[4:0]. ", @"", ISA_Enc.VOP3a2, 355, 0, 0xD2C60000, 0x0007),
new InstInfo(0524, "v_ashrrev_i32", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.i = S1.i >> S0.i[4:0]. The sign bit is shifted into the vacated bits. ", @"", ISA_Enc.VOP2, 24, 525, 0x30000000, 0x0005),
new InstInfo(0525, "v_ashrrev_i32_ext", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.i = S1.i >> S0.i[4:0]. The sign bit is shifted into the vacated bits. ", @"", ISA_Enc.VOP3a2, 280, 0, 0xD2300000, 0x0007),
new InstInfo(0526, "v_bcnt_u32_b32", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Bit count.<br>D.u = CountOneBits(S0.u) + S1.u. ", @"", ISA_Enc.VOP2, 34, 527, 0x44000000, 0x0005),
new InstInfo(0527, "v_bcnt_u32_b32_ext", "v4b", "v4b", "v4u", "none", "none", "none", "none", 3, 3, @"Bit count.<br>D.u = CountOneBits(S0.u) + S1.u. ", @"", ISA_Enc.VOP3a2, 290, 0, 0xD2440000, 0x0007),
new InstInfo(0528, "v_bfe_i32", "v4i", "v4i", "v1u", "v1u", "none", "none", "none", 4, 4, @"DX11 signed bitfield extract. src0 = input data, src1 = offset, and src2 = width. The bit position offset is extracted through offset + width from the input data. All bits remaining after dst are stuffed with replications of the sign bit. <br>If (src2[4:0] == 0)<br>   dst = 0;<br>Else if (src2[4:0] + src1[4:0] < 32)<br>   dst = (src0 << (32-src1[4:0] - src2{4:0])) >>> (32 - src2[4:0])<br>Else<br>   dst = src0 >>> src1[4:0]<br>D.i = (S0.i>>S1.u[4:0]) & ((1<<S2.u[4:0])-1); bitfield extract, S0=data, S1=field_offset, S2=field_width. ", @"", ISA_Enc.VOP3a3, 329, 0, 0xD2920000, 0x0007),
new InstInfo(0529, "v_bfe_u32", "v4u", "v4u", "v1u", "v1u", "none", "none", "none", 4, 4, @"DX11 unsigned bitfield extract. Src0 = input data, scr1 = offset, and src2 = width. Bit position offset is extracted through offset + width from input data.<br>If (src2[4:0] == 0)<br>   dst = 0;<br>Else if (src2[4:0] + src1[4:0] < 32) {<br>   dst = (src0 << (32-src1[4:0] - src2{4:0])) >> (32 - src2[4:0])<br>Else<br>   dst = src0 >> src1[4:0]<br>D.u = (S0.u>>S1.u[4:0]) & ((1<<S2.u[4:0])-1); bitfield extract, S0=data, S1=field_offset, S2=field_width. ", @"", ISA_Enc.VOP3a3, 328, 0, 0xD2900000, 0x0007),
new InstInfo(0530, "v_bfi_b32", "v4b", "v4b", "v1u", "v1u", "none", "none", "none", 4, 4, @"Bitfield insert used after BFM to implement DX11 bitfield insert. <br>src0 = bitfield mask (from BFM)<br>src 1 & src2 = input data<br>This replaces bits in src2 with bits in src1 according to the bitfield mask.<br>D.u = (S0.u & S1.u) | (~S0.u & S2.u). ", @"", ISA_Enc.VOP3a3, 330, 0, 0xD2940000, 0x0007),
new InstInfo(0531, "v_bfm_b32", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Bitfield mask. Used before BFI to implement DX11 bitfield insert.<br>D.u  = ((1<<S0.u[4:0])-1) << S1.u[4:0]; S0=bitfield_width, S1=bitfield_offset. ", @"", ISA_Enc.VOP2, 30, 532, 0x3C000000, 0x0005),
new InstInfo(0532, "v_bfm_b32_ext", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Bitfield mask. Used before BFI to implement DX11 bitfield insert.<br>D.u  = ((1<<S0.u[4:0])-1) << S1.u[4:0]; S0=bitfield_width, S1=bitfield_offset. ", @"", ISA_Enc.VOP3a2, 286, 0, 0xD23C0000, 0x0007),
new InstInfo(0533, "v_bfrev_b32", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"Bitfield reverse.<br>D.u[31:0] = S0.u[0:31]. ", @"", ISA_Enc.VOP1, 56, 534, 0x7E007000, 0x0005),
new InstInfo(0534, "v_bfrev_b32_ext", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"Bitfield reverse.<br>D.u[31:0] = S0.u[0:31]. ", @"", ISA_Enc.VOP3a1, 440, 0, 0xD3700000, 0x0007),
new InstInfo(0535, "v_ceil_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Floating point ceiling function.<br>D.f = ceil(S0.f).  Implemented as: D.f = trunc(S0.f);<br>if (S0 > 0.0 && S0 != D), D += 1.0. ", @"", ISA_Enc.VOP1, 34, 536, 0x7E004400, 0x0005),
new InstInfo(0536, "v_ceil_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Floating point ceiling function.<br>D.f = ceil(S0.f).  Implemented as: D.f = trunc(S0.f);<br>if (S0 > 0.0 && S0 != D), D += 1.0. ", @"", ISA_Enc.VOP3a1, 418, 0, 0xD3440000, 0x0007),
new InstInfo(0537, "v_ceil_f64", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"64-bit floating-point ceiling.<br>D.d = trunc(S0.d); if (S0.d > 0.0 && S0.d != D.d), D.d += 1.0. ", @"", ISA_Enc.VOP1, 24, 0, 0x7E003000, 0x0005),
new InstInfo(0538, "v_clrexcp", "none", "none", "none", "none", "none", "none", "none", 0, 0, @"Clear wave's exception state in SIMD. ", @"", ISA_Enc.VOP1, 65, 539, 0x7E008200, 0x0005),
new InstInfo(0539, "v_clrexcp_ext", "none", "none", "none", "none", "none", "none", "none", 0, 0, @"Clear wave's exception state in SIMD. ", @"", ISA_Enc.VOP3a0, 449, 0, 0xD3820000, 0x0007),
new InstInfo(0540, "v_cmp_class_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D = IEEE numeric class function specified in S1.u, performed on S0.f.", @"", ISA_Enc.VOPC, 136, 541, 0x7D100000, 0x0025),
new InstInfo(0541, "v_cmp_class_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D = IEEE numeric class function specified in S1.u, performed on S0.f.", @"", ISA_Enc.VOP3bC, 136, 0, 0x7D100000, 0x0025),
new InstInfo(0542, "v_cmp_class_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D = IEEE numeric class function specified in S1.u, performed on S0.d.", @"", ISA_Enc.VOPC, 168, 543, 0x7D500000, 0x0025),
new InstInfo(0543, "v_cmp_class_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D = IEEE numeric class function specified in S1.u, performed on S0.d.", @"", ISA_Enc.VOP3bC, 168, 0, 0x7D500000, 0x0025),
new InstInfo(0544, "v_cmp_eq_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 2, 545, 0x7C040000, 0x0025),
new InstInfo(0545, "v_cmp_eq_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 2, 0, 0x7C040000, 0x0025),
new InstInfo(0546, "v_cmp_eq_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 34, 547, 0x7C440000, 0x0025),
new InstInfo(0547, "v_cmp_eq_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 34, 0, 0x7C440000, 0x0025),
new InstInfo(0548, "v_cmp_eq_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); On 32-bit integers.", @"", ISA_Enc.VOPC, 130, 549, 0x7D040000, 0x0025),
new InstInfo(0549, "v_cmp_eq_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); On 32-bit integers.", @"", ISA_Enc.VOP3bC, 130, 0, 0x7D040000, 0x0025),
new InstInfo(0550, "v_cmp_eq_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); On 64-bit integers.", @"", ISA_Enc.VOPC, 162, 551, 0x7D440000, 0x0025),
new InstInfo(0551, "v_cmp_eq_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); On 64-bit integers.", @"", ISA_Enc.VOP3bC, 162, 0, 0x7D440000, 0x0025),
new InstInfo(0552, "v_cmp_eq_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); On unsigned 32-bit integers.", @"", ISA_Enc.VOPC, 194, 553, 0x7D840000, 0x0025),
new InstInfo(0553, "v_cmp_eq_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); On unsigned 32-bit integers.", @"", ISA_Enc.VOP3bC, 194, 0, 0x7D840000, 0x0025),
new InstInfo(0554, "v_cmp_eq_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); On unsigned 64-bit integers.", @"", ISA_Enc.VOPC, 226, 555, 0x7DC40000, 0x0025),
new InstInfo(0555, "v_cmp_eq_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); On unsigned 64-bit integers.", @"", ISA_Enc.VOP3bC, 226, 0, 0x7DC40000, 0x0025),
new InstInfo(0556, "v_cmp_f_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 0, 557, 0x7C000000, 0x0025),
new InstInfo(0557, "v_cmp_f_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 0, 0, 0x7C000000, 0x0025),
new InstInfo(0558, "v_cmp_f_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 32, 559, 0x7C400000, 0x0025),
new InstInfo(0559, "v_cmp_f_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 32, 0, 0x7C400000, 0x0025),
new InstInfo(0560, "v_cmp_f_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = 0; On 32-bit integers.", @"", ISA_Enc.VOPC, 128, 561, 0x7D000000, 0x0025),
new InstInfo(0561, "v_cmp_f_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = 0; On 32-bit integers.", @"", ISA_Enc.VOP3bC, 128, 0, 0x7D000000, 0x0025),
new InstInfo(0562, "v_cmp_f_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = 0; On 64-bit integers.", @"", ISA_Enc.VOPC, 160, 563, 0x7D400000, 0x0025),
new InstInfo(0563, "v_cmp_f_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = 0; On 64-bit integers.", @"", ISA_Enc.VOP3bC, 160, 0, 0x7D400000, 0x0025),
new InstInfo(0564, "v_cmp_f_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = 0; On unsigned 32-bit integers.", @"", ISA_Enc.VOPC, 192, 565, 0x7D800000, 0x0025),
new InstInfo(0565, "v_cmp_f_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = 0; On unsigned 32-bit integers.", @"", ISA_Enc.VOP3bC, 192, 0, 0x7D800000, 0x0025),
new InstInfo(0566, "v_cmp_f_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = 0; On unsigned 64-bit integers.", @"", ISA_Enc.VOPC, 224, 567, 0x7DC00000, 0x0025),
new InstInfo(0567, "v_cmp_f_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = 0; On unsigned 64-bit integers.", @"", ISA_Enc.VOP3bC, 224, 0, 0x7DC00000, 0x0025),
new InstInfo(0568, "v_cmp_ge_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 6, 569, 0x7C0C0000, 0x0025),
new InstInfo(0569, "v_cmp_ge_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 6, 0, 0x7C0C0000, 0x0025),
new InstInfo(0570, "v_cmp_ge_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 38, 571, 0x7C4C0000, 0x0025),
new InstInfo(0571, "v_cmp_ge_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 38, 0, 0x7C4C0000, 0x0025),
new InstInfo(0572, "v_cmp_ge_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); On 32-bit integers.", @"", ISA_Enc.VOPC, 134, 573, 0x7D0C0000, 0x0025),
new InstInfo(0573, "v_cmp_ge_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); On 32-bit integers.", @"", ISA_Enc.VOP3bC, 134, 0, 0x7D0C0000, 0x0025),
new InstInfo(0574, "v_cmp_ge_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); On 64-bit integers.", @"", ISA_Enc.VOPC, 166, 575, 0x7D4C0000, 0x0025),
new InstInfo(0575, "v_cmp_ge_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); On 64-bit integers.", @"", ISA_Enc.VOP3bC, 166, 0, 0x7D4C0000, 0x0025),
new InstInfo(0576, "v_cmp_ge_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); On unsigned 32-bit integers.", @"", ISA_Enc.VOPC, 198, 577, 0x7D8C0000, 0x0025),
new InstInfo(0577, "v_cmp_ge_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); On unsigned 32-bit integers.", @"", ISA_Enc.VOP3bC, 198, 0, 0x7D8C0000, 0x0025),
new InstInfo(0578, "v_cmp_ge_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); On unsigned 64-bit integers.", @"", ISA_Enc.VOPC, 230, 579, 0x7DCC0000, 0x0025),
new InstInfo(0579, "v_cmp_ge_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); On unsigned 64-bit integers.", @"", ISA_Enc.VOP3bC, 230, 0, 0x7DCC0000, 0x0025),
new InstInfo(0580, "v_cmp_gt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 4, 581, 0x7C080000, 0x0025),
new InstInfo(0581, "v_cmp_gt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 4, 0, 0x7C080000, 0x0025),
new InstInfo(0582, "v_cmp_gt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 36, 583, 0x7C480000, 0x0025),
new InstInfo(0583, "v_cmp_gt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 36, 0, 0x7C480000, 0x0025),
new InstInfo(0584, "v_cmp_gt_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); On 32-bit integers.", @"", ISA_Enc.VOPC, 132, 585, 0x7D080000, 0x0025),
new InstInfo(0585, "v_cmp_gt_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); On 32-bit integers.", @"", ISA_Enc.VOP3bC, 132, 0, 0x7D080000, 0x0025),
new InstInfo(0586, "v_cmp_gt_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); On 64-bit integers.", @"", ISA_Enc.VOPC, 164, 587, 0x7D480000, 0x0025),
new InstInfo(0587, "v_cmp_gt_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); On 64-bit integers.", @"", ISA_Enc.VOP3bC, 164, 0, 0x7D480000, 0x0025),
new InstInfo(0588, "v_cmp_gt_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); On unsigned 32-bit integers.", @"", ISA_Enc.VOPC, 196, 589, 0x7D880000, 0x0025),
new InstInfo(0589, "v_cmp_gt_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); On unsigned 32-bit integers.", @"", ISA_Enc.VOP3bC, 196, 0, 0x7D880000, 0x0025),
new InstInfo(0590, "v_cmp_gt_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); On unsigned 64-bit integers.", @"", ISA_Enc.VOPC, 228, 591, 0x7DC80000, 0x0025),
new InstInfo(0591, "v_cmp_gt_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); On unsigned 64-bit integers.", @"", ISA_Enc.VOP3bC, 228, 0, 0x7DC80000, 0x0025),
new InstInfo(0592, "v_cmp_le_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 3, 593, 0x7C060000, 0x0025),
new InstInfo(0593, "v_cmp_le_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 3, 0, 0x7C060000, 0x0025),
new InstInfo(0594, "v_cmp_le_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 35, 595, 0x7C460000, 0x0025),
new InstInfo(0595, "v_cmp_le_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 35, 0, 0x7C460000, 0x0025),
new InstInfo(0596, "v_cmp_le_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); On 32-bit integers.", @"", ISA_Enc.VOPC, 131, 597, 0x7D060000, 0x0025),
new InstInfo(0597, "v_cmp_le_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); On 32-bit integers.", @"", ISA_Enc.VOP3bC, 131, 0, 0x7D060000, 0x0025),
new InstInfo(0598, "v_cmp_le_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); On 64-bit integers.", @"", ISA_Enc.VOPC, 163, 599, 0x7D460000, 0x0025),
new InstInfo(0599, "v_cmp_le_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); On 64-bit integers.", @"", ISA_Enc.VOP3bC, 163, 0, 0x7D460000, 0x0025),
new InstInfo(0600, "v_cmp_le_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); On unsigned 32-bit integers.", @"", ISA_Enc.VOPC, 195, 601, 0x7D860000, 0x0025),
new InstInfo(0601, "v_cmp_le_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); On unsigned 32-bit integers.", @"", ISA_Enc.VOP3bC, 195, 0, 0x7D860000, 0x0025),
new InstInfo(0602, "v_cmp_le_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); On unsigned 64-bit integers.", @"", ISA_Enc.VOPC, 227, 603, 0x7DC60000, 0x0025),
new InstInfo(0603, "v_cmp_le_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); On unsigned 64-bit integers.", @"", ISA_Enc.VOP3bC, 227, 0, 0x7DC60000, 0x0025),
new InstInfo(0604, "v_cmp_lg_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 5, 605, 0x7C0A0000, 0x0025),
new InstInfo(0605, "v_cmp_lg_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 5, 0, 0x7C0A0000, 0x0025),
new InstInfo(0606, "v_cmp_lg_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 37, 607, 0x7C4A0000, 0x0025),
new InstInfo(0607, "v_cmp_lg_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 37, 0, 0x7C4A0000, 0x0025),
new InstInfo(0608, "v_cmp_lg_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); On 32-bit integers.", @"", ISA_Enc.VOPC, 133, 609, 0x7D0A0000, 0x0025),
new InstInfo(0609, "v_cmp_lg_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); On 32-bit integers.", @"", ISA_Enc.VOP3bC, 133, 0, 0x7D0A0000, 0x0025),
new InstInfo(0610, "v_cmp_lg_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); On 64-bit integers.", @"", ISA_Enc.VOPC, 165, 611, 0x7D4A0000, 0x0025),
new InstInfo(0611, "v_cmp_lg_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); On 64-bit integers.", @"", ISA_Enc.VOP3bC, 165, 0, 0x7D4A0000, 0x0025),
new InstInfo(0612, "v_cmp_lg_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); On unsigned 32-bit integers.", @"", ISA_Enc.VOPC, 197, 613, 0x7D8A0000, 0x0025),
new InstInfo(0613, "v_cmp_lg_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); On unsigned 32-bit integers.", @"", ISA_Enc.VOP3bC, 197, 0, 0x7D8A0000, 0x0025),
new InstInfo(0614, "v_cmp_lg_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); On unsigned 64-bit integers.", @"", ISA_Enc.VOPC, 229, 615, 0x7DCA0000, 0x0025),
new InstInfo(0615, "v_cmp_lg_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); On unsigned 64-bit integers.", @"", ISA_Enc.VOP3bC, 229, 0, 0x7DCA0000, 0x0025),
new InstInfo(0616, "v_cmp_lt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 1, 617, 0x7C020000, 0x0025),
new InstInfo(0617, "v_cmp_lt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 1, 0, 0x7C020000, 0x0025),
new InstInfo(0618, "v_cmp_lt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 33, 619, 0x7C420000, 0x0025),
new InstInfo(0619, "v_cmp_lt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 33, 0, 0x7C420000, 0x0025),
new InstInfo(0620, "v_cmp_lt_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); On 32-bit integers.", @"", ISA_Enc.VOPC, 129, 621, 0x7D020000, 0x0025),
new InstInfo(0621, "v_cmp_lt_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); On 32-bit integers.", @"", ISA_Enc.VOP3bC, 129, 0, 0x7D020000, 0x0025),
new InstInfo(0622, "v_cmp_lt_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); On 64-bit integers.", @"", ISA_Enc.VOPC, 161, 623, 0x7D420000, 0x0025),
new InstInfo(0623, "v_cmp_lt_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); On 64-bit integers.", @"", ISA_Enc.VOP3bC, 161, 0, 0x7D420000, 0x0025),
new InstInfo(0624, "v_cmp_lt_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); On unsigned 32-bit integers.", @"", ISA_Enc.VOPC, 193, 625, 0x7D820000, 0x0025),
new InstInfo(0625, "v_cmp_lt_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); On unsigned 32-bit integers.", @"", ISA_Enc.VOP3bC, 193, 0, 0x7D820000, 0x0025),
new InstInfo(0626, "v_cmp_lt_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); On unsigned 64-bit integers.", @"", ISA_Enc.VOPC, 225, 627, 0x7DC20000, 0x0025),
new InstInfo(0627, "v_cmp_lt_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); On unsigned 64-bit integers.", @"", ISA_Enc.VOP3bC, 225, 0, 0x7DC20000, 0x0025),
new InstInfo(0628, "v_cmp_neq_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 13, 629, 0x7C1A0000, 0x0025),
new InstInfo(0629, "v_cmp_neq_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 13, 0, 0x7C1A0000, 0x0025),
new InstInfo(0630, "v_cmp_neq_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 45, 631, 0x7C5A0000, 0x0025),
new InstInfo(0631, "v_cmp_neq_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 45, 0, 0x7C5A0000, 0x0025),
new InstInfo(0632, "v_cmp_nge_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 9, 633, 0x7C120000, 0x0025),
new InstInfo(0633, "v_cmp_nge_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 9, 0, 0x7C120000, 0x0025),
new InstInfo(0634, "v_cmp_nge_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 41, 635, 0x7C520000, 0x0025),
new InstInfo(0635, "v_cmp_nge_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 41, 0, 0x7C520000, 0x0025),
new InstInfo(0636, "v_cmp_ngt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 11, 637, 0x7C160000, 0x0025),
new InstInfo(0637, "v_cmp_ngt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 11, 0, 0x7C160000, 0x0025),
new InstInfo(0638, "v_cmp_ngt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 43, 639, 0x7C560000, 0x0025),
new InstInfo(0639, "v_cmp_ngt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 43, 0, 0x7C560000, 0x0025),
new InstInfo(0640, "v_cmp_nle_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 12, 641, 0x7C180000, 0x0025),
new InstInfo(0641, "v_cmp_nle_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 12, 0, 0x7C180000, 0x0025),
new InstInfo(0642, "v_cmp_nle_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 44, 643, 0x7C580000, 0x0025),
new InstInfo(0643, "v_cmp_nle_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 44, 0, 0x7C580000, 0x0025),
new InstInfo(0644, "v_cmp_nlg_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 10, 645, 0x7C140000, 0x0025),
new InstInfo(0645, "v_cmp_nlg_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 10, 0, 0x7C140000, 0x0025),
new InstInfo(0646, "v_cmp_nlg_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 42, 647, 0x7C540000, 0x0025),
new InstInfo(0647, "v_cmp_nlg_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 42, 0, 0x7C540000, 0x0025),
new InstInfo(0648, "v_cmp_nlt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 14, 649, 0x7C1C0000, 0x0025),
new InstInfo(0649, "v_cmp_nlt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 14, 0, 0x7C1C0000, 0x0025),
new InstInfo(0650, "v_cmp_nlt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 46, 651, 0x7C5C0000, 0x0025),
new InstInfo(0651, "v_cmp_nlt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 46, 0, 0x7C5C0000, 0x0025),
new InstInfo(0652, "v_cmp_o_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 7, 653, 0x7C0E0000, 0x0025),
new InstInfo(0653, "v_cmp_o_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 7, 0, 0x7C0E0000, 0x0025),
new InstInfo(0654, "v_cmp_o_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 39, 655, 0x7C4E0000, 0x0025),
new InstInfo(0655, "v_cmp_o_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 39, 0, 0x7C4E0000, 0x0025),
new InstInfo(0656, "v_cmp_tru_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 15, 657, 0x7C1E0000, 0x0025),
new InstInfo(0657, "v_cmp_tru_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 15, 0, 0x7C1E0000, 0x0025),
new InstInfo(0658, "v_cmp_tru_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 47, 659, 0x7C5E0000, 0x0025),
new InstInfo(0659, "v_cmp_tru_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 47, 0, 0x7C5E0000, 0x0025),
new InstInfo(0660, "v_cmp_tru_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = 1; On 32-bit integers.", @"", ISA_Enc.VOPC, 135, 661, 0x7D0E0000, 0x0025),
new InstInfo(0661, "v_cmp_tru_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = 1; On 32-bit integers.", @"", ISA_Enc.VOP3bC, 135, 0, 0x7D0E0000, 0x0025),
new InstInfo(0662, "v_cmp_tru_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = 1; On 64-bit integers.", @"", ISA_Enc.VOPC, 167, 663, 0x7D4E0000, 0x0025),
new InstInfo(0663, "v_cmp_tru_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = 1; On 64-bit integers.", @"", ISA_Enc.VOP3bC, 167, 0, 0x7D4E0000, 0x0025),
new InstInfo(0664, "v_cmp_tru_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = 1; On unsigned 32-bit integers.", @"", ISA_Enc.VOPC, 199, 665, 0x7D8E0000, 0x0025),
new InstInfo(0665, "v_cmp_tru_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = 1; On unsigned 32-bit integers.", @"", ISA_Enc.VOP3bC, 199, 0, 0x7D8E0000, 0x0025),
new InstInfo(0666, "v_cmp_tru_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = 1; On unsigned 64-bit integers.", @"", ISA_Enc.VOPC, 231, 667, 0x7DCE0000, 0x0025),
new InstInfo(0667, "v_cmp_tru_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = 1; On unsigned 64-bit integers.", @"", ISA_Enc.VOP3bC, 231, 0, 0x7DCE0000, 0x0025),
new InstInfo(0668, "v_cmp_u_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 8, 669, 0x7C100000, 0x0025),
new InstInfo(0669, "v_cmp_u_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 8, 0, 0x7C100000, 0x0025),
new InstInfo(0670, "v_cmp_u_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on sNaN input only. ", @"", ISA_Enc.VOPC, 40, 671, 0x7C500000, 0x0025),
new InstInfo(0671, "v_cmp_u_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on sNaN input only. ", @"", ISA_Enc.VOP3bC, 40, 0, 0x7C500000, 0x0025),
new InstInfo(0672, "v_cmps_eq_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 66, 673, 0x7C840000, 0x0025),
new InstInfo(0673, "v_cmps_eq_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 66, 0, 0x7C840000, 0x0025),
new InstInfo(0674, "v_cmps_eq_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 98, 675, 0x7CC40000, 0x0025),
new InstInfo(0675, "v_cmps_eq_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 98, 0, 0x7CC40000, 0x0025),
new InstInfo(0676, "v_cmps_f_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on any NaN.", @"", ISA_Enc.VOPC, 64, 677, 0x7C800000, 0x0025),
new InstInfo(0677, "v_cmps_f_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on any NaN.", @"", ISA_Enc.VOP3bC, 64, 0, 0x7C800000, 0x0025),
new InstInfo(0678, "v_cmps_f_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on any NaN.", @"", ISA_Enc.VOPC, 96, 679, 0x7CC00000, 0x0025),
new InstInfo(0679, "v_cmps_f_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on any NaN.", @"", ISA_Enc.VOP3bC, 96, 0, 0x7CC00000, 0x0025),
new InstInfo(0680, "v_cmps_ge_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 70, 681, 0x7C8C0000, 0x0025),
new InstInfo(0681, "v_cmps_ge_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 70, 0, 0x7C8C0000, 0x0025),
new InstInfo(0682, "v_cmps_ge_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 102, 683, 0x7CCC0000, 0x0025),
new InstInfo(0683, "v_cmps_ge_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 102, 0, 0x7CCC0000, 0x0025),
new InstInfo(0684, "v_cmps_gt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 68, 685, 0x7C880000, 0x0025),
new InstInfo(0685, "v_cmps_gt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 68, 0, 0x7C880000, 0x0025),
new InstInfo(0686, "v_cmps_gt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 100, 687, 0x7CC80000, 0x0025),
new InstInfo(0687, "v_cmps_gt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 100, 0, 0x7CC80000, 0x0025),
new InstInfo(0688, "v_cmps_le_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 67, 689, 0x7C860000, 0x0025),
new InstInfo(0689, "v_cmps_le_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 67, 0, 0x7C860000, 0x0025),
new InstInfo(0690, "v_cmps_le_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 99, 691, 0x7CC60000, 0x0025),
new InstInfo(0691, "v_cmps_le_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 99, 0, 0x7CC60000, 0x0025),
new InstInfo(0692, "v_cmps_lg_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 69, 693, 0x7C8A0000, 0x0025),
new InstInfo(0693, "v_cmps_lg_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 69, 0, 0x7C8A0000, 0x0025),
new InstInfo(0694, "v_cmps_lg_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 101, 695, 0x7CCA0000, 0x0025),
new InstInfo(0695, "v_cmps_lg_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 101, 0, 0x7CCA0000, 0x0025),
new InstInfo(0696, "v_cmps_lt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 65, 697, 0x7C820000, 0x0025),
new InstInfo(0697, "v_cmps_lt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 65, 0, 0x7C820000, 0x0025),
new InstInfo(0698, "v_cmps_lt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 97, 699, 0x7CC20000, 0x0025),
new InstInfo(0699, "v_cmps_lt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 97, 0, 0x7CC20000, 0x0025),
new InstInfo(0700, "v_cmps_neq_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 77, 701, 0x7C9A0000, 0x0025),
new InstInfo(0701, "v_cmps_neq_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 77, 0, 0x7C9A0000, 0x0025),
new InstInfo(0702, "v_cmps_neq_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 109, 703, 0x7CDA0000, 0x0025),
new InstInfo(0703, "v_cmps_neq_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 109, 0, 0x7CDA0000, 0x0025),
new InstInfo(0704, "v_cmps_nge_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 73, 705, 0x7C920000, 0x0025),
new InstInfo(0705, "v_cmps_nge_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 73, 0, 0x7C920000, 0x0025),
new InstInfo(0706, "v_cmps_nge_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 105, 707, 0x7CD20000, 0x0025),
new InstInfo(0707, "v_cmps_nge_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 105, 0, 0x7CD20000, 0x0025),
new InstInfo(0708, "v_cmps_ngt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 75, 709, 0x7C960000, 0x0025),
new InstInfo(0709, "v_cmps_ngt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 75, 0, 0x7C960000, 0x0025),
new InstInfo(0710, "v_cmps_ngt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 107, 711, 0x7CD60000, 0x0025),
new InstInfo(0711, "v_cmps_ngt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 107, 0, 0x7CD60000, 0x0025),
new InstInfo(0712, "v_cmps_nle_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 76, 713, 0x7C980000, 0x0025),
new InstInfo(0713, "v_cmps_nle_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 76, 0, 0x7C980000, 0x0025),
new InstInfo(0714, "v_cmps_nle_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 108, 715, 0x7CD80000, 0x0025),
new InstInfo(0715, "v_cmps_nle_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 108, 0, 0x7CD80000, 0x0025),
new InstInfo(0716, "v_cmps_nlg_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 74, 717, 0x7C940000, 0x0025),
new InstInfo(0717, "v_cmps_nlg_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 74, 0, 0x7C940000, 0x0025),
new InstInfo(0718, "v_cmps_nlg_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 106, 719, 0x7CD40000, 0x0025),
new InstInfo(0719, "v_cmps_nlg_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 106, 0, 0x7CD40000, 0x0025),
new InstInfo(0720, "v_cmps_nlt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 78, 721, 0x7C9C0000, 0x0025),
new InstInfo(0721, "v_cmps_nlt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 78, 0, 0x7C9C0000, 0x0025),
new InstInfo(0722, "v_cmps_nlt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on any NaN.", @"", ISA_Enc.VOPC, 110, 723, 0x7CDC0000, 0x0025),
new InstInfo(0723, "v_cmps_nlt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 110, 0, 0x7CDC0000, 0x0025),
new InstInfo(0724, "v_cmps_o_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on any NaN.", @"", ISA_Enc.VOPC, 71, 725, 0x7C8E0000, 0x0025),
new InstInfo(0725, "v_cmps_o_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 71, 0, 0x7C8E0000, 0x0025),
new InstInfo(0726, "v_cmps_o_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on any NaN.", @"", ISA_Enc.VOPC, 103, 727, 0x7CCE0000, 0x0025),
new InstInfo(0727, "v_cmps_o_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 103, 0, 0x7CCE0000, 0x0025),
new InstInfo(0728, "v_cmps_tru_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on any NaN.", @"", ISA_Enc.VOPC, 79, 729, 0x7C9E0000, 0x0025),
new InstInfo(0729, "v_cmps_tru_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on any NaN.", @"", ISA_Enc.VOP3bC, 79, 0, 0x7C9E0000, 0x0025),
new InstInfo(0730, "v_cmps_tru_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on any NaN.", @"", ISA_Enc.VOPC, 111, 731, 0x7CDE0000, 0x0025),
new InstInfo(0731, "v_cmps_tru_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on any NaN.", @"", ISA_Enc.VOP3bC, 111, 0, 0x7CDE0000, 0x0025),
new InstInfo(0732, "v_cmps_u_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on any NaN.", @"", ISA_Enc.VOPC, 72, 733, 0x7C900000, 0x0025),
new InstInfo(0733, "v_cmps_u_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 72, 0, 0x7C900000, 0x0025),
new InstInfo(0734, "v_cmps_u_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on any NaN.", @"", ISA_Enc.VOPC, 104, 735, 0x7CD00000, 0x0025),
new InstInfo(0735, "v_cmps_u_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on any NaN.", @"", ISA_Enc.VOP3bC, 104, 0, 0x7CD00000, 0x0025),
new InstInfo(0736, "v_cmpsx_eq_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 82, 737, 0x7CA40000, 0x002D),
new InstInfo(0737, "v_cmpsx_eq_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 82, 0, 0x7CA40000, 0x002D),
new InstInfo(0738, "v_cmpsx_eq_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 114, 739, 0x7CE40000, 0x002D),
new InstInfo(0739, "v_cmpsx_eq_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 114, 0, 0x7CE40000, 0x002D),
new InstInfo(0740, "v_cmpsx_f_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 80, 741, 0x7CA00000, 0x002D),
new InstInfo(0741, "v_cmpsx_f_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 80, 0, 0x7CA00000, 0x002D),
new InstInfo(0742, "v_cmpsx_f_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 112, 743, 0x7CE00000, 0x002D),
new InstInfo(0743, "v_cmpsx_f_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 112, 0, 0x7CE00000, 0x002D),
new InstInfo(0744, "v_cmpsx_ge_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 86, 745, 0x7CAC0000, 0x002D),
new InstInfo(0745, "v_cmpsx_ge_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 86, 0, 0x7CAC0000, 0x002D),
new InstInfo(0746, "v_cmpsx_ge_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 118, 747, 0x7CEC0000, 0x002D),
new InstInfo(0747, "v_cmpsx_ge_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 118, 0, 0x7CEC0000, 0x002D),
new InstInfo(0748, "v_cmpsx_gt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 84, 749, 0x7CA80000, 0x002D),
new InstInfo(0749, "v_cmpsx_gt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 84, 0, 0x7CA80000, 0x002D),
new InstInfo(0750, "v_cmpsx_gt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 116, 751, 0x7CE80000, 0x002D),
new InstInfo(0751, "v_cmpsx_gt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 116, 0, 0x7CE80000, 0x002D),
new InstInfo(0752, "v_cmpsx_le_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 83, 753, 0x7CA60000, 0x002D),
new InstInfo(0753, "v_cmpsx_le_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 83, 0, 0x7CA60000, 0x002D),
new InstInfo(0754, "v_cmpsx_le_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 115, 755, 0x7CE60000, 0x002D),
new InstInfo(0755, "v_cmpsx_le_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 115, 0, 0x7CE60000, 0x002D),
new InstInfo(0756, "v_cmpsx_lg_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 85, 757, 0x7CAA0000, 0x002D),
new InstInfo(0757, "v_cmpsx_lg_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 85, 0, 0x7CAA0000, 0x002D),
new InstInfo(0758, "v_cmpsx_lg_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 117, 759, 0x7CEA0000, 0x002D),
new InstInfo(0759, "v_cmpsx_lg_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 117, 0, 0x7CEA0000, 0x002D),
new InstInfo(0760, "v_cmpsx_lt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 81, 761, 0x7CA20000, 0x002D),
new InstInfo(0761, "v_cmpsx_lt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 81, 0, 0x7CA20000, 0x002D),
new InstInfo(0762, "v_cmpsx_lt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 113, 763, 0x7CE20000, 0x002D),
new InstInfo(0763, "v_cmpsx_lt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 113, 0, 0x7CE20000, 0x002D),
new InstInfo(0764, "v_cmpsx_neq_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 93, 765, 0x7CBA0000, 0x002D),
new InstInfo(0765, "v_cmpsx_neq_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 93, 0, 0x7CBA0000, 0x002D),
new InstInfo(0766, "v_cmpsx_neq_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 125, 767, 0x7CFA0000, 0x002D),
new InstInfo(0767, "v_cmpsx_neq_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 125, 0, 0x7CFA0000, 0x002D),
new InstInfo(0768, "v_cmpsx_nge_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 89, 769, 0x7CB20000, 0x002D),
new InstInfo(0769, "v_cmpsx_nge_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 89, 0, 0x7CB20000, 0x002D),
new InstInfo(0770, "v_cmpsx_nge_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 121, 771, 0x7CF20000, 0x002D),
new InstInfo(0771, "v_cmpsx_nge_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 121, 0, 0x7CF20000, 0x002D),
new InstInfo(0772, "v_cmpsx_ngt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 91, 773, 0x7CB60000, 0x002D),
new InstInfo(0773, "v_cmpsx_ngt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 91, 0, 0x7CB60000, 0x002D),
new InstInfo(0774, "v_cmpsx_ngt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 123, 775, 0x7CF60000, 0x002D),
new InstInfo(0775, "v_cmpsx_ngt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 123, 0, 0x7CF60000, 0x002D),
new InstInfo(0776, "v_cmpsx_nle_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 92, 777, 0x7CB80000, 0x002D),
new InstInfo(0777, "v_cmpsx_nle_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 92, 0, 0x7CB80000, 0x002D),
new InstInfo(0778, "v_cmpsx_nle_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 124, 779, 0x7CF80000, 0x002D),
new InstInfo(0779, "v_cmpsx_nle_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 124, 0, 0x7CF80000, 0x002D),
new InstInfo(0780, "v_cmpsx_nlg_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 90, 781, 0x7CB40000, 0x002D),
new InstInfo(0781, "v_cmpsx_nlg_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 90, 0, 0x7CB40000, 0x002D),
new InstInfo(0782, "v_cmpsx_nlg_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 122, 783, 0x7CF40000, 0x002D),
new InstInfo(0783, "v_cmpsx_nlg_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 122, 0, 0x7CF40000, 0x002D),
new InstInfo(0784, "v_cmpsx_nlt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 94, 785, 0x7CBC0000, 0x002D),
new InstInfo(0785, "v_cmpsx_nlt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 94, 0, 0x7CBC0000, 0x002D),
new InstInfo(0786, "v_cmpsx_nlt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 126, 787, 0x7CFC0000, 0x002D),
new InstInfo(0787, "v_cmpsx_nlt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 126, 0, 0x7CFC0000, 0x002D),
new InstInfo(0788, "v_cmpsx_o_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 87, 789, 0x7CAE0000, 0x002D),
new InstInfo(0789, "v_cmpsx_o_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 87, 0, 0x7CAE0000, 0x002D),
new InstInfo(0790, "v_cmpsx_o_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 119, 791, 0x7CEE0000, 0x002D),
new InstInfo(0791, "v_cmpsx_o_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 119, 0, 0x7CEE0000, 0x002D),
new InstInfo(0792, "v_cmpsx_tru_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 95, 793, 0x7CBE0000, 0x002D),
new InstInfo(0793, "v_cmpsx_tru_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 95, 0, 0x7CBE0000, 0x002D),
new InstInfo(0794, "v_cmpsx_tru_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 127, 795, 0x7CFE0000, 0x002D),
new InstInfo(0795, "v_cmpsx_tru_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 127, 0, 0x7CFE0000, 0x002D),
new InstInfo(0796, "v_cmpsx_u_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 88, 797, 0x7CB00000, 0x002D),
new InstInfo(0797, "v_cmpsx_u_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 88, 0, 0x7CB00000, 0x002D),
new InstInfo(0798, "v_cmpsx_u_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOPC, 120, 799, 0x7CF00000, 0x002D),
new InstInfo(0799, "v_cmpsx_u_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on any NaN. Also write EXEC.", @"", ISA_Enc.VOP3bC, 120, 0, 0x7CF00000, 0x002D),
new InstInfo(0800, "v_cmpx_class_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D = IEEE numericclass function specified in S1.u, performed on S0.f. Also write EXEC.", @"", ISA_Enc.VOPC, 152, 801, 0x7D300000, 0x002D),
new InstInfo(0801, "v_cmpx_class_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D = IEEE numericclass function specified in S1.u, performed on S0.f. Also write EXEC.", @"", ISA_Enc.VOP3bC, 152, 0, 0x7D300000, 0x002D),
new InstInfo(0802, "v_cmpx_class_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D = IEEE numericclass function specified in S1.u, performed on S0.d. Also write EXEC.", @"", ISA_Enc.VOPC, 184, 803, 0x7D700000, 0x002D),
new InstInfo(0803, "v_cmpx_class_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D = IEEE numericclass function specified in S1.u, performed on S0.d. Also write EXEC.", @"", ISA_Enc.VOP3bC, 184, 0, 0x7D700000, 0x002D),
new InstInfo(0804, "v_cmpx_eq_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 18, 805, 0x7C240000, 0x002D),
new InstInfo(0805, "v_cmpx_eq_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 18, 0, 0x7C240000, 0x002D),
new InstInfo(0806, "v_cmpx_eq_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 50, 807, 0x7C640000, 0x002D),
new InstInfo(0807, "v_cmpx_eq_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 50, 0, 0x7C640000, 0x002D),
new InstInfo(0808, "v_cmpx_eq_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Also write EXEC.", @"", ISA_Enc.VOPC, 146, 809, 0x7D240000, 0x002D),
new InstInfo(0809, "v_cmpx_eq_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 146, 0, 0x7D240000, 0x002D),
new InstInfo(0810, "v_cmpx_eq_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Also write EXEC.", @"", ISA_Enc.VOPC, 178, 811, 0x7D640000, 0x002D),
new InstInfo(0811, "v_cmpx_eq_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 178, 0, 0x7D640000, 0x002D),
new InstInfo(0812, "v_cmpx_eq_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Also write EXEC.", @"", ISA_Enc.VOPC, 210, 813, 0x7DA40000, 0x002D),
new InstInfo(0813, "v_cmpx_eq_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 210, 0, 0x7DA40000, 0x002D),
new InstInfo(0814, "v_cmpx_eq_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Also write EXEC.", @"", ISA_Enc.VOPC, 242, 815, 0x7DE40000, 0x002D),
new InstInfo(0815, "v_cmpx_eq_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 == S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 242, 0, 0x7DE40000, 0x002D),
new InstInfo(0816, "v_cmpx_f_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 16, 817, 0x7C200000, 0x002D),
new InstInfo(0817, "v_cmpx_f_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 16, 0, 0x7C200000, 0x002D),
new InstInfo(0818, "v_cmpx_f_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 48, 819, 0x7C600000, 0x002D),
new InstInfo(0819, "v_cmpx_f_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 0; Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 48, 0, 0x7C600000, 0x002D),
new InstInfo(0820, "v_cmpx_f_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = 0; Also write EXEC.", @"", ISA_Enc.VOPC, 144, 821, 0x7D200000, 0x002D),
new InstInfo(0821, "v_cmpx_f_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = 0; Also write EXEC.", @"", ISA_Enc.VOP3bC, 144, 0, 0x7D200000, 0x002D),
new InstInfo(0822, "v_cmpx_f_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = 0; Also write EXEC.", @"", ISA_Enc.VOPC, 176, 823, 0x7D600000, 0x002D),
new InstInfo(0823, "v_cmpx_f_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = 0; Also write EXEC.", @"", ISA_Enc.VOP3bC, 176, 0, 0x7D600000, 0x002D),
new InstInfo(0824, "v_cmpx_f_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = 0; Also write EXEC.", @"", ISA_Enc.VOPC, 208, 825, 0x7DA00000, 0x002D),
new InstInfo(0825, "v_cmpx_f_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = 0; Also write EXEC.", @"", ISA_Enc.VOP3bC, 208, 0, 0x7DA00000, 0x002D),
new InstInfo(0826, "v_cmpx_f_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = 0; Also write EXEC.", @"", ISA_Enc.VOPC, 240, 827, 0x7DE00000, 0x002D),
new InstInfo(0827, "v_cmpx_f_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = 0; Also write EXEC.", @"", ISA_Enc.VOP3bC, 240, 0, 0x7DE00000, 0x002D),
new InstInfo(0828, "v_cmpx_ge_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 22, 829, 0x7C2C0000, 0x002D),
new InstInfo(0829, "v_cmpx_ge_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 22, 0, 0x7C2C0000, 0x002D),
new InstInfo(0830, "v_cmpx_ge_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 54, 831, 0x7C6C0000, 0x002D),
new InstInfo(0831, "v_cmpx_ge_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 54, 0, 0x7C6C0000, 0x002D),
new InstInfo(0832, "v_cmpx_ge_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Also write EXEC.", @"", ISA_Enc.VOPC, 150, 833, 0x7D2C0000, 0x002D),
new InstInfo(0833, "v_cmpx_ge_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 150, 0, 0x7D2C0000, 0x002D),
new InstInfo(0834, "v_cmpx_ge_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Also write EXEC.", @"", ISA_Enc.VOPC, 182, 835, 0x7D6C0000, 0x002D),
new InstInfo(0835, "v_cmpx_ge_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 182, 0, 0x7D6C0000, 0x002D),
new InstInfo(0836, "v_cmpx_ge_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Also write EXEC.", @"", ISA_Enc.VOPC, 214, 837, 0x7DAC0000, 0x002D),
new InstInfo(0837, "v_cmpx_ge_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 214, 0, 0x7DAC0000, 0x002D),
new InstInfo(0838, "v_cmpx_ge_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Also write EXEC.", @"", ISA_Enc.VOPC, 246, 839, 0x7DEC0000, 0x002D),
new InstInfo(0839, "v_cmpx_ge_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 >= S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 246, 0, 0x7DEC0000, 0x002D),
new InstInfo(0840, "v_cmpx_gt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 20, 841, 0x7C280000, 0x002D),
new InstInfo(0841, "v_cmpx_gt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 20, 0, 0x7C280000, 0x002D),
new InstInfo(0842, "v_cmpx_gt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 52, 843, 0x7C680000, 0x002D),
new InstInfo(0843, "v_cmpx_gt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 52, 0, 0x7C680000, 0x002D),
new InstInfo(0844, "v_cmpx_gt_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Also write EXEC.", @"", ISA_Enc.VOPC, 148, 845, 0x7D280000, 0x002D),
new InstInfo(0845, "v_cmpx_gt_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 148, 0, 0x7D280000, 0x002D),
new InstInfo(0846, "v_cmpx_gt_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Also write EXEC.", @"", ISA_Enc.VOPC, 180, 847, 0x7D680000, 0x002D),
new InstInfo(0847, "v_cmpx_gt_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 180, 0, 0x7D680000, 0x002D),
new InstInfo(0848, "v_cmpx_gt_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Also write EXEC.", @"", ISA_Enc.VOPC, 212, 849, 0x7DA80000, 0x002D),
new InstInfo(0849, "v_cmpx_gt_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 212, 0, 0x7DA80000, 0x002D),
new InstInfo(0850, "v_cmpx_gt_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Also write EXEC.", @"", ISA_Enc.VOPC, 244, 851, 0x7DE80000, 0x002D),
new InstInfo(0851, "v_cmpx_gt_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 > S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 244, 0, 0x7DE80000, 0x002D),
new InstInfo(0852, "v_cmpx_le_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 19, 853, 0x7C260000, 0x002D),
new InstInfo(0853, "v_cmpx_le_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 19, 0, 0x7C260000, 0x002D),
new InstInfo(0854, "v_cmpx_le_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 51, 855, 0x7C660000, 0x002D),
new InstInfo(0855, "v_cmpx_le_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 51, 0, 0x7C660000, 0x002D),
new InstInfo(0856, "v_cmpx_le_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Also write EXEC.", @"", ISA_Enc.VOPC, 147, 857, 0x7D260000, 0x002D),
new InstInfo(0857, "v_cmpx_le_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 147, 0, 0x7D260000, 0x002D),
new InstInfo(0858, "v_cmpx_le_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Also write EXEC.", @"", ISA_Enc.VOPC, 179, 859, 0x7D660000, 0x002D),
new InstInfo(0859, "v_cmpx_le_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 179, 0, 0x7D660000, 0x002D),
new InstInfo(0860, "v_cmpx_le_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Also write EXEC.", @"", ISA_Enc.VOPC, 211, 861, 0x7DA60000, 0x002D),
new InstInfo(0861, "v_cmpx_le_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 211, 0, 0x7DA60000, 0x002D),
new InstInfo(0862, "v_cmpx_le_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Also write EXEC.", @"", ISA_Enc.VOPC, 243, 863, 0x7DE60000, 0x002D),
new InstInfo(0863, "v_cmpx_le_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <= S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 243, 0, 0x7DE60000, 0x002D),
new InstInfo(0864, "v_cmpx_lg_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 21, 865, 0x7C2A0000, 0x002D),
new InstInfo(0865, "v_cmpx_lg_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 21, 0, 0x7C2A0000, 0x002D),
new InstInfo(0866, "v_cmpx_lg_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 53, 867, 0x7C6A0000, 0x002D),
new InstInfo(0867, "v_cmpx_lg_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 53, 0, 0x7C6A0000, 0x002D),
new InstInfo(0868, "v_cmpx_lg_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Also write EXEC.", @"", ISA_Enc.VOPC, 149, 869, 0x7D2A0000, 0x002D),
new InstInfo(0869, "v_cmpx_lg_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 149, 0, 0x7D2A0000, 0x002D),
new InstInfo(0870, "v_cmpx_lg_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Also write EXEC.", @"", ISA_Enc.VOPC, 181, 871, 0x7D6A0000, 0x002D),
new InstInfo(0871, "v_cmpx_lg_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 181, 0, 0x7D6A0000, 0x002D),
new InstInfo(0872, "v_cmpx_lg_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Also write EXEC.", @"", ISA_Enc.VOPC, 213, 873, 0x7DAA0000, 0x002D),
new InstInfo(0873, "v_cmpx_lg_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 213, 0, 0x7DAA0000, 0x002D),
new InstInfo(0874, "v_cmpx_lg_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Also write EXEC.", @"", ISA_Enc.VOPC, 245, 875, 0x7DEA0000, 0x002D),
new InstInfo(0875, "v_cmpx_lg_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 <> S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 245, 0, 0x7DEA0000, 0x002D),
new InstInfo(0876, "v_cmpx_lt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 17, 877, 0x7C220000, 0x002D),
new InstInfo(0877, "v_cmpx_lt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 17, 0, 0x7C220000, 0x002D),
new InstInfo(0878, "v_cmpx_lt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 49, 879, 0x7C620000, 0x002D),
new InstInfo(0879, "v_cmpx_lt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 49, 0, 0x7C620000, 0x002D),
new InstInfo(0880, "v_cmpx_lt_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Also write EXEC.", @"", ISA_Enc.VOPC, 145, 881, 0x7D220000, 0x002D),
new InstInfo(0881, "v_cmpx_lt_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 145, 0, 0x7D220000, 0x002D),
new InstInfo(0882, "v_cmpx_lt_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Also write EXEC.", @"", ISA_Enc.VOPC, 177, 883, 0x7D620000, 0x002D),
new InstInfo(0883, "v_cmpx_lt_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 177, 0, 0x7D620000, 0x002D),
new InstInfo(0884, "v_cmpx_lt_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Also write EXEC.", @"", ISA_Enc.VOPC, 209, 885, 0x7DA20000, 0x002D),
new InstInfo(0885, "v_cmpx_lt_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 209, 0, 0x7DA20000, 0x002D),
new InstInfo(0886, "v_cmpx_lt_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Also write EXEC.", @"", ISA_Enc.VOPC, 241, 887, 0x7DE20000, 0x002D),
new InstInfo(0887, "v_cmpx_lt_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = (S0 < S1); Also write EXEC.", @"", ISA_Enc.VOP3bC, 241, 0, 0x7DE20000, 0x002D),
new InstInfo(0888, "v_cmpx_neq_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 29, 889, 0x7C3A0000, 0x002D),
new InstInfo(0889, "v_cmpx_neq_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 29, 0, 0x7C3A0000, 0x002D),
new InstInfo(0890, "v_cmpx_neq_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 61, 891, 0x7C7A0000, 0x002D),
new InstInfo(0891, "v_cmpx_neq_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 == S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 61, 0, 0x7C7A0000, 0x002D),
new InstInfo(0892, "v_cmpx_nge_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 25, 893, 0x7C320000, 0x002D),
new InstInfo(0893, "v_cmpx_nge_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 25, 0, 0x7C320000, 0x002D),
new InstInfo(0894, "v_cmpx_nge_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 57, 895, 0x7C720000, 0x002D),
new InstInfo(0895, "v_cmpx_nge_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 >= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 57, 0, 0x7C720000, 0x002D),
new InstInfo(0896, "v_cmpx_ngt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 27, 897, 0x7C360000, 0x002D),
new InstInfo(0897, "v_cmpx_ngt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 27, 0, 0x7C360000, 0x002D),
new InstInfo(0898, "v_cmpx_ngt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 59, 899, 0x7C760000, 0x002D),
new InstInfo(0899, "v_cmpx_ngt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 > S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 59, 0, 0x7C760000, 0x002D),
new InstInfo(0900, "v_cmpx_nle_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 28, 901, 0x7C380000, 0x002D),
new InstInfo(0901, "v_cmpx_nle_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 28, 0, 0x7C380000, 0x002D),
new InstInfo(0902, "v_cmpx_nle_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 60, 903, 0x7C780000, 0x002D),
new InstInfo(0903, "v_cmpx_nle_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <= S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 60, 0, 0x7C780000, 0x002D),
new InstInfo(0904, "v_cmpx_nlg_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 26, 905, 0x7C340000, 0x002D),
new InstInfo(0905, "v_cmpx_nlg_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 26, 0, 0x7C340000, 0x002D),
new InstInfo(0906, "v_cmpx_nlg_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 58, 907, 0x7C740000, 0x002D),
new InstInfo(0907, "v_cmpx_nlg_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 <> S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 58, 0, 0x7C740000, 0x002D),
new InstInfo(0908, "v_cmpx_nlt_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 30, 909, 0x7C3C0000, 0x002D),
new InstInfo(0909, "v_cmpx_nlt_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 30, 0, 0x7C3C0000, 0x002D),
new InstInfo(0910, "v_cmpx_nlt_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 62, 911, 0x7C7C0000, 0x002D),
new InstInfo(0911, "v_cmpx_nlt_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = !(S0 < S1); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 62, 0, 0x7C7C0000, 0x002D),
new InstInfo(0912, "v_cmpx_o_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 23, 913, 0x7C2E0000, 0x002D),
new InstInfo(0913, "v_cmpx_o_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 23, 0, 0x7C2E0000, 0x002D),
new InstInfo(0914, "v_cmpx_o_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 55, 915, 0x7C6E0000, 0x002D),
new InstInfo(0915, "v_cmpx_o_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) && !isNaN(S1)); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 55, 0, 0x7C6E0000, 0x002D),
new InstInfo(0916, "v_cmpx_tru_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 31, 917, 0x7C3E0000, 0x002D),
new InstInfo(0917, "v_cmpx_tru_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 31, 0, 0x7C3E0000, 0x002D),
new InstInfo(0918, "v_cmpx_tru_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 63, 919, 0x7C7E0000, 0x002D),
new InstInfo(0919, "v_cmpx_tru_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = 1; Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 63, 0, 0x7C7E0000, 0x002D),
new InstInfo(0920, "v_cmpx_tru_i32", "vcc", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = 1; Also write EXEC.", @"", ISA_Enc.VOPC, 151, 921, 0x7D2E0000, 0x002D),
new InstInfo(0921, "v_cmpx_tru_i32_ext", "s8b", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = 1; Also write EXEC.", @"", ISA_Enc.VOP3bC, 151, 0, 0x7D2E0000, 0x002D),
new InstInfo(0922, "v_cmpx_tru_i64", "vcc", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = 1; Also write EXEC.", @"", ISA_Enc.VOPC, 183, 923, 0x7D6E0000, 0x002D),
new InstInfo(0923, "v_cmpx_tru_i64_ext", "s8b", "v8i", "v8i", "none", "none", "none", "none", 3, 3, @"D.u = 1; Also write EXEC.", @"", ISA_Enc.VOP3bC, 183, 0, 0x7D6E0000, 0x002D),
new InstInfo(0924, "v_cmpx_tru_u32", "vcc", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = 1; Also write EXEC.", @"", ISA_Enc.VOPC, 215, 925, 0x7DAE0000, 0x002D),
new InstInfo(0925, "v_cmpx_tru_u32_ext", "s8b", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"D.u = 1; Also write EXEC.", @"", ISA_Enc.VOP3bC, 215, 0, 0x7DAE0000, 0x002D),
new InstInfo(0926, "v_cmpx_tru_u64", "vcc", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = 1; Also write EXEC.", @"", ISA_Enc.VOPC, 247, 927, 0x7DEE0000, 0x002D),
new InstInfo(0927, "v_cmpx_tru_u64_ext", "s8b", "v8u", "v8u", "none", "none", "none", "none", 3, 3, @"D.u = 1; Also write EXEC.", @"", ISA_Enc.VOP3bC, 247, 0, 0x7DEE0000, 0x002D),
new InstInfo(0928, "v_cmpx_u_f32", "vcc", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 24, 929, 0x7C300000, 0x002D),
new InstInfo(0929, "v_cmpx_u_f32_ext", "s8b", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 24, 0, 0x7C300000, 0x002D),
new InstInfo(0930, "v_cmpx_u_f64", "vcc", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOPC, 56, 931, 0x7C700000, 0x002D),
new InstInfo(0931, "v_cmpx_u_f64_ext", "s8b", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.u = (!isNaN(S0) || !isNaN(S1)); Signal on sNaNinput only. Also write EXEC.", @"", ISA_Enc.VOP3bC, 56, 0, 0x7C700000, 0x002D),
new InstInfo(0932, "v_cndmask_b32", "v4b", "v4b", "v4b", "vcc", "none", "none", "none", 4, 4, @"Boolean conditional based on bit mask from SGPRs or VCC.<br>D.u = VCC[i] ? S1.u : S0.u (i = threadID in wave); VOP3: specify VCC as a scalar GPR in S2. ", @"", ISA_Enc.VOP2, 0, 933, 0x00000000, 0x0045),
new InstInfo(0933, "v_cndmask_b32_ext", "v4b", "v4b", "v4b", "s4b", "none", "none", "none", 4, 4, @"Boolean conditional based on bit mask from SGPRs or VCC.<br>D.u = VCC[i] ? S1.u : S0.u (i = threadID in wave); VOP3: specify VCC as a scalar GPR in S2. ", @"", ISA_Enc.VOP3a3, 256, 0, 0xD2000000, 0x0047),
new InstInfo(0934, "v_cos_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Cos function.<br>Input must be normalized from radians by dividing by 2*PI.<br>Valid input domain [-256, +256], which corresponds to an un-normalized input domain [-512*PI, +512*PI].<br>Out-of-range input results in float 1.<br>D.f = cos(S0.f). ", @"", ISA_Enc.VOP1, 54, 935, 0x7E006C00, 0x0005),
new InstInfo(0935, "v_cos_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Cos function.<br>Input must be normalized from radians by dividing by 2*PI.<br>Valid input domain [-256, +256], which corresponds to an un-normalized input domain [-512*PI, +512*PI].<br>Out-of-range input results in float 1.<br>D.f = cos(S0.f). ", @"", ISA_Enc.VOP3a1, 438, 0, 0xD36C0000, 0x0007),
new InstInfo(0936, "v_cubeid_f32", "v4f", "v4f", "v4f", "v4f", "none", "none", "none", 4, 4, @"Cubemap Face ID determination. Result is a floating point face ID.<br>S0.f = x<br>S1.f = y<br>S2.f = z<br>If (Abs(S2.f) >= Abs(S0.f) && Abs(S2.f) >= Abs(S1.f))<br>   If (S2.f < 0) D.f = 5.0<br>   Else D.f = 4.0<br>Else if (Abs(S1.f) >= Abs(S0.f))<br>   If (S1.f < 0) D.f = 3.0<br>   Else D.f = 2.0<br>Else<br>   If (S0.f < 0) D.f = 1.0<br>   Else D.f = 0.0 ", @"", ISA_Enc.VOP3a3, 324, 0, 0xD2880000, 0x0007),
new InstInfo(0937, "v_cubema_f32", "v4f", "v4f", "v4f", "v4f", "none", "none", "none", 4, 4, @"Cubemap Major Axis determination. Result is 2.0 * Major Axis.<br>S0.f = x<br>S1.f = y<br>S2.f = z<br>If (Abs(S2.f) >= Abs(S0.f) &&<br>      Abs(S2.f) >= Abs(S1.f))<br>   D.f = 2.0*S2.f<br>Else if (Abs(S1.f) >= Abs(S0.f))<br>   D.f = 2.0 * S1.f<br>Else<br>   D.f = 2.0 * S0.f ", @"", ISA_Enc.VOP3a3, 327, 0, 0xD28E0000, 0x0007),
new InstInfo(0938, "v_cubesc_f32", "v4f", "v4f", "v4f", "v4f", "none", "none", "none", 4, 4, @"Cubemap S coordination determination.<br>S0.f = x<br>S1.f = y<br>S2.f = z<br>If (Abs(S2.f) >= Abs(S0.f) &&<br>      Abs(S2.f) >= Abs(S1.f))<br>   If (S2.f < 0) D.f = -S0.f<br>   Else D.f = S0.f<br>Else if (Abs(S1.f) >= Abs(S0.f))<br>   D.f = S0.f<br>Else<br>   If (S0.f < 0) D.f = S2.f<br>   Else D.f = -S2.f ", @"", ISA_Enc.VOP3a3, 325, 0, 0xD28A0000, 0x0007),
new InstInfo(0939, "v_cubetc_f32", "v4f", "v4f", "v4f", "v4f", "none", "none", "none", 4, 4, @"Cubemap T coordinate determination.<br>S0.f = x<br>S1.f = y<br>S2.f = z<br>If (Abs(S2.f) >= Abs(S0.f) &&<br>      Abs(S2.f) >= Abs(S1.f))<br>   D.f = -S1.f<br>Else if (Abs(S1.f) >= Abs(S0.f))<br>   If (S1.f < 0) D.f = -S2.f<br>   Else D.f = S2.f<br>Else<br>   D.f = -S1.f ", @"", ISA_Enc.VOP3a3, 326, 0, 0xD28C0000, 0x0007),
new InstInfo(0940, "v_cvt_f16_f32", "v2f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Float32 to Float16.<br>D.f16 = flt32_to_flt16(S0.f). ", @"", ISA_Enc.VOP1, 10, 941, 0x7E001400, 0x0005),
new InstInfo(0941, "v_cvt_f16_f32_ext", "v2f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Float32 to Float16.<br>D.f16 = flt32_to_flt16(S0.f). ", @"", ISA_Enc.VOP3a1, 394, 0, 0xD3140000, 0x0007),
new InstInfo(0942, "v_cvt_f32_f16", "v4f", "v2f", "none", "none", "none", "none", "none", 2, 2, @"DX11 Float16 to Float32.<br>D.f = flt16_to_flt32(S0.f16). ", @"", ISA_Enc.VOP1, 11, 943, 0x7E001600, 0x0005),
new InstInfo(0943, "v_cvt_f32_f16_ext", "v4f", "v2f", "none", "none", "none", "none", "none", 2, 2, @"DX11 Float16 to Float32.<br>D.f = flt16_to_flt32(S0.f16). ", @"", ISA_Enc.VOP3a1, 395, 0, 0xD3160000, 0x0007),
new InstInfo(0944, "v_cvt_f32_f64", "v4f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Convert Double Precision Float to Single Precision Float.<br>Overflows obey round mode rules. Infinity is exact.<br>D.f = (float)S0.d. ", @"", ISA_Enc.VOP1, 15, 945, 0x7E001E00, 0x0005),
new InstInfo(0945, "v_cvt_f32_f64_ext", "v4f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Convert Double Precision Float to Single Precision Float.<br>Overflows obey round mode rules. Infinity is exact.<br>D.f = (float)S0.d. ", @"", ISA_Enc.VOP3a1, 399, 0, 0xD31E0000, 0x0007),
new InstInfo(0946, "v_cvt_f32_i32", "v4f", "v4i", "none", "none", "none", "none", "none", 2, 2, @"The input is interpreted as a signed integer value and converted to a float. <br>D.f = (float)S0.i. ", @"", ISA_Enc.VOP1, 5, 947, 0x7E000A00, 0x0005),
new InstInfo(0947, "v_cvt_f32_i32_ext", "v4f", "v4i", "none", "none", "none", "none", "none", 2, 2, @"The input is interpreted as a signed integer value and converted to a float. <br>D.f = (float)S0.i. ", @"", ISA_Enc.VOP3a1, 389, 0, 0xD30A0000, 0x0007),
new InstInfo(0948, "v_cvt_f32_u32", "v4f", "v4u", "none", "none", "none", "none", "none", 2, 2, @"The input is interpreted as an unsigned integer value and converted to a float. <br>D.f = (float)S0.u. ", @"", ISA_Enc.VOP1, 6, 949, 0x7E000C00, 0x0005),
new InstInfo(0949, "v_cvt_f32_u32_ext", "v4f", "v4u", "none", "none", "none", "none", "none", 2, 2, @"The input is interpreted as an unsigned integer value and converted to a float. <br>D.f = (float)S0.u. ", @"", ISA_Enc.VOP3a1, 390, 0, 0xD30C0000, 0x0007),
new InstInfo(0950, "v_cvt_f32_ubyte0", "v4f", "v4u", "none", "none", "none", "none", "none", 2, 2, @"Byte 0 to float. Perform unsigned int to float conversion on byte 0 of S0.<br>D.f = UINT2FLT(S0.u[7:0]). ", @"", ISA_Enc.VOP1, 17, 951, 0x7E002200, 0x0005),
new InstInfo(0951, "v_cvt_f32_ubyte0_ext", "v4f", "v1u", "none", "none", "none", "none", "none", 2, 2, @"Byte 0 to float. Perform unsigned int to float conversion on byte 0 of S0.<br>D.f = UINT2FLT(S0.u[7:0]). ", @"", ISA_Enc.VOP3a1, 401, 0, 0xD3220000, 0x0007),
new InstInfo(0952, "v_cvt_f32_ubyte1", "v4f", "v4u", "none", "none", "none", "none", "none", 2, 2, @"Byte 1 to float. Perform unsigned int to float conversion on byte 1 of S0.<br>D.f = UINT2FLT(S0.u[15:8]). ", @"", ISA_Enc.VOP1, 18, 953, 0x7E002400, 0x0005),
new InstInfo(0953, "v_cvt_f32_ubyte1_ext", "v4f", "v2u", "none", "none", "none", "none", "none", 2, 2, @"Byte 1 to float. Perform unsigned int to float conversion on byte 1 of S0.<br>D.f = UINT2FLT(S0.u[15:8]). ", @"", ISA_Enc.VOP3a1, 402, 0, 0xD3240000, 0x0007),
new InstInfo(0954, "v_cvt_f32_ubyte2", "v4f", "v4u", "none", "none", "none", "none", "none", 2, 2, @"Byte 2 to float. Perform unsigned int to float conversion on byte 2 of S0.<br>D.f = UINT2FLT(S0.u[23:16]). ", @"", ISA_Enc.VOP1, 19, 955, 0x7E002600, 0x0005),
new InstInfo(0955, "v_cvt_f32_ubyte2_ext", "v4f", "v3u", "none", "none", "none", "none", "none", 2, 2, @"Byte 2 to float. Perform unsigned int to float conversion on byte 2 of S0.<br>D.f = UINT2FLT(S0.u[23:16]). ", @"", ISA_Enc.VOP3a1, 403, 0, 0xD3260000, 0x0007),
new InstInfo(0956, "v_cvt_f32_ubyte3", "v4f", "v4u", "none", "none", "none", "none", "none", 2, 2, @"Byte 3 to float. Perform unsigned int to float conversion on byte 3 of S0.<br>D.f = UINT2FLT(S0.u[31:24]). ", @"", ISA_Enc.VOP1, 20, 957, 0x7E002800, 0x0005),
new InstInfo(0957, "v_cvt_f32_ubyte3_ext", "v4f", "v4u", "none", "none", "none", "none", "none", 2, 2, @"Byte 3 to float. Perform unsigned int to float conversion on byte 3 of S0.<br>D.f = UINT2FLT(S0.u[31:24]). ", @"", ISA_Enc.VOP3a1, 404, 0, 0xD3280000, 0x0007),
new InstInfo(0958, "v_cvt_f64_f32", "v8f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Convert Single Precision Float to Double Precision Float.<br>D.d = (double)S0.f. ", @"", ISA_Enc.VOP1, 16, 959, 0x7E002000, 0x0005),
new InstInfo(0959, "v_cvt_f64_f32_ext", "v8f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Convert Single Precision Float to Double Precision Float.<br>D.d = (double)S0.f. ", @"", ISA_Enc.VOP3a1, 400, 0, 0xD3200000, 0x0007),
new InstInfo(0960, "v_cvt_f64_i32", "v8f", "v4i", "none", "none", "none", "none", "none", 2, 2, @"Convert Signed Integer to Double Precision Float.<br>D.f = (float)S0.i. ", @"", ISA_Enc.VOP1, 4, 961, 0x7E000800, 0x0005),
new InstInfo(0961, "v_cvt_f64_i32_ext", "v8f", "v4i", "none", "none", "none", "none", "none", 2, 2, @"Convert Signed Integer to Double Precision Float.<br>D.f = (float)S0.i. ", @"", ISA_Enc.VOP3a1, 388, 0, 0xD3080000, 0x0007),
new InstInfo(0962, "v_cvt_f64_u32", "v8f", "v4u", "none", "none", "none", "none", "none", 2, 2, @"Convert Unsigned Integer to Double Precision Float.<br>D.d = (double)S0.u. ", @"", ISA_Enc.VOP1, 22, 963, 0x7E002C00, 0x0005),
new InstInfo(0963, "v_cvt_f64_u32_ext", "v8f", "v4u", "none", "none", "none", "none", "none", 2, 2, @"Convert Unsigned Integer to Double Precision Float.<br>D.d = (double)S0.u. ", @"", ISA_Enc.VOP3a1, 406, 0, 0xD32C0000, 0x0007),
new InstInfo(0964, "v_cvt_flr_i32_f32", "v4i", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Float input is converted to a signed integer value using floor function.  Float magnitudes too great to be represented by an integer float (unbiased exponent > 30) saturate to max_int or -max_int.<br>D.i = (int)floor(S0.f). ", @"", ISA_Enc.VOP1, 13, 965, 0x7E001A00, 0x0005),
new InstInfo(0965, "v_cvt_flr_i32_f32_ext", "v4i", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Float input is converted to a signed integer value using floor function.  Float magnitudes too great to be represented by an integer float (unbiased exponent > 30) saturate to max_int or -max_int.<br>D.i = (int)floor(S0.f). ", @"", ISA_Enc.VOP3a1, 397, 0, 0xD31A0000, 0x0007),
new InstInfo(0966, "v_cvt_i32_f32", "v4i", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Float input is converted to a signed integer using truncation.<br>Float magnitudes too great to be represented by an integer float (unbiased exponent > 30) saturate to max_int or -max_int.<br>Special case number handling:<br>inf -->  max_int<br>-inf -->  -max_int<br>NaN & -Nan & 0 & -0 -->  0<br>D.i = (int)S0.f. ", @"", ISA_Enc.VOP1, 8, 967, 0x7E001000, 0x0005),
new InstInfo(0967, "v_cvt_i32_f32_ext", "v4i", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Float input is converted to a signed integer using truncation.<br>Float magnitudes too great to be represented by an integer float (unbiased exponent > 30) saturate to max_int or -max_int.<br>Special case number handling:<br>inf -->  max_int<br>-inf -->  -max_int<br>NaN & -Nan & 0 & -0 -->  0<br>D.i = (int)S0.f. ", @"", ISA_Enc.VOP3a1, 392, 0, 0xD3100000, 0x0007),
new InstInfo(0968, "v_cvt_i32_f64", "v4i", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Covert Double Precision Float to Signed Integer.<br>Truncate (round-to-zero) only. Other round modes require a rne_f64, ceil_f64 or floor_f64 pre-op. Float magnitudes too great to be represented by an integer float (unbiased exponent > 30) saturate to max_int or -max_int.<br>Special case number handling:<br>inf --> max_int<br>-inf --> -max_int<br>NaN & -Nan & 0 & -0 --> 0<br>D.i = (int)S0.d. ", @"", ISA_Enc.VOP1, 3, 969, 0x7E000600, 0x0005),
new InstInfo(0969, "v_cvt_i32_f64_ext", "v4f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Covert Double Precision Float to Signed Integer.<br>Truncate (round-to-zero) only. Other round modes require a rne_f64, ceil_f64 or floor_f64 pre-op. Float magnitudes too great to be represented by an integer float (unbiased exponent > 30) saturate to max_int or -max_int.<br>Special case number handling:<br>inf --> max_int<br>-inf --> -max_int<br>NaN & -Nan & 0 & -0 --> 0<br>D.i = (int)S0.d. ", @"", ISA_Enc.VOP3a1, 387, 0, 0xD3060000, 0x0007),
new InstInfo(0970, "v_cvt_off_f32_i4", "v4f", "v1i", "none", "none", "none", "none", "none", 2, 2, @"4-bit signed int to 32-bit float. For interpolation in shader.<br>S0Result<br>1000-0.5f<br>1001-0.4375f<br>1010-0.375f<br>1011-0.3125f<br>1100-0.25f<br>1101-0.1875f<br>1110-0.125f<br>1111-0.0625f<br>00000.0f<br>00010.0625f<br>00100.125f<br>00110.1875f<br>01000.25f<br>01010.3125f<br>01100.375f<br>01110.4375f ", @"", ISA_Enc.VOP1, 14, 971, 0x7E001C00, 0x0005),
new InstInfo(0971, "v_cvt_off_f32_i4_ext", "v4f", "v1i", "none", "none", "none", "none", "none", 2, 2, @"4-bit signed int to 32-bit float. For interpolation in shader.<br>S0Result<br>1000-0.5f<br>1001-0.4375f<br>1010-0.375f<br>1011-0.3125f<br>1100-0.25f<br>1101-0.1875f<br>1110-0.125f<br>1111-0.0625f<br>00000.0f<br>00010.0625f<br>00100.125f<br>00110.1875f<br>01000.25f<br>01010.3125f<br>01100.375f<br>01110.4375f ", @"", ISA_Enc.VOP3a1, 398, 0, 0xD31C0000, 0x0007),
new InstInfo(0972, "v_cvt_pk_i16_i32", "v2i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"DX signed 32-bit integer to signed 16-bit integer.<br>Overflow clamped to 0x7FFF. Underflow clamped to 0x8000.<br>D = {(i32.i16)S1.i, (i32.i16)S0.i}. ", @"", ISA_Enc.VOP2, 49, 973, 0x62000000, 0x0005),
new InstInfo(0973, "v_cvt_pk_i16_i32_ext", "v2i", "v4i", "v4u", "none", "none", "none", "none", 3, 3, @"DX signed 32-bit integer to signed 16-bit integer.<br>Overflow clamped to 0x7FFF. Underflow clamped to 0x8000.<br>D = {(i32.i16)S1.i, (i32.i16)S0.i}. ", @"", ISA_Enc.VOP3a2, 305, 0, 0xD2620000, 0x0007),
new InstInfo(0974, "v_cvt_pk_u16_u32", "v2u", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"DX11 unsigned 32-bit integer to unsigned 16-bit integer.<br>Overflow clamped to 0xFFFF.<br>D = {(u32.u16)S1.u, (u32.u16)S0.u}. ", @"", ISA_Enc.VOP2, 48, 975, 0x60000000, 0x0005),
new InstInfo(0975, "v_cvt_pk_u16_u32_ext", "v2u", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"DX11 unsigned 32-bit integer to unsigned 16-bit integer.<br>Overflow clamped to 0xFFFF.<br>D = {(u32.u16)S1.u, (u32.u16)S0.u}. ", @"", ISA_Enc.VOP3a2, 304, 0, 0xD2600000, 0x0007),
new InstInfo(0976, "v_cvt_pk_u8_f32", "v4b", "v4f", "v1b", "v4b", "none", "none", "none", 4, 4, @"Float to 8 bit unsigned integer conversion<br>Replacement for 8xx/9xx FLT_TO_UINT4 opcode.<br>Float to 8 bit uint conversion placed into any byte of result, accumulated with S2.f. Four applications of this opcode can accumulate 4 8-bit integers packed into a single dword.<br>D.f = ((flt_to_uint(S0.f) & 0xff) <<<br>  8*S1.f[1:0])) || (S2.f & ~(0xff <<<br>  (8*S1.f[1:0])));<br>Intended use, ops in any order:<br>op - cvt_pk_u8_f32 r0 foo2, 2, r0<br>op - cvt_pk_u8_f32 r0 foo1, 1, r0<br>op - cvt_pk_u8_f32 r0 foo3, 3, r0<br>op - cvt_pk_u8_f32 r0 foo0, 0, r0<br>r0 result is 4 bytes packed into a dword:<br>{foo3, foo2, foo1, foo0} ", @"", ISA_Enc.VOP3a3, 350, 0, 0xD2BC0000, 0x0007),
new InstInfo(0977, "v_cvt_pkaccum_u8_f32", "v1u", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"f32.u8(s0.f), pack into byte(s1.u), of dst.  ", @"", ISA_Enc.VOP2, 44, 978, 0x58000000, 0x0005),
new InstInfo(0978, "v_cvt_pkaccum_u8_f32_ext", "v1u", "v4f", "v4b", "none", "none", "none", "none", 3, 3, @"f32.u8(s0.f), pack into byte(s1.u), of dst.  ", @"", ISA_Enc.VOP3a2, 300, 0, 0xD2580000, 0x0007),
new InstInfo(0979, "v_cvt_pknorm_i16_f32", "v2i", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"DX Float32 to SNORM16, a signed, normalized 16-bit value.<br>D = {(snorm)S1.f, (snorm)S0.f}. ", @"", ISA_Enc.VOP2, 45, 980, 0x5A000000, 0x0005),
new InstInfo(0980, "v_cvt_pknorm_i16_f32_ext", "v2i", "v4f", "v4i", "none", "none", "none", "none", 3, 3, @"DX Float32 to SNORM16, a signed, normalized 16-bit value.<br>D = {(snorm)S1.f, (snorm)S0.f}. ", @"", ISA_Enc.VOP3a2, 301, 0, 0xD25A0000, 0x0007),
new InstInfo(0981, "v_cvt_pknorm_u16_f32", "v2u", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"DX Float32 to UNORM16, an unsigned, normalized 16-bit value.<br>D = {(unorm)S1.f, (unorm)S0.f}. ", @"", ISA_Enc.VOP2, 46, 982, 0x5C000000, 0x0005),
new InstInfo(0982, "v_cvt_pknorm_u16_f32_ext", "v2u", "v4f", "v4i", "none", "none", "none", "none", 3, 3, @"DX Float32 to UNORM16, an unsigned, normalized 16-bit value.<br>D = {(unorm)S1.f, (unorm)S0.f}. ", @"", ISA_Enc.VOP3a2, 302, 0, 0xD25C0000, 0x0007),
new InstInfo(0983, "v_cvt_pkrtz_f16_f32", "v2f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"Convert two float 32 numbers into a single register holding two packed 16-bit floats.<br>D = {flt32_to_flt16(S1.f),flt32_to_flt16(S0.f)}, with round-toward-zero. ", @"", ISA_Enc.VOP2, 47, 984, 0x5E000000, 0x0005),
new InstInfo(0984, "v_cvt_pkrtz_f16_f32_ext", "v2f", "v4f", "v4i", "none", "none", "none", "none", 3, 3, @"Convert two float 32 numbers into a single register holding two packed 16-bit floats.<br>D = {flt32_to_flt16(S1.f),flt32_to_flt16(S0.f)}, with round-toward-zero. ", @"", ISA_Enc.VOP3a2, 303, 0, 0xD25E0000, 0x0007),
new InstInfo(0985, "v_cvt_rpi_i32_f32", "v4i", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Float input is converted to a signed integer value using round to positive infinity tiebreaker for 0.5.  Float magnitudes too great to be represented by an integer float (unbiased exponent > 30) saturate to max_int or -max_int.<br>D.i = (int)floor(S0.f + 0.5). ", @"", ISA_Enc.VOP1, 12, 986, 0x7E001800, 0x0005),
new InstInfo(0986, "v_cvt_rpi_i32_f32_ext", "v4i", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Float input is converted to a signed integer value using round to positive infinity tiebreaker for 0.5.  Float magnitudes too great to be represented by an integer float (unbiased exponent > 30) saturate to max_int or -max_int.<br>D.i = (int)floor(S0.f + 0.5). ", @"", ISA_Enc.VOP3a1, 396, 0, 0xD3180000, 0x0007),
new InstInfo(0987, "v_cvt_u32_f32", "v4u", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Input is converted to an unsigned integer value using truncation. Positive float magnitudes too great to be represented by an unsigned integer float (unbiased exponent > 31) saturate to max_uint.<br>Special number handling:<br>-inf & NaN & 0 & -0 . 0<br>Inf . max_uint<br>D.u = (unsigned)S0.f. ", @"", ISA_Enc.VOP1, 7, 988, 0x7E000E00, 0x0005),
new InstInfo(0988, "v_cvt_u32_f32_ext", "v4u", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Input is converted to an unsigned integer value using truncation. Positive float magnitudes too great to be represented by an unsigned integer float (unbiased exponent > 31) saturate to max_uint.<br>Special number handling:<br>-inf & NaN & 0 & -0 . 0<br>Inf . max_uint<br>D.u = (unsigned)S0.f. ", @"", ISA_Enc.VOP3a1, 391, 0, 0xD30E0000, 0x0007),
new InstInfo(0989, "v_cvt_u32_f64", "v4f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Covert Double Precision Float to Unsigned Integer <br>Truncate (round-to-zero) only. Other round modes require a rne_f64, ceil_f64 or floor_f64 pre-op. Positive float magnitudes too great to be represented by an unsigned integer float (unbiased exponent > 31) saturate to max_uint. <br>Special number handling:<br>-inf & NaN & 0 & -0 . 0<br>Inf . max_uint<br>D.u = (uint)S0.d. ", @"", ISA_Enc.VOP1, 21, 990, 0x7E002A00, 0x0005),
new InstInfo(0990, "v_cvt_u32_f64_ext", "v4u", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Covert Double Precision Float to Unsigned Integer <br>Truncate (round-to-zero) only. Other round modes require a rne_f64, ceil_f64 or floor_f64 pre-op. Positive float magnitudes too great to be represented by an unsigned integer float (unbiased exponent > 31) saturate to max_uint. <br>Special number handling:<br>-inf & NaN & 0 & -0 . 0<br>Inf . max_uint<br>D.u = (uint)S0.d. ", @"", ISA_Enc.VOP3a1, 405, 0, 0xD32A0000, 0x0007),
new InstInfo(0991, "v_div_fixup_f32", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"Single precision division fixup.<br>Given a numerator, denominator, and quotient from a divide, this opcode detects and applies special-case numerics, touching up the quotient if necessary. This opcode also generates all exceptions caused by the division. The generation of the inexact exception requires a fused multiple add (FMA), making this opcode a variant of FMA.<br>S0.f = Quotient<br>S1.f = Denominator<br>S2.f = Numerator<br>If (S1.f==Nan && S2.f!=SNan)<br>   D.f = Quiet(S1.f);<br>Else if (S2.f==Nan)<br>   D.f = Quiet(S2.f);<br>Else if (S1.f==S2.f==0)<br>   # 0/0<br>   D.f = pele_nan(0xffc00000);<br>Else if (abs(S1.f)==abs(S2.f)==infinity)<br>   # inf/inf<br>   D.f = pele_nan(0xffc00000);<br>Else if (S1.f==0)<br>   # x/0<br>   D.f = (sign(S1.f)^sign(S0.f) ? -inf : inf;<br>Else if (abs(S1.f)==inf)<br>   # x/inf<br>   D.f = (sign(S1.f)^sign(S0.f) ? -0 : 0;<br>Else if (S0.f==Nan)<br>   # division error correction nan due to N*1/D overflow (result of divide is overflow)<br>   D.f = (sign(S1.f)^sign(S0.f) ? -inf : inf;<br>Else<br>   D.f = S0.f; ", @"", ISA_Enc.VOP3a2, 351, 0, 0xD2BE0000, 0x0007),
new InstInfo(0992, "v_div_fixup_f64", "v8f", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"Double precision division fixup.<br>Given a numerator, denominator, and quotient from a divide, this opcode will detect and apply special case numerics, touching up the quotient if necessary. This opcode also generates all exceptions caused by the division. The generation of the inexact exception requires a fused multiply add (FMA), making this opcode a variant of FMA.<br>D.d = Special case divide fixup and flags(s0.d = Quotient, s1.d = Denominator, s2.d = Numerator). ", @"", ISA_Enc.VOP3a2, 352, 0, 0xD2C00000, 0x0007),
new InstInfo(0993, "v_div_fmas_f32", "v4f", "v4f", "v4f", "v4f", "none", "none", "none", 4, 4, @"D.f = Special case divide FMA with scale and flags(s0.f = Quotient, s1.f = Denominator, s2.f = Numerator). ", @"", ISA_Enc.VOP3a3, 367, 0, 0xD2DE0000, 0x0007),
new InstInfo(0994, "v_div_fmas_f64", "v8f", "v8f", "v8f", "v8f", "none", "none", "none", 4, 4, @"D.d = Special case divide FMA with scale and flags(s0.d = Quotient, s1.d = Denominator, s2.d = Numerator). ", @"", ISA_Enc.VOP3a3, 368, 0, 0xD2E00000, 0x0007),
new InstInfo(0995, "v_div_scale_f32", "v4f", "v4f", "v4f", "v4f", "none", "none", "none", 4, 4, @"D.f = Special case divide preop and flags(s0.f = Quotient, s1.f = Denominator, s2.f = Numerator) s0 must equal s1 or s2. ", @"", ISA_Enc.VOP3b3, 365, 0, 0xD2DA0000, 0x0007),
new InstInfo(0996, "v_div_scale_f64", "v8f", "v8f", "v8f", "v8f", "none", "none", "none", 4, 4, @"D.d = Special case divide preop and flags(s0.d = Quotient, s1.d = Denominator, s2.d = Numerator) s0 must equal s1 or s2. ", @"", ISA_Enc.VOP3b3, 366, 0, 0xD2DC0000, 0x0007),
new InstInfo(0997, "v_exp_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Base2 exponent function.<br>If (Arg1 == 0.0f)<br>  Result = 1.0f;<br>Else <br>  Result = Approximate2ToX(Arg1);", @"", ISA_Enc.VOP1, 37, 998, 0x7E004A00, 0x0005),
new InstInfo(0998, "v_exp_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Base2 exponent function.<br>If (Arg1 == 0.0f)<br>  Result = 1.0f;<br>Else <br>  Result = Approximate2ToX(Arg1);", @"", ISA_Enc.VOP3a1, 421, 0, 0xD34A0000, 0x0007),
new InstInfo(0999, "v_exp_legacy_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Return 2^(argument) floating-point value, using the same precision as Sea Islands.<br>D.f = pow(2.0, S0.f). Same as Sea Islands. ", @"", ISA_Enc.VOP1, 70, 0, 0x7E008C00, 0x0005),
new InstInfo(1000, "v_ffbh_i32", "v4i", "v4i", "none", "none", "none", "none", "none", 2, 2, @"Find first bit signed high.<br>Find first bit set in a positive integer from MSB, or find first bit clear in a negative integer from MSB<br>D.u = position of first bit different from sign bit in S0 from MSB; D=0xFFFFFFFF if S0==0 or 0xFFFFFFFF. ", @"", ISA_Enc.VOP1, 59, 1001, 0x7E007600, 0x0005),
new InstInfo(1001, "v_ffbh_i32_ext", "v4i", "v4i", "none", "none", "none", "none", "none", 2, 2, @"Find first bit signed high.<br>Find first bit set in a positive integer from MSB, or find first bit clear in a negative integer from MSB<br>D.u = position of first bit different from sign bit in S0 from MSB; D=0xFFFFFFFF if S0==0 or 0xFFFFFFFF. ", @"", ISA_Enc.VOP3a1, 443, 0, 0xD3760000, 0x0007),
new InstInfo(1002, "v_ffbh_u32", "v4u", "v4u", "none", "none", "none", "none", "none", 2, 2, @"Find first bit high.<br>D.u = position of first 1 in S0 from MSB; D=0xFFFFFFFF if S0==0. ", @"", ISA_Enc.VOP1, 57, 1003, 0x7E007200, 0x0005),
new InstInfo(1003, "v_ffbh_u32_ext", "v4u", "v4u", "none", "none", "none", "none", "none", 2, 2, @"Find first bit high.<br>D.u = position of first 1 in S0 from MSB; D=0xFFFFFFFF if S0==0. ", @"", ISA_Enc.VOP3a1, 441, 0, 0xD3720000, 0x0007),
new InstInfo(1004, "v_ffbl_b32", "v4u", "v4b", "none", "none", "none", "none", "none", 2, 2, @"Find first bit low.<br>D.u = position of first 1 in S0 from LSB; D=0xFFFFFFFF if S0==0. ", @"", ISA_Enc.VOP1, 58, 1005, 0x7E007400, 0x0005),
new InstInfo(1005, "v_ffbl_b32_ext", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"Find first bit low.<br>D.u = position of first 1 in S0 from LSB; D=0xFFFFFFFF if S0==0. ", @"", ISA_Enc.VOP3a1, 442, 0, 0xD3740000, 0x0007),
new InstInfo(1006, "v_floor_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Floating-point floor function.<br>D.f = trunc(S0); if ((S0 < 0.0) && (S0 != D)) D += -1.0. ", @"", ISA_Enc.VOP1, 36, 1007, 0x7E004800, 0x0005),
new InstInfo(1007, "v_floor_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Floating-point floor function.<br>D.f = trunc(S0); if ((S0 < 0.0) && (S0 != D)) D += -1.0. ", @"", ISA_Enc.VOP3a1, 420, 0, 0xD3480000, 0x0007),
new InstInfo(1008, "v_floor_f64", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"64-bit floating-point floor.<br>D.d = trunc(S0.d); if (S0.d < 0.0 && S0.d != D.d), D.d += -1.0. ", @"", ISA_Enc.VOP1, 26, 0, 0x7E003400, 0x0005),
new InstInfo(1009, "v_fma_f32", "v4f", "v4f", "v4f", "v4f", "none", "none", "none", 4, 4, @"Fused single-precision multiply-add. Only for double-precision parts.<br>D.f = S0.f * S1.f + S2.f. ", @"", ISA_Enc.VOP3a3, 331, 0, 0xD2960000, 0x0007),
new InstInfo(1010, "v_fma_f64", "v8f", "v8f", "v8f", "v8f", "none", "none", "none", 4, 4, @"Double-precision floating-point fused multiply add (FMA).<br>Adds the src2 to the product of the src0 and src1. A single round is performed on the sum - the product of src0 and src1 is not truncated or rounded.<br>The instruction specifies which one of two data elements in a four-element vector is operated on (the two dwords of a double precision floating point number), and the result can be stored in the wz or yx elements of the destination GPR.<br>D.d = S0.d * S1.d + S2.d.", @"", ISA_Enc.VOP3a3, 332, 0, 0xD2980000, 0x0007),
new InstInfo(1011, "v_fract_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Floating point 'fractional' part of S0.f.<br>D.f = S0.f - floor(S0.f). ", @"", ISA_Enc.VOP1, 32, 1012, 0x7E004000, 0x0005),
new InstInfo(1012, "v_fract_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Floating point 'fractional' part of S0.f.<br>D.f = S0.f - floor(S0.f). ", @"", ISA_Enc.VOP3a1, 416, 0, 0xD3400000, 0x0007),
new InstInfo(1013, "v_fract_f64", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Double-precision fractional part of Arg1.<br>Double result written to two consecutive GPRs; the instruction Dest specifies the lesser of the two.<br>D.d = FRAC64(S0.d);<br>Return fractional part of input as double [0.0 - 1.0). ", @"", ISA_Enc.VOP1, 62, 1014, 0x7E007C00, 0x0005),
new InstInfo(1014, "v_fract_f64_ext", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Double-precision fractional part of Arg1.<br>Double result written to two consecutive GPRs; the instruction Dest specifies the lesser of the two.<br>D.d = FRAC64(S0.d);<br>Return fractional part of input as double [0.0 - 1.0). ", @"", ISA_Enc.VOP3a1, 446, 0, 0xD37C0000, 0x0007),
new InstInfo(1015, "v_frexp_exp_i32_f32", "v4i", "v4f", "none", "none", "none", "none", "none", 2, 2, @"C math library frexp function. Returns the exponent of a single precision float input, such that:<br>original single float = significand * 2exponent .<br>D.f = 2's complement (exponent(S0.f) - 127 +1). ", @"", ISA_Enc.VOP1, 63, 1016, 0x7E007E00, 0x0005),
new InstInfo(1016, "v_frexp_exp_i32_f32_ext", "v4i", "v4f", "none", "none", "none", "none", "none", 2, 2, @"C math library frexp function. Returns the exponent of a single precision float input, such that:<br>original single float = significand * 2exponent .<br>D.f = 2's complement (exponent(S0.f) - 127 +1). ", @"", ISA_Enc.VOP3a1, 447, 0, 0xD37E0000, 0x0007),
new InstInfo(1017, "v_frexp_exp_i32_f64", "v4i", "v8f", "none", "none", "none", "none", "none", 2, 2, @"C++ FREXP math function.<br>Returns exponent of double precision float input, such that:<br>original double float = significand * 2exponent .<br>D.i = 2's complement (exponent(S0.d) - 1023 +1). ", @"", ISA_Enc.VOP1, 60, 1018, 0x7E007800, 0x0005),
new InstInfo(1018, "v_frexp_exp_i32_f64_ext", "v4i", "v8f", "none", "none", "none", "none", "none", 2, 2, @"C++ FREXP math function.<br>Returns exponent of double precision float input, such that:<br>original double float = significand * 2exponent .<br>D.i = 2's complement (exponent(S0.d) - 1023 +1). ", @"", ISA_Enc.VOP3a1, 444, 0, 0xD3780000, 0x0007),
new InstInfo(1019, "v_frexp_mant_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"C math library frexp function. Returns binary significand of single precision float input, such that:<br>original single float = significand * 2exponent .<br>D.f =Mantissa(S0.f).<br>D.f range(-1.0,-0.5] or [0.5,1.0). ", @"", ISA_Enc.VOP1, 64, 1020, 0x7E008000, 0x0005),
new InstInfo(1020, "v_frexp_mant_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"C math library frexp function. Returns binary significand of single precision float input, such that:<br>original single float = significand * 2exponent .<br>D.f =Mantissa(S0.f).<br>D.f range(-1.0,-0.5] or [0.5,1.0). ", @"", ISA_Enc.VOP3a1, 448, 0, 0xD3800000, 0x0007),
new InstInfo(1021, "v_frexp_mant_f64", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"C++ FREXP math function.<br>Returns binary significand of double precision float input, such that <br>original double float = significand * 2exponent .<br>D.d =Mantissa(S0.d).<br>D.d range(-1.0,-0.5] or [0.5,1.0). ", @"", ISA_Enc.VOP1, 61, 1022, 0x7E007A00, 0x0005),
new InstInfo(1022, "v_frexp_mant_f64_ext", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"C++ FREXP math function.<br>Returns binary significand of double precision float input, such that <br>original double float = significand * 2exponent .<br>D.d =Mantissa(S0.d).<br>D.d range(-1.0,-0.5] or [0.5,1.0). ", @"", ISA_Enc.VOP3a1, 445, 0, 0xD37A0000, 0x0007),
new InstInfo(1023, "v_interp_mov_f32", "v4f", "v4f", "none", "none", "none", "2u", "6u", 2, 4, @"Vertex Parameter Interpolation using parameters stored in LDS and barycentric coordinates in VGPRs.<br>M0 must contain: { 1'b0, new_prim_mask[15:1], lds_param_offset[15:0] }.<br>The ATTR field indicates which attribute (0-32) to interpolate.<br>The ATTRCHAN field indicates which channel: 0=x, 1=y, 2=z and 3=w. ", @"", ISA_Enc.VINTRP, 2, 0, 0xC8020000, 0x0005),
new InstInfo(1024, "v_interp_p1_f32", "v4f", "v4f", "none", "none", "none", "2u", "6u", 2, 4, @"Vertex Parameter Interpolation using parameters stored in LDS and barycentric coordinates in VGPRs.<br>M0 must contain: { 1'b0, new_prim_mask[15:1], lds_param_offset[15:0] }.<br>The ATTR field indicates which attribute (0-32) to interpolate.<br>The ATTRCHAN field indicates which channel: 0=x, 1=y, 2=z and 3=w. ", @"", ISA_Enc.VINTRP, 0, 0, 0xC8000000, 0x0005),
new InstInfo(1025, "v_interp_p2_f32", "v4f", "v4f", "none", "none", "none", "2u", "6u", 2, 4, @"Vertex Parameter Interpolation using parameters stored in LDS and barycentric coordinates in VGPRs.<br>M0 must contain: { 1'b0, new_prim_mask[15:1], lds_param_offset[15:0] }.<br>The ATTR field indicates which attribute (0-32) to interpolate.<br>The ATTRCHAN field indicates which channel: 0=x, 1=y, 2=z and 3=w. ", @"", ISA_Enc.VINTRP, 1, 0, 0xC8010000, 0x0005),
new InstInfo(1026, "v_ldexp_f32", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"C math library ldexp function.<br>Result = S0.f * (2 ^ S1.i)<br>So = float 32<br>S1 = signed integer ", @"", ISA_Enc.VOP2, 43, 1027, 0x56000000, 0x0005),
new InstInfo(1027, "v_ldexp_f32_ext", "v4f", "v4f", "v4b", "none", "none", "none", "none", 3, 3, @"C math library ldexp function.<br>Result = S0.f * (2 ^ S1.i)<br>So = float 32<br>S1 = signed integer ", @"", ISA_Enc.VOP3a2, 299, 0, 0xD2560000, 0x0007),
new InstInfo(1028, "v_ldexp_f64", "v8f", "v4i", "v8f", "none", "none", "none", "none", 3, 3, @"Double-precision LDEXP from the C math library.<br>This instruction gets a 52-bit mantissa from the double-precision floating-point value in src1.YX and a 32-bit integer exponent in src0.X, and multiplies the mantissa by 2exponent. The double-precision floating-point result is stored in dst.YX. <br>dst = src1 * 2^src0<br>mant  = mantissa(src1)<br>exp   = exponent(src1)<br>sign  = sign(src1)<br>if (exp==0x7FF)           //src1 is inf or a NaN<br>   dst = src1;<br>else if (exp==0x0)      //src1 is zero or a denorm<br>   dst = (sign) ? 0x8000000000000000 : 0x0;<br>else                    //src1 is a float<br>{<br>exp+= src0;<br>if (exp>=0x7FF)     //overflow<br>   dst = {sign,inf};<br>if (src0<=0)              //underflow<br>   dst = {sign,0};<br>mant |= (exp<<52);<br>mant |= (sign<<63);<br>dst = mant;}", @"", ISA_Enc.VOP3a2, 360, 0, 0xD2D00000, 0x0007),
new InstInfo(1029, "v_lerp_u8", "v1u", "v1u", "v4b", "v4b", "none", "none", "none", 4, 4, @"Unsigned eight-bit pixel average on packed unsigned bytes (linear interpolation). S2 acts as a round mode; if set, 0.5 rounds up; otherwise, 0.5 truncates.<br>D.u = ((S0.u[31:24] + S1.u[31:24] + S2.u[24]) >> 1) << 24 + ((S0.u[23:16] + S1.u[23:16] + S2.u[16]) >> 1) << 16 + ((S0.u[15:8] + S1.u[15:8] + S2.u[8]) >> 1) << 8 + ((S0.u[7:0] + S1.u[7:0] + S2.u[0]) >> 1).<br>dst = ((src0[31:24] + src1[31:24] + src2[24]) >> 1) << 24 +<br>((src0[23:16] + src1[23:16] + src2[16]) >>1) << 16 +<br>((src0[15:8] + src1[15:8] + src2[8]) >> 1) << 8 +<br>((src0[7:0] + src1[7:0] + src2[0]) >> 1) ", @"", ISA_Enc.VOP3a3, 333, 0, 0xD29A0000, 0x0007),
new InstInfo(1030, "v_log_clamp_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Base2 log function.<br>The clamp prevents infinite results, clamping infinities to max_float.<br>If (Arg1 == 1.0f)<br>  Result = 0.0f;<br>Else <br>  Result = LOG_IEEE(Arg1)<br>// clamp result<br>if (Result == -INFINITY) <br>  Result = -MAX_FLOAT;", @"", ISA_Enc.VOP1, 38, 1031, 0x7E004C00, 0x0005),
new InstInfo(1031, "v_log_clamp_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Base2 log function.<br>The clamp prevents infinite results, clamping infinities to max_float.<br>If (Arg1 == 1.0f)<br>  Result = 0.0f;<br>Else <br>  Result = LOG_IEEE(Arg1)<br>// clamp result<br>if (Result == -INFINITY) <br>  Result = -MAX_FLOAT;", @"", ISA_Enc.VOP3a1, 422, 0, 0xD34C0000, 0x0007),
new InstInfo(1032, "v_log_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Base2 log function.<br>D.f = log2(S0.f). ", @"", ISA_Enc.VOP1, 39, 1033, 0x7E004E00, 0x0005),
new InstInfo(1033, "v_log_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Base2 log function.<br>D.f = log2(S0.f). ", @"", ISA_Enc.VOP3a1, 423, 0, 0xD34E0000, 0x0007),
new InstInfo(1034, "v_log_legacy_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Return the algorithm of a 32-bit floating point value, using the same precision as Sea Islands.<br>D.f = log2(S0.f). Base 2 logarithm. Same as Sea Islands. ", @"", ISA_Enc.VOP1, 69, 0, 0x7E008A00, 0x0005),
new InstInfo(1035, "v_lshl_b32", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Scalar Logical Shift Left. Zero is shifted into the vacated locations. <br>D.u = S0.u << S1.u[4:0]. ", @"", ISA_Enc.VOP2, 25, 1036, 0x32000000, 0x0005),
new InstInfo(1036, "v_lshl_b32_ext", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Scalar Logical Shift Left. Zero is shifted into the vacated locations. <br>D.u = S0.u << S1.u[4:0]. ", @"", ISA_Enc.VOP3a2, 281, 0, 0xD2320000, 0x0007),
new InstInfo(1037, "v_lshl_b64", "v8b", "v8b", "v1u", "none", "none", "none", "none", 3, 3, @"D = S0.u << S1.u[4:0]. ", @"", ISA_Enc.VOP3a2, 353, 0, 0xD2C20000, 0x0007),
new InstInfo(1038, "v_lshlrev_b32", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"D.u = S1.u << S0.u[4:0].  ", @"", ISA_Enc.VOP2, 26, 1039, 0x34000000, 0x0005),
new InstInfo(1039, "v_lshlrev_b32_ext", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"D.u = S1.u << S0.u[4:0].  ", @"", ISA_Enc.VOP3a2, 282, 0, 0xD2340000, 0x0007),
new InstInfo(1040, "v_lshr_b32", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Scalar Logical Shift Right.  Zero is shifted into the vacated locations. <br>D.u = S0.u >> S1.u[4:0]. ", @"", ISA_Enc.VOP2, 21, 1041, 0x2A000000, 0x0005),
new InstInfo(1041, "v_lshr_b32_ext", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Scalar Logical Shift Right.  Zero is shifted into the vacated locations. <br>D.u = S0.u >> S1.u[4:0]. ", @"", ISA_Enc.VOP3a2, 277, 0, 0xD22A0000, 0x0007),
new InstInfo(1042, "v_lshr_b64", "v8b", "v8b", "v1u", "none", "none", "none", "none", 3, 3, @"D = S0.u >> S1.u[4:0]. ", @"", ISA_Enc.VOP3a2, 354, 0, 0xD2C40000, 0x0007),
new InstInfo(1043, "v_lshrrev_b32", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"D.u = S1.u >> S0.u[4:0].  ", @"", ISA_Enc.VOP2, 22, 1044, 0x2C000000, 0x0005),
new InstInfo(1044, "v_lshrrev_b32_ext", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"D.u = S1.u >> S0.u[4:0].  ", @"", ISA_Enc.VOP3a2, 278, 0, 0xD22C0000, 0x0007),
new InstInfo(1045, "v_mac_f32", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.f = S0.f * S1.f + D.f.  ", @"", ISA_Enc.VOP2, 31, 1046, 0x3E000000, 0x0005),
new InstInfo(1046, "v_mac_f32_ext", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.f = S0.f * S1.f + D.f.  ", @"", ISA_Enc.VOP3a2, 287, 0, 0xD23E0000, 0x0007),
new InstInfo(1047, "v_mac_legacy_f32", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.f = S0.F * S1.f + D.f. (Note that 'legacy' means that, unlike IEEE rules, 0 * anything = 0.) ", @"", ISA_Enc.VOP2, 6, 1048, 0x0C000000, 0x0005),
new InstInfo(1048, "v_mac_legacy_f32_ext", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.f = S0.F * S1.f + D.f. (Note that 'legacy' means that, unlike IEEE rules, 0 * anything = 0.) ", @"", ISA_Enc.VOP3a2, 262, 0, 0xD20C0000, 0x0007),
new InstInfo(1049, "v_mad_f32", "v4f", "v4f", "v4f", "v4f", "none", "none", "none", 4, 4, @"Floating point multiply-add (MAD). Gives same result as ADD after MUL_IEEE. Uses IEEE rules for 0*anything.<br>D.f = S0.f * S1.f + S2.f. ", @"", ISA_Enc.VOP3a3, 321, 0, 0xD2820000, 0x0007),
new InstInfo(1050, "v_mad_i32_i24", "v8i", "v3i", "v3i", "v4[iu]", "none", "none", "none", 4, 4, @"24-bit signed integer muladd.<br>S0 and S1 are treated as 24-bit signed integers. S2 is treated as a 32-bit signed or unsigned integer. Bits [31:24] are ignored. The result represents the low-order sign extended 32 bits of the multiply add result.<br>Result = Arg1.i[23:0] * Arg2.i[23:0] + Arg3.i[31:0] (low order bits). ", @"uint only works here if values are <2147483648(aka top bit must be 0). If larger is needed use U32 version. ", ISA_Enc.VOP3a3, 322, 0, 0xD2840000, 0x0007),
new InstInfo(1051, "v_mad_i64_i32", "v8i", "v4i", "v4i", "v8i", "none", "none", "none", 4, 4, @"Multiply add using the product of two 32-bit signed integers, then added to a 64-bit integer.<br>{vcc_out,D.i64} = S0.i32 * S1.i32 + S2.i64. ", @"", ISA_Enc.VOP3a3, 375, 0, 0xD2EE0000, 0x0027),
new InstInfo(1052, "v_mad_legacy_f32", "v8f", "v4f", "v4f", "v4f", "none", "none", "none", 4, 4, @"Floating-point multiply-add (MAD).  Gives same result as ADD after MUL.<br>D.f = S0.f * S1.f + S2.f (DX9 rules, 0.0*x = 0.0). ", @"", ISA_Enc.VOP3a3, 320, 0, 0xD2800000, 0x0007),
new InstInfo(1053, "v_mad_u32_u24", "v8u", "v3u", "v3u", "v4u", "none", "none", "none", 4, 4, @"24 bit unsigned integer muladd<br>Src a and b treated as 24 bit unsigned integers. Src c treated as 32 bit signed or unsigned integer. Bits [31:24] ignored. The result represents the low-order 32 bits of the multiply add result.<br>D.u = S0.u[23:0] * S1.u[23:0] + S2.u[31:0]. ", @"", ISA_Enc.VOP3a3, 323, 0, 0xD2860000, 0x0007),
new InstInfo(1054, "v_mad_u64_u32", "v8u", "v4u", "v4u", "v8u", "none", "none", "none", 4, 4, @"Multiply add using the product of two 32-bit unsigned integers, then added to a 64-bit integer.<br>{vcc_out,D.u64} = S0.u32 * S1.u32 + S2.u64. ", @"", ISA_Enc.VOP3a3, 374, 0, 0xD2EC0000, 0x0027),
new InstInfo(1055, "v_madak_f32", "v4f", "v4f", "v4f", "literal", "none", "none", "none", 4, 4, @"D.f = S0.f * S1.f + K; K is a 32-bit literal constant. ", @"", ISA_Enc.VOP2, 33, 0, 0x42000000, 0x0005),
new InstInfo(1056, "v_madmk_f32", "v4f", "v4f", "v4f", "literal", "none", "none", "none", 4, 4, @"D.f = S0.f * K + S1.f; K is a 32-bit literal constant. ", @"", ISA_Enc.VOP2, 32, 0, 0x40000000, 0x0005),
new InstInfo(1057, "v_max_f32", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"if (ieee_mode)<br>   if (S0.f==sNaN        result = quiet(S0.f);<br>   else if (S1.f==sNaN   result = quiet(S1.f);<br>   else if (S0.f==NaN)   result = S1.f;<br>   else if (S1.f==NaN)   result = S0.f;<br>   else if (S0.f>S1.f)   result = S0.f;<br>   else                  result = S1.f;<br>else<br>   else if (S0.f==NaN)   result = S1.f;<br>   else if (S1.f==NaN)   result = S0.f;<br>   else if (S0.f>=S1.f)  result = S0.f;<br>   else                  result = S1.f; ", @"", ISA_Enc.VOP2, 16, 1058, 0x20000000, 0x0005),
new InstInfo(1058, "v_max_f32_ext", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"if (ieee_mode)<br>   if (S0.f==sNaN        result = quiet(S0.f);<br>   else if (S1.f==sNaN   result = quiet(S1.f);<br>   else if (S0.f==NaN)   result = S1.f;<br>   else if (S1.f==NaN)   result = S0.f;<br>   else if (S0.f>S1.f)   result = S0.f;<br>   else                  result = S1.f;<br>else<br>   else if (S0.f==NaN)   result = S1.f;<br>   else if (S1.f==NaN)   result = S0.f;<br>   else if (S0.f>=S1.f)  result = S0.f;<br>   else                  result = S1.f; ", @"", ISA_Enc.VOP3a2, 272, 0, 0xD2200000, 0x0007),
new InstInfo(1059, "v_max_f64", "v8f", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"The instruction specifies which one of two data elements in a four-element vector is operated on (the two dwords of a double precision floating point number), and the result can be stored in the wz or yx elements of the destination GPR.<br>D.d = max(S0.d, S1.d).<br>if (src0 > src1)<br>   dst = src0;<br>else<br>   dst = src1;<br>max(-0,+0)=max(+0,-0)=+0", @"", ISA_Enc.VOP3a2, 359, 0, 0xD2CE0000, 0x0007),
new InstInfo(1060, "v_max_i32", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"Integer maximum based on signed integer components.<br>D.i = max(S0.i, S1.i). ", @"", ISA_Enc.VOP2, 18, 1061, 0x24000000, 0x0005),
new InstInfo(1061, "v_max_i32_ext", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"Integer maximum based on signed integer components.<br>D.i = max(S0.i, S1.i). ", @"", ISA_Enc.VOP3a2, 274, 0, 0xD2240000, 0x0007),
new InstInfo(1062, "v_max_legacy_f32", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"Floating-point maximum. <br>If (S0.f >= S1.f) <br>   D.f = S0.f; <br>Else <br>   D.f = S1.f;<br>D.f = max(S0.f, S1.f) (DX9 rules for NaN). ", @"", ISA_Enc.VOP2, 14, 1063, 0x1C000000, 0x0005),
new InstInfo(1063, "v_max_legacy_f32_ext", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"Floating-point maximum. <br>If (S0.f >= S1.f) <br>   D.f = S0.f; <br>Else <br>   D.f = S1.f;<br>D.f = max(S0.f, S1.f) (DX9 rules for NaN). ", @"", ISA_Enc.VOP3a2, 270, 0, 0xD21C0000, 0x0007),
new InstInfo(1064, "v_max_u32", "v4u", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"Integer maximum based on unsigned integer components.<br>If (S0.u >= S1.u) <br>   D.u = S0.u; <br>Else <br>   D.u = S1.u; ", @"", ISA_Enc.VOP2, 20, 1065, 0x28000000, 0x0005),
new InstInfo(1065, "v_max_u32_ext", "v4u", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"Integer maximum based on unsigned integer components.<br>If (S0.u >= S1.u) <br>   D.u = S0.u; <br>Else <br>   D.u = S1.u; ", @"", ISA_Enc.VOP3a2, 276, 0, 0xD2280000, 0x0007),
new InstInfo(1066, "v_max3_f32", "v4f", "v4f", "v4f", "v4f", "none", "none", "none", 4, 4, @"Maximum of three numbers. DX10 NaN handland and flag creation.<br>D.f = max(S0.f, S1.f, S2.f). ", @"", ISA_Enc.VOP3a3, 340, 0, 0xD2A80000, 0x0007),
new InstInfo(1067, "v_max3_i32", "v4i", "v4i", "v4i", "v4i", "none", "none", "none", 4, 4, @"Maximum of three numbers.<br>D.i = max(S0.i, S1.i, S2.i). ", @"", ISA_Enc.VOP3a3, 341, 0, 0xD2AA0000, 0x0007),
new InstInfo(1068, "v_max3_u32", "v4u", "v4u", "v4u", "v4u", "none", "none", "none", 4, 4, @"Maximum of three numbers.<br>D.u = max(S0.u, S1.u, S2.u). ", @"", ISA_Enc.VOP3a3, 342, 0, 0xD2AC0000, 0x0007),
new InstInfo(1069, "v_mbcnt_hi_u32_b32", "v4u", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Masked bit count of the upper 32 threads (threads 32-63). For each thread, this instruction returns the number of active threads which come before it.<br>ThreadMask = (1 << ThreadPosition) - 1;<br>D.u = CountOneBits(S0.u & ThreadMask[63:32]) + S1.u. ", @"", ISA_Enc.VOP2, 36, 1070, 0x48000000, 0x0005),
new InstInfo(1070, "v_mbcnt_hi_u32_b32_ext", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Masked bit count of the upper 32 threads (threads 32-63). For each thread, this instruction returns the number of active threads which come before it.<br>ThreadMask = (1 << ThreadPosition) - 1;<br>D.u = CountOneBits(S0.u & ThreadMask[63:32]) + S1.u. ", @"", ISA_Enc.VOP3a2, 292, 0, 0xD2480000, 0x0007),
new InstInfo(1071, "v_mbcnt_lo_u32_b32", "v4u", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Masked bit count set 32 low. ThreadPosition is the position of this thread in the wavefront (in 0..63).<br>ThreadMask = (1 << ThreadPosition) - 1; <br>D.u = CountOneBits(S0.u & ThreadMask[31:0]) + S1.u.  ", @"", ISA_Enc.VOP2, 35, 1072, 0x46000000, 0x0005),
new InstInfo(1072, "v_mbcnt_lo_u32_b32_ext", "v4b", "v4b", "v4u", "none", "none", "none", "none", 3, 3, @"Masked bit count set 32 low. ThreadPosition is the position of this thread in the wavefront (in 0..63).<br>ThreadMask = (1 << ThreadPosition) - 1; <br>D.u = CountOneBits(S0.u & ThreadMask[31:0]) + S1.u.  ", @"", ISA_Enc.VOP3a2, 291, 0, 0xD2460000, 0x0007),
new InstInfo(1073, "v_med3_f32", "v4f", "v4f", "v4f", "v4f", "none", "none", "none", 4, 4, @"Median of three numbers. DX10 NaN handling and flag creation.<br>If (isNan(S0.f) || isNan(S1.f) || isNan(S2.f))<br>   D.f = MIN3(S0.f, S1.f, S2.f)<br>Else if (MAX3(S0.f,S1.f,S2.f) == S0.f)<br>   D.f = MAX(S1.f, S2.f)<br>Else if (MAX3(S0.f,S1.f,S2.f) == S1.f)<br>   D.f = MAX(S0.f, S2.f)<br>Else<br>   D.f = MAX(S0.f, S1.f) ", @"", ISA_Enc.VOP3a3, 343, 0, 0xD2AE0000, 0x0007),
new InstInfo(1074, "v_med3_i32", "v4i", "v4i", "v4i", "v4i", "none", "none", "none", 4, 4, @"Median of three numbers. <br>If (isNan(S0.f) || isNan(S1.f) || isNan(S2.f))<br>   D.f = MIN3(S0.f, S1.f, S2.f)<br>Else if (MAX3(S0.f,S1.f,S2.f) == S0.f)<br>   D.f = MAX(S1.f, S2.f)<br>Else if (MAX3(S0.f,S1.f,S2.f) == S1.f)<br>   D.f = MAX(S0.f, S2.f)<br>Else<br>   D.f = MAX(S0.f, S1.f) ", @"", ISA_Enc.VOP3a3, 344, 0, 0xD2B00000, 0x0007),
new InstInfo(1075, "v_med3_u32", "v4u", "v4u", "v4u", "v4u", "none", "none", "none", 4, 4, @"Median of three numbers. <br>If (isNan(S0.f) || isNan(S1.f) || isNan(S2.f))<br>  D.f = MIN3(S0.f, S1.f, S2.f)<br>Else if (MAX3(S0.f,S1.f,S2.f) == S0.f)<br>  D.f = MAX(S1.f, S2.f)<br>Else if (MAX3(S0.f,S1.f,S2.f) == S1.f)<br>  D.f = MAX(S0.f, S2.f)<br>Else<br>  D.f = MAX(S0.f, S1.f) ", @"", ISA_Enc.VOP3a3, 345, 0, 0xD2B20000, 0x0007),
new InstInfo(1076, "v_min_f32", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"if (ieee_mode){<br>   if (S0.f==sNaN)         result = quiet(S0.f);<br>   else if (S1.f==sNaN)    result = quiet(S1.f);<br>   else if (S0.f==NaN)    result = S1.f;<br>   else if (S1.f==NaN)    result = S0.f;<br>   else if (S0.f<S1.f)    result = S0.f;<br>   else                   result = S1.f;}<br>else{<br>   if (S0.f==NaN)    result = S1.f;<br>   else if (S1.f==NaN)    result = S0.f;<br>   else if (S0.f<S1.f)    result = S0.f;<br>   else                   result = S1.f;}", @"", ISA_Enc.VOP2, 15, 1077, 0x1E000000, 0x0005),
new InstInfo(1077, "v_min_f32_ext", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"if (ieee_mode){<br>   if (S0.f==sNaN)         result = quiet(S0.f);<br>   else if (S1.f==sNaN)    result = quiet(S1.f);<br>   else if (S0.f==NaN)    result = S1.f;<br>   else if (S1.f==NaN)    result = S0.f;<br>   else if (S0.f<S1.f)    result = S0.f;<br>   else                   result = S1.f;}<br>else{<br>   if (S0.f==NaN)    result = S1.f;<br>   else if (S1.f==NaN)    result = S0.f;<br>   else if (S0.f<S1.f)    result = S0.f;<br>   else                   result = S1.f;}", @"", ISA_Enc.VOP3a2, 271, 0, 0xD21E0000, 0x0007),
new InstInfo(1078, "v_min_f64", "v8f", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"Double precision floating point minimum.<br>The instruction specifies which one of two data elements in a four-element vector is operated on (the two dwords of a double precision floating point number), and the result can be stored in the wz or yx elements of the destination GPR.<br>DX10 implies slightly different handling of Nan's.  See the SP Numeric spec for details. <br>Double result written to two consecutive GPRs; the instruction Dest specifies the lesser of the two.<br>if (src0 < src1)<br>   dst = src0;<br>else<br>   dst = src1;<br>min(-0,+0)=min(+0,-0)=-0<br>D.d = min(S0.d, S1.d). ", @"", ISA_Enc.VOP3a2, 358, 0, 0xD2CC0000, 0x0007),
new InstInfo(1079, "v_min_i32", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"Integer minimum based on signed integer components.<br>If (S0.i < S1.i) <br>   D.i = S0.i; <br>Else <br>   D.i = S1.i; ", @"", ISA_Enc.VOP2, 17, 1080, 0x22000000, 0x0005),
new InstInfo(1080, "v_min_i32_ext", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"Integer minimum based on signed integer components.<br>If (S0.i < S1.i) <br>   D.i = S0.i; <br>Else <br>   D.i = S1.i; ", @"", ISA_Enc.VOP3a2, 273, 0, 0xD2220000, 0x0007),
new InstInfo(1081, "v_min_legacy_f32", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"Floating-point minimum.<br>If (S0.f < S1.f) <br>   D.f = S0.f; <br>Else <br>   D.f = S1.f;<br>D.f = min(S0.f, S1.f) (DX9 rules for NaN). ", @"", ISA_Enc.VOP2, 13, 1082, 0x1A000000, 0x0005),
new InstInfo(1082, "v_min_legacy_f32_ext", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"Floating-point minimum.<br>If (S0.f < S1.f) <br>   D.f = S0.f; <br>Else <br>   D.f = S1.f;<br>D.f = min(S0.f, S1.f) (DX9 rules for NaN). ", @"", ISA_Enc.VOP3a2, 269, 0, 0xD21A0000, 0x0007),
new InstInfo(1083, "v_min_u32", "v4u", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"Integer minimum based on signed unsigned integer components.<br>If (S0.u < S1.u) <br>   D.u = S0.u; <br>Else <br>   D.u = S1.u; ", @"", ISA_Enc.VOP2, 19, 1084, 0x26000000, 0x0005),
new InstInfo(1084, "v_min_u32_ext", "v4u", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"Integer minimum based on signed unsigned integer components.<br>If (S0.u < S1.u) <br>   D.u = S0.u; <br>Else <br>   D.u = S1.u; ", @"", ISA_Enc.VOP3a2, 275, 0, 0xD2260000, 0x0007),
new InstInfo(1085, "v_min3_f32", "v4f", "v4f", "v4f", "v4f", "none", "none", "none", 4, 4, @"Minimum of three numbers. DX10 NaN handling and flag creation.<br>D.f = min(S0.f, S1.f, S2.f). ", @"", ISA_Enc.VOP3a3, 337, 0, 0xD2A20000, 0x0007),
new InstInfo(1086, "v_min3_i32", "v4i", "v4i", "v4i", "v4i", "none", "none", "none", 4, 4, @"Minimum of three numbers.<br>D.i = min(S0.i, S1.i, S2.i). ", @"", ISA_Enc.VOP3a3, 338, 0, 0xD2A40000, 0x0007),
new InstInfo(1087, "v_min3_u32", "v4u", "v4u", "v4u", "v4u", "none", "none", "none", 4, 4, @"Minimum of three numbers.<br>D.u = min(S0.u, S1.u, S2.u). ", @"", ISA_Enc.VOP3a3, 339, 0, 0xD2A60000, 0x0007),
new InstInfo(1088, "v_mov_b32", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"Single operand move instruction. Allows denorms in and out, regardless of denorm mode, in both single and double precision designs.<br>D.u = S0.u. ", @"", ISA_Enc.VOP1, 1, 1089, 0x7E000200, 0x0005),
new InstInfo(1089, "v_mov_b32_ext", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"Single operand move instruction. Allows denorms in and out, regardless of denorm mode, in both single and double precision designs.<br>D.u = S0.u. ", @"", ISA_Enc.VOP3a1, 385, 0, 0xD3020000, 0x0007),
new InstInfo(1090, "v_mov_fed_b32", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"D.u = S0.u, introduce edc double error upon write to dest VGPR without causing an exception.", @"", ISA_Enc.VOP1, 9, 0, 0x7E001200, 0x0005),
new InstInfo(1091, "v_mov_fed_b32_ext", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"D.u = S0.u, introduce edc double error upon write to dest VGPR without causing an exception.", @"", ISA_Enc.VOP3a1, 393, 0, 0XD3120000, 0x0007),
new InstInfo(1092, "v_movreld_b32", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"VGPR[D.u + M0.u] = VGPR[S0.u]. ", @"", ISA_Enc.VOP1, 66, 0, 0x7E008400, 0x0005),
new InstInfo(1093, "v_movreld_b32_ext", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"VGPR[D.u + M0.u] = VGPR[S0.u]. ", @"", ISA_Enc.VOP3a1, 450, 0, 0xD3840000, 0x0007),
new InstInfo(1094, "v_movrels_b32", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"VGPR[D.u] = VGPR[S0.u + M0.u]. ", @"", ISA_Enc.VOP1, 67, 0, 0x7E008600, 0x0005),
new InstInfo(1095, "v_movrels_b32_ext", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"VGPR[D.u] = VGPR[S0.u + M0.u]. ", @"", ISA_Enc.VOP3a1, 451, 0, 0xD3860000, 0x0007),
new InstInfo(1096, "v_movrelsd_b32", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"VGPR[D.u + M0.u] = VGPR[S0.u + M0.u]. ", @"", ISA_Enc.VOP1, 68, 0, 0x7E008800, 0x0005),
new InstInfo(1097, "v_movrelsd_b32_ext", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"VGPR[D.u + M0.u] = VGPR[S0.u + M0.u]. ", @"", ISA_Enc.VOP3a1, 452, 0, 0xD3880000, 0x0007),
new InstInfo(1098, "v_mqsad_pk_u16_u8", "v8u", "v8u", "v4u", "v8u", "none", "none", "none", 4, 4, @"D.u = Masked Quad-Byte SAD with accum_lo/hi(S0.u[63:0], S1.u[31:0], S2.u[63:0]). ", @"", ISA_Enc.VOP3a3, 371, 0, 0xD2E60000, 0x0007),
new InstInfo(1099, "v_mqsad_u32_u8", "v8u", "v4u", "v16u", "none", "none", "none", "none", 3, 3, @"Masked quad sum-of-absolute-difference.<br>D.u128 = Masked Quad-Byte SAD with 32-bit accum_lo/hi(S0.u[63:0], S1.u[31:0], S2.u[127:0]) ", @"", ISA_Enc.VOP3a2, 373, 0, 0xD2EA0000, 0x0007),
new InstInfo(1100, "v_msad_u8", "v1u", "v1u", "v1u", "v1u", "none", "none", "none", 4, 4, @"D.u = Masked Byte SAD with accum_lo(S0.u, S1.u, S2.u). ", @"", ISA_Enc.VOP3a3, 369, 0, 0xD2E20000, 0x0007),
new InstInfo(1101, "v_mul_f32", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"Floating point multiply.  Uses IEEE rules for 0*anything.<br>D.f = S0.f * S1.f. ", @"", ISA_Enc.VOP2, 8, 0, 0x10000000, 0x0005),
new InstInfo(1102, "v_mul_f32_ext", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"Floating point multiply.  Uses IEEE rules for 0*anything.<br>D.f = S0.f * S1.f. ", @"", ISA_Enc.VOP3a2, 264, 0, 0xD2100000, 0x0007),
new InstInfo(1103, "v_mul_f64", "v8f", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"Floating-point 64-bit multiply. Multiplies a double-precision value in src0.YX by a double-precision value in src1.YX, and places the lower 64 bits of the result in dst.YX. Inputs are from two consecutive GPRs, with the instruction specifying the lesser of the two; the double result is written to two consecutive GPRs.<br>dst = src0 * src1;<br>D.d = S0.d * S1.d. <br>(A * B) == (B * A) <br>Coissue: The V_MUL_F64 instruction is a four-slot instruction. Therefore, a single V_MUL_F64 instruction can be issued in slots 0, 1, 2, and 3. Slot 4 can contain any other valid instruction.  ", @"", ISA_Enc.VOP3a2, 357, 0, 0xD2CA0000, 0x0007),
new InstInfo(1104, "v_mul_hi_i32", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"Signed integer multiplication. The result represents the high-order 32 bits of the multiply result.<br>D.i = (S0.i * S1.i)>>32. ", @"", ISA_Enc.VOP3a2, 364, 0, 0xD2D80000, 0x0007),
new InstInfo(1105, "v_mul_hi_i32_i24", "v4i", "v3i", "v3i", "none", "none", "none", "none", 3, 3, @"24-bit signed integer multiply.<br>S0 and S1 are treated as 24-bit signed integers. Bits [31:24] are ignored. The result represents the high-order 16 bits of the 48-bit multiply result, sign extended to 32 bits:<br>D.i = (S0.i[23:0] * S1.i[23:0])>>32. ", @"", ISA_Enc.VOP2, 10, 0, 0x14000000, 0x0005),
new InstInfo(1106, "v_mul_hi_i32_i24_ext", "v4i", "v3i", "v3i", "none", "none", "none", "none", 3, 3, @"24-bit signed integer multiply.<br>S0 and S1 are treated as 24-bit signed integers. Bits [31:24] are ignored. The result represents the high-order 16 bits of the 48-bit multiply result, sign extended to 32 bits:<br>D.i = (S0.i[23:0] * S1.i[23:0])>>32. ", @"", ISA_Enc.VOP3a2, 266, 0, 0xD2140000, 0x0007),
new InstInfo(1107, "v_mul_hi_u32", "v4u", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"Unsigned integer multiplication. The result represents the high-order 32 bits of the multiply result.<br>D.u = (S0.u * S1.u)>>32. ", @"", ISA_Enc.VOP3a2, 362, 0, 0xD2D40000, 0x0007),
new InstInfo(1108, "v_mul_hi_u32_u24", "v4u", "v3u", "v3u", "none", "none", "none", "none", 3, 3, @"24-bit unsigned integer multiply.<br>S0 and S1 are treated as 24-bit unsigned integers. Bits [31:24]are ignored. The result represents the high-order 16 bits of the 48-bit multiply result: {16'b0, mul_result[47:32]}.<br>D.i = (S0.u[23:0] * S1.u[23:0])>>32. ", @"", ISA_Enc.VOP2, 12, 0, 0x18000000, 0x0005),
new InstInfo(1109, "v_mul_hi_u32_u24_ext", "v4u", "v3u", "v3u", "none", "none", "none", "none", 3, 3, @"24-bit unsigned integer multiply.<br>S0 and S1 are treated as 24-bit unsigned integers. Bits [31:24]are ignored. The result represents the high-order 16 bits of the 48-bit multiply result: {16'b0, mul_result[47:32]}.<br>D.i = (S0.u[23:0] * S1.u[23:0])>>32. ", @"", ISA_Enc.VOP3a2, 268, 0, 0xD2180000, 0x0007),
new InstInfo(1110, "v_mul_i32_i24", "v4i", "v3i", "v3i", "none", "none", "none", "none", 3, 3, @"24 bit signed integer multiply<br>Src a and b treated as 24 bit signed integers. Bits [31:24] ignored. The result represents the low-order 32 bits of the 48 bit multiply result: mul_result[31:0].<br>D.i = S0.i[23:0] * S1.i[23:0]. ", @"", ISA_Enc.VOP2, 9, 0, 0x12000000, 0x0005),
new InstInfo(1111, "v_mul_i32_i24_ext", "v4i", "v3i", "v3i", "none", "none", "none", "none", 3, 3, @"24 bit signed integer multiply<br>Src a and b treated as 24 bit signed integers. Bits [31:24] ignored. The result represents the low-order 32 bits of the 48 bit multiply result: mul_result[31:0].<br>D.i = S0.i[23:0] * S1.i[23:0]. ", @"", ISA_Enc.VOP3a2, 265, 0, 0xD2120000, 0x0007),
new InstInfo(1112, "v_mul_legacy_f32", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"Floating-point multiply.<br>D.f = S0.f * S1.f (DX9 rules, 0.0*x = 0.0). ", @"", ISA_Enc.VOP2, 7, 0, 0x0E000000, 0x0005),
new InstInfo(1113, "v_mul_legacy_f32_ext", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"Floating-point multiply.<br>D.f = S0.f * S1.f (DX9 rules, 0.0*x = 0.0). ", @"", ISA_Enc.VOP3a2, 263, 0, 0xD20E0000, 0x0007),
new InstInfo(1114, "v_mul_lo_i32", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"Signed integer multiplication. The result represents the low-order 32 bits of the multiply result.<br>D.i = S0.i * S1.i. ", @"", ISA_Enc.VOP3a2, 363, 0, 0xD2D60000, 0x0007),
new InstInfo(1115, "v_mul_lo_u32", "v4u", "v4u", "v4u", "none", "none", "none", "none", 3, 3, @"Unsigned integer multiplication. The result represents the low-order 32 bits of the multiply result.<br>D.u = S0.u * S1.u. ", @"", ISA_Enc.VOP3a2, 361, 0, 0xD2D20000, 0x0007),
new InstInfo(1116, "v_mul_u32_u24", "v4u", "v3u", "v3u", "none", "none", "none", "none", 3, 3, @"24-bit unsigned integer multiply.<br>S0 and S1 are treated as 24-bit unsigned integers. Bits [31:24] are ignored. The result represents the low-order 32 bits of the 48-bit multiply result: mul_result[31:0].<br>D.u = S0.u[23:0] * S1.u[23:0]. ", @"", ISA_Enc.VOP2, 11, 0, 0x16000000, 0x0005),
new InstInfo(1117, "v_mul_u32_u24_ext", "v4u", "v3u", "v3u", "none", "none", "none", "none", 3, 3, @"24-bit unsigned integer multiply.<br>S0 and S1 are treated as 24-bit unsigned integers. Bits [31:24] are ignored. The result represents the low-order 32 bits of the 48-bit multiply result: mul_result[31:0].<br>D.u = S0.u[23:0] * S1.u[23:0]. ", @"", ISA_Enc.VOP3a2, 267, 0, 0xD2160000, 0x0007),
new InstInfo(1118, "v_mullit_f32", "v4f", "v4f", "v4f", "v4b", "none", "none", "none", 4, 4, @"Scalar multiply (2) with result replicated in all four channels.<br>It is used when emulating LIT instruction. 0*anything = 0.<br>Note this instruction takes three inputs.<br>D.f = S0.f * S1.f, replicate result into 4 components (0.0 * x = 0.0; special INF, NaN, overflow rules). ", @"", ISA_Enc.VOP3a3, 336, 0, 0xD2A00000, 0x0007),
new InstInfo(1119, "v_nop", "none", "none", "none", "none", "none", "none", "none", 0, 0, @"Do nothing. ", @"", ISA_Enc.VOP1, 0, 0, 0x7E000000, 0x0005),
new InstInfo(1120, "v_nop_ext", "none", "none", "none", "none", "none", "none", "none", 0, 0, @"Do nothing. ", @"", ISA_Enc.VOP3a0, 384, 0, 0xD3000000, 0x0007),
new InstInfo(1121, "v_not_b32", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"Logical bit-wise NOT.<br>D.u = ~S0.u. ", @"", ISA_Enc.VOP1, 55, 0, 0x7E006E00, 0x0005),
new InstInfo(1122, "v_not_b32_ext", "v4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"Logical bit-wise NOT.<br>D.u = ~S0.u. ", @"", ISA_Enc.VOP3a1, 439, 0, 0xD36E0000, 0x0007),
new InstInfo(1123, "v_or_b32", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Logical bit-wise OR.<br>D.u = S0.u | S1.u. ", @"", ISA_Enc.VOP2, 28, 0, 0x38000000, 0x0005),
new InstInfo(1124, "v_or_b32_ext", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Logical bit-wise OR.<br>D.u = S0.u | S1.u. ", @"", ISA_Enc.VOP3a2, 284, 0, 0xD2380000, 0x0007),
new InstInfo(1125, "v_qsad_pk_u16_u8", "v8u", "v8u", "v8u", "v8u", "none", "none", "none", 4, 4, @"D.u = Quad-Byte SAD with accum_lo/hiu(S0.u[63:0], S1.u[31:0], S2.u[63:0]). ", @"", ISA_Enc.VOP3a3, 370, 0, 0xD2E40000, 0x0007),
new InstInfo(1126, "v_rcp_clamp_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Reciprocal, < 1 ulp error.<br>The clamp prevents infinite results, clamping infinities to max_float.<br>This reciprocal approximation converges to < 0.5 ulp error with one newton rhapson performed with two fused multiple adds (FMAs).<br>D.f = 1.0 / S0.f, result clamped to +-max_float. ", @"", ISA_Enc.VOP1, 40, 0, 0x7E005000, 0x0005),
new InstInfo(1127, "v_rcp_clamp_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Reciprocal, < 1 ulp error.<br>The clamp prevents infinite results, clamping infinities to max_float.<br>This reciprocal approximation converges to < 0.5 ulp error with one newton rhapson performed with two fused multiple adds (FMAs).<br>D.f = 1.0 / S0.f, result clamped to +-max_float. ", @"", ISA_Enc.VOP3a1, 424, 0, 0xD3500000, 0x0007),
new InstInfo(1128, "v_rcp_clamp_f64", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Double reciprocal.<br>The clamp prevents infinite results, clamping infinities to max_float. Inputs from two consecutive GPRs, instruction source specifies less of the two.<br>Double result are written to two consecutive GPRs, instruction Dest specifies the lesser of the two.<br>D.f = 1.0 / (S0.f), result clamped to +-max_float. ", @"", ISA_Enc.VOP1, 48, 0, 0x7E006000, 0x0005),
new InstInfo(1129, "v_rcp_clamp_f64_ext", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Double reciprocal.<br>The clamp prevents infinite results, clamping infinities to max_float. Inputs from two consecutive GPRs, instruction source specifies less of the two.<br>Double result are written to two consecutive GPRs, instruction Dest specifies the lesser of the two.<br>D.f = 1.0 / (S0.f), result clamped to +-max_float. ", @"", ISA_Enc.VOP3a1, 432, 0, 0xD3600000, 0x0007),
new InstInfo(1130, "v_rcp_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Reciprocal, < 1 ulp error.<br>This reciprocal approximation converges to < 0.5 ulp error with one newton rhapson performed with two fused multiple adds (FMAs).<br>D.f = 1.0 / S0.f. ", @"", ISA_Enc.VOP1, 42, 0, 0x7E005400, 0x0005),
new InstInfo(1131, "v_rcp_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Reciprocal, < 1 ulp error.<br>This reciprocal approximation converges to < 0.5 ulp error with one newton rhapson performed with two fused multiple adds (FMAs).<br>D.f = 1.0 / S0.f. ", @"", ISA_Enc.VOP3a1, 426, 0, 0xD3540000, 0x0007),
new InstInfo(1132, "v_rcp_f64", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Double reciprocal.<br>Inputs from two consecutive GPRs, the instruction source specifies less of the two. Double result written to two consecutive GPRs; the instruction Dest specifies the lesser of the two.<br>D.d = 1.0 / (S0.d). ", @"", ISA_Enc.VOP1, 47, 0, 0x7E005E00, 0x0005),
new InstInfo(1133, "v_rcp_f64_ext", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Double reciprocal.<br>Inputs from two consecutive GPRs, the instruction source specifies less of the two. Double result written to two consecutive GPRs; the instruction Dest specifies the lesser of the two.<br>D.d = 1.0 / (S0.d). ", @"", ISA_Enc.VOP3a1, 431, 0, 0xD35E0000, 0x0007),
new InstInfo(1134, "v_rcp_iflag_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Reciprocal.<br>Signals exceptions using integer divide by zero flag only; does not trigger any floating point exceptions. To be used in an integer reciprocal macro by the compiler.<br>D.f = 1.0 / S0.f, only integer div_by_zero flag can be raised. ", @"", ISA_Enc.VOP1, 43, 0, 0x7E005600, 0x0005),
new InstInfo(1135, "v_rcp_iflag_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Reciprocal.<br>Signals exceptions using integer divide by zero flag only; does not trigger any floating point exceptions. To be used in an integer reciprocal macro by the compiler.<br>D.f = 1.0 / S0.f, only integer div_by_zero flag can be raised. ", @"", ISA_Enc.VOP3a1, 427, 0, 0xD3560000, 0x0007),
new InstInfo(1136, "v_rcp_legacy_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Reciprocal, < 1 ulp error.<br>Legacy refers to the behavior that rcp_legacy(+/-0)=+0.<br>This reciprocal approximation converges to < 0.5 ulp error with one newton rhapson performed with two fused multiple adds (FMAs).<br>If (Arg1 == 1.0f)<br>   Result = 1.0f;<br>Else If (Arg1 == 0.0f)<br>   Result = 0.0f;<br>Else<br>   Result = RECIP_IEEE(Arg1); <br>// clamp result<br>if (Result == -INFINITY)<br>   Result = -ZERO;<br>if (Result == +INFINITY)<br>   Result = +ZERO;", @"", ISA_Enc.VOP1, 41, 0, 0x7E005200, 0x0005),
new InstInfo(1137, "v_rcp_legacy_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Reciprocal, < 1 ulp error.<br>Legacy refers to the behavior that rcp_legacy(+/-0)=+0.<br>This reciprocal approximation converges to < 0.5 ulp error with one newton rhapson performed with two fused multiple adds (FMAs).<br>If (Arg1 == 1.0f)<br>   Result = 1.0f;<br>Else If (Arg1 == 0.0f)<br>   Result = 0.0f;<br>Else<br>   Result = RECIP_IEEE(Arg1); <br>// clamp result<br>if (Result == -INFINITY)<br>   Result = -ZERO;<br>if (Result == +INFINITY)<br>   Result = +ZERO;", @"", ISA_Enc.VOP3a1, 425, 0, 0xD3520000, 0x0007),
new InstInfo(1138, "v_readfirstlane_b32", "s4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"copy one VGPR value to one SGPR.  Dst = SGPR-dest, Src0 = Source Data (VGPR# or M0(lds-direct)), Lane# = FindFirst1fromLSB(exec) (lane = 0 if exec is zero).  Ignores exec mask.  ", @"", ISA_Enc.VOP1, 2, 0, 0x7E000400, 0x0001),
new InstInfo(1139, "v_readfirstlane_b32_ext", "s4b", "v4b", "none", "none", "none", "none", "none", 2, 2, @"copy one VGPR value to one SGPR.  Dst = SGPR-dest, Src0 = Source Data (VGPR# or M0(lds-direct)), Lane# = FindFirst1fromLSB(exec) (lane = 0 if exec is zero).  Ignores exec mask.  ", @"", ISA_Enc.VOP3a1, 386, 0, 0xD3040000, 0x0013),
new InstInfo(1140, "v_readlane_b32", "s4b", "v4b", "v1u", "none", "none", "none", "none", 3, 3, @"Copy one VGPR value to one SGPR.  Dst = SGPR-dest, Src0 = Source Data (VGPR# or M0(lds-direct)), Src1 = Lane Select (SGPR or M0).  Ignores exec mask. A lane corresponds to one thread in a wavefront.  ", @"", ISA_Enc.VOP2, 1, 0, 0x02000000, 0x0001),
new InstInfo(1141, "v_readlane_b32_ext", "s4b", "v4b", "v1u", "none", "none", "none", "none", 3, 3, @"Copy one VGPR value to one SGPR.  Dst = SGPR-dest, Src0 = Source Data (VGPR# or M0(lds-direct)), Src1 = Lane Select (SGPR or M0).  Ignores exec mask. A lane corresponds to one thread in a wavefront.  ", @"", ISA_Enc.VOP3a2, 257, 0, 0xD2020000, 0x0003),
new InstInfo(1142, "v_rndne_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Floating-point Round-to-Nearest-Even Integer.<br>D.f = round_nearest_even(S0.f). ", @"", ISA_Enc.VOP1, 35, 0, 0x7E004600, 0x0005),
new InstInfo(1143, "v_rndne_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Floating-point Round-to-Nearest-Even Integer.<br>D.f = round_nearest_even(S0.f). ", @"", ISA_Enc.VOP3a1, 419, 0, 0xD3460000, 0x0007),
new InstInfo(1144, "v_rndne_f64", "v2d", "v8f", "none", "none", "none", "none", "none", 2, 2, @"64-bit floating-point round-to-nearest-even.<br>D.d = round_nearest_even(S0.d). ", @"", ISA_Enc.VOP1, 25, 0, 0x7E003200, 0x0005),
new InstInfo(1145, "v_rsq_clamp_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Reciprocal square root.<br>The clamp prevents infinite results, clamping infinities to max_float.<br>D.f = 1.0 / sqrt(S0.f), result clamped to +-max_float. ", @"", ISA_Enc.VOP1, 44, 0, 0x7E005800, 0x0005),
new InstInfo(1146, "v_rsq_clamp_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Reciprocal square root.<br>The clamp prevents infinite results, clamping infinities to max_float.<br>D.f = 1.0 / sqrt(S0.f), result clamped to +-max_float. ", @"", ISA_Enc.VOP3a1, 428, 0, 0xD3580000, 0x0007),
new InstInfo(1147, "v_rsq_clamp_f64", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Double reciprocal square root.<br>The clamp prevents infinite results, clamping infinities to max_float. Inputs from two consecutive GPRs, the instruction source specifies the lesser of the two. Double result written to two consecutive GPRs, the instruction Dest specifies the lesser of the two.<br>D.d = 1.0 / sqrt(S0.d), result clamped to +-max_float. ", @"", ISA_Enc.VOP1, 50, 0, 0x7E006400, 0x0005),
new InstInfo(1148, "v_rsq_clamp_f64_ext", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Double reciprocal square root.<br>The clamp prevents infinite results, clamping infinities to max_float. Inputs from two consecutive GPRs, the instruction source specifies the lesser of the two. Double result written to two consecutive GPRs, the instruction Dest specifies the lesser of the two.<br>D.d = 1.0 / sqrt(S0.d), result clamped to +-max_float. ", @"", ISA_Enc.VOP3a1, 434, 0, 0xD3640000, 0x0007),
new InstInfo(1149, "v_rsq_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Reciprocal square roots.<br>D.f = 1.0 / sqrt(S0.f). ", @"", ISA_Enc.VOP1, 46, 0, 0x7E005C00, 0x0005),
new InstInfo(1150, "v_rsq_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Reciprocal square roots.<br>D.f = 1.0 / sqrt(S0.f). ", @"", ISA_Enc.VOP3a1, 430, 0, 0xD35C0000, 0x0007),
new InstInfo(1151, "v_rsq_f64", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Double reciprocal square root.<br>Inputs from two consecutive GPRs; the instruction source specifies the lesser of the two. The double result is written to two consecutive GPRs; the instruction Dest specifies the lesser of the two.<br>D.f = 1.0 / sqrt(S0.f). ", @"", ISA_Enc.VOP1, 49, 0, 0x7E006200, 0x0005),
new InstInfo(1152, "v_rsq_f64_ext", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Double reciprocal square root.<br>Inputs from two consecutive GPRs; the instruction source specifies the lesser of the two. The double result is written to two consecutive GPRs; the instruction Dest specifies the lesser of the two.<br>D.f = 1.0 / sqrt(S0.f). ", @"", ISA_Enc.VOP3a1, 433, 0, 0xD3620000, 0x0007),
new InstInfo(1153, "v_rsq_legacy_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Reciprocal square root.<br>Legacy refers to the behavior that rsq_legacy(+/-0)=+0.<br>The clamp prevents infinite results, clamping infinities to max_float.<br>D.f = 1.0 / sqrt(S0.f). ", @"", ISA_Enc.VOP1, 45, 0, 0x7E005A00, 0x0005),
new InstInfo(1154, "v_rsq_legacy_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Reciprocal square root.<br>Legacy refers to the behavior that rsq_legacy(+/-0)=+0.<br>The clamp prevents infinite results, clamping infinities to max_float.<br>D.f = 1.0 / sqrt(S0.f). ", @"", ISA_Enc.VOP3a1, 429, 0, 0xD35A0000, 0x0007),
new InstInfo(1155, "v_sad_hi_u8", "v1u", "v1u", "v1u", "v4u", "none", "none", "none", 4, 4, @"Sum of absolute differences with accumulation.<br>Perform 4x1 SAD with S0.u and S1.u, and accumulate result into msb's of S2.u. Overflow is lost.<br>ABS_DIFF (A,B) = (A>B) ? (A-B) : (B-A) <br>D.u = (ABS_DIFF (S0.u[31:24],S1.u[31:24])+ ABS_DIFF (S0.u[23:16],S1.u[23:16]) + ABS_DIFF (S0.u[15:8],S1.u[15:8]) + ABS_DIFF (S0.u[7:0],S1.u[7:0]) ) << 16 + S2.u ", @"", ISA_Enc.VOP3a3, 347, 0, 0xD2B60000, 0x0007),
new InstInfo(1156, "v_sad_u16", "v2u", "v2u", "v2u", "v4u", "none", "none", "none", 4, 4, @"Sum of absolute differences with accumulation.<br>Perform 2x1 SAD with S0.u and S1.u, and accumulate result with S2.u. <br>ABS_DIFF (A,B) = (A>B) ? (A-B) : (B-A) <br>D.u = ABS_DIFF (S0.u[31:16],S1.u[31:16]) + ABS_DIFF (S0.u[15:0],S1.u[15:0]) + S2.u ", @"", ISA_Enc.VOP3a3, 348, 0, 0xD2B80000, 0x0007),
new InstInfo(1157, "v_sad_u32", "v4u", "v4u", "v4u", "v4u", "none", "none", "none", 4, 4, @"Sum of absolute differences with accumulation.<br>Perform a single-element SAD with S0.u and S1.u, and accumulate result into msb's of S2.u. Overflow is lost.<br>ABS_DIFF (A,B) = (A>B) ? (A-B) : (B-A) <br>D.u = ABS_DIFF (S0.u,S1.u)  + S2.u ", @"", ISA_Enc.VOP3a3, 349, 0, 0xD2BA0000, 0x0007),
new InstInfo(1158, "v_sad_u8", "v1u", "v1u", "v1u", "v4u", "none", "none", "none", 4, 4, @"Sum of absolute differences with accumulation.<br>Perform 4x1 SAD with S0.u and S1.u, and accumulate result into lsbs of S2.u. Overflow into S2.u upper bits is allowed.<br>ABS_DIFF (A,B) = (A>B) ? (A-B) : (B-A) <br>D.u = ABS_DIFF (S0.u[31:24],S1.u[31:24])+ ABS_DIFF (S0.u[23:16],S1.u[23:16])+<br>ABS_DIFF (S0.u[15:8],S1.u[15:8])+ ABS_DIFF (S0.u[7:0],S1.u[7:0]) + S2.u ", @"", ISA_Enc.VOP3a3, 346, 0, 0xD2B40000, 0x0007),
new InstInfo(1159, "v_sin_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Sin function.<br>Input must be normalized from radians by dividing by 2*PI.<br>Valid input domain [-256, +256], which corresponds to an un-normalized input domain [-512*PI, +512*PI].<br>Out of range input results in float 0.<br>D.f = sin(S0.f). ", @"", ISA_Enc.VOP1, 53, 0, 0x7E006A00, 0x0005),
new InstInfo(1160, "v_sin_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Sin function.<br>Input must be normalized from radians by dividing by 2*PI.<br>Valid input domain [-256, +256], which corresponds to an un-normalized input domain [-512*PI, +512*PI].<br>Out of range input results in float 0.<br>D.f = sin(S0.f). ", @"", ISA_Enc.VOP3a1, 437, 0, 0xD36A0000, 0x0007),
new InstInfo(1161, "v_sqrt_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Square root. Useful for normal compression.<br>D.f = sqrt(S0.f). ", @"", ISA_Enc.VOP1, 51, 0, 0x7E006600, 0x0005),
new InstInfo(1162, "v_sqrt_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Square root. Useful for normal compression.<br>D.f = sqrt(S0.f). ", @"", ISA_Enc.VOP3a1, 435, 0, 0xD3660000, 0x0007),
new InstInfo(1163, "v_sqrt_f64", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"D.d = sqrt(S0.d). ", @"", ISA_Enc.VOP1, 52, 0, 0x7E006800, 0x0005),
new InstInfo(1164, "v_sqrt_f64_ext", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"D.d = sqrt(S0.d). ", @"", ISA_Enc.VOP3a1, 436, 0, 0xD3680000, 0x0007),
new InstInfo(1165, "v_sub_f32", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.f = S0.f - S1.f.  ", @"", ISA_Enc.VOP2, 4, 0, 0x08000000, 0x0005),
new InstInfo(1166, "v_sub_f32_ext", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.f = S0.f - S1.f.  ", @"", ISA_Enc.VOP3a2, 260, 0, 0xD2080000, 0x0007),
new InstInfo(1167, "v_sub_i32", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"Unsigned integer subtract based on unsigned integer components. Produces an unsigned borrow out in VCC or a scalar register.<br>D.u = S0.u - S1.u; VCC=carry-out (VOP3:sgpr=carry-out). ", @"", ISA_Enc.VOP2, 38, 0, 0x4C000000, 0x0025),
new InstInfo(1168, "v_sub_i32_ext", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"Unsigned integer subtract based on unsigned integer components. Produces an unsigned borrow out in VCC or a scalar register.<br>D.u = S0.u - S1.u; VCC=carry-out (VOP3:sgpr=carry-out). ", @"", ISA_Enc.VOP3b2, 294, 0, 0xD24C0000, 0x0027),
new InstInfo(1169, "v_subb_u32", "v4u", "v4u", "v4u", "vcc", "none", "none", "none", 4, 4, @"Integer subtract based on signed or unsigned integer components, with borrow in. Produces a borrow out in VCC or a scalar register.<br>D.u = S0.u - S1.u - VCC; VCC=carry-out (VOP3:sgpr=carry-out, S2.u=carry-in). ", @"", ISA_Enc.VOP2, 41, 0, 0x52000000, 0x0025),
new InstInfo(1170, "v_subb_u32_ext", "v4u", "v4u", "v4u", "v1u", "none", "none", "none", 4, 4, @"Integer subtract based on signed or unsigned integer components, with borrow in. Produces a borrow out in VCC or a scalar register.<br>D.u = S0.u - S1.u - VCC; VCC=carry-out (VOP3:sgpr=carry-out, S2.u=carry-in). ", @"", ISA_Enc.VOP3b3, 297, 0, 0xD2520000, 0x0067),
new InstInfo(1171, "v_subbrev_u32", "v4u", "v4u", "v4u", "vcc", "none", "none", "none", 4, 4, @"D.u = S1.u - S0.u - VCC; VCC=carry-out (VOP3:sgpr=carry-out, S2.u=carry-in).  ", @"", ISA_Enc.VOP2, 42, 0, 0x54000000, 0x0065),
new InstInfo(1172, "v_subbrev_u32_ext", "v4u", "v4u", "v4u", "v1u", "none", "none", "none", 4, 4, @"D.u = S1.u - S0.u - VCC; VCC=carry-out (VOP3:sgpr=carry-out, S2.u=carry-in).  ", @"", ISA_Enc.VOP3b3, 298, 0, 0xD2540000, 0x0067),
new InstInfo(1173, "v_subrev_f32", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.f = S1.f - S0.f.  ", @"", ISA_Enc.VOP2, 5, 0, 0x0A000000, 0x0005),
new InstInfo(1174, "v_subrev_f32_ext", "v4f", "v4f", "v4f", "none", "none", "none", "none", 3, 3, @"D.f = S1.f - S0.f.  ", @"", ISA_Enc.VOP3a2, 261, 0, 0xD20A0000, 0x0007),
new InstInfo(1175, "v_subrev_i32", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = S1.u - S0.u; VCC=carry-out (VOP3:sgpr=carry-out).  ", @"", ISA_Enc.VOP2, 39, 0, 0x4E000000, 0x0025),
new InstInfo(1176, "v_subrev_i32_ext", "v4i", "v4i", "v4i", "none", "none", "none", "none", 3, 3, @"D.u = S1.u - S0.u; VCC=carry-out (VOP3:sgpr=carry-out).  ", @"", ISA_Enc.VOP3b2, 295, 0, 0xD24E0000, 0x0027),
new InstInfo(1177, "v_trig_preop_f64", "v3d", "v8f", "v8f", "none", "none", "none", "none", 3, 3, @"D.d = Look Up 2/PI (S0.d) with segment select S1.u[4:0]. ", @"", ISA_Enc.VOP3a2, 372, 0, 0xD2E80000, 0x0007),
new InstInfo(1178, "v_trunc_f32", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Floating point 'integer' part of S0.f.<br>D.f = trunc(S0.f), return integer part of S0. ", @"", ISA_Enc.VOP1, 33, 0, 0x7E004200, 0x0005),
new InstInfo(1179, "v_trunc_f32_ext", "v4f", "v4f", "none", "none", "none", "none", "none", 2, 2, @"Floating point 'integer' part of S0.f.<br>D.f = trunc(S0.f), return integer part of S0. ", @"", ISA_Enc.VOP3a1, 417, 0, 0xD3420000, 0x0007),
new InstInfo(1180, "v_trunc_f64", "v8f", "v8f", "none", "none", "none", "none", "none", 2, 2, @"Truncate a 64-bit floating-point value, and return the resulting integer value.<br>D.d = trunc(S0.d), return integer part of S0.d. ", @"", ISA_Enc.VOP1, 23, 0, 0x7E002E00, 0x0005),
new InstInfo(1181, "v_writelane_b32", "v4b", "s4b", "v1u", "none", "none", "none", "none", 3, 3, @"Write value into one VGPR one one lane.  Dst = VGPR-dest, Src0 = Source Data (SGPR, M0, exec, or constants), Src1 = Lane Select (SGPR or M0).  Ignores exec mask.  ", @"", ISA_Enc.VOP2, 2, 0, 0x04000000, 0x0001),
new InstInfo(1182, "v_writelane_b32_ext", "v4b", "s4b", "v1u", "none", "none", "none", "none", 3, 3, @"Write value into one VGPR one one lane.  Dst = VGPR-dest, Src0 = Source Data (SGPR, M0, exec, or constants), Src1 = Lane Select (SGPR or M0).  Ignores exec mask.  ", @"", ISA_Enc.VOP3a2, 258, 0, 0xD2040000, 0x0003),
new InstInfo(1183, "v_xor_b32", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Logical bit-wise XOR.<br>D.u = S0.u ^ S1.u. ", @"", ISA_Enc.VOP2, 29, 0, 0x3A000000, 0x0005),
new InstInfo(1184, "v_xor_b32_ext", "v4b", "v4b", "v4b", "none", "none", "none", "none", 3, 3, @"Logical bit-wise XOR.<br>D.u = S0.u ^ S1.u. ", @"", ISA_Enc.VOP3a2, 285, 0, 0xD23A0000, 0x0007),
        
        });
    }

    /// <summary>
    /// This is the datatype the user intended. "-0.5" -> NEG_FLOAT. 0b101011 or 0x1D3B -> HEX  
    /// </summary>
    [Flags]
    public enum DataDesc
    {
        /// <summary>Represents a Negative Int</summary>
        NEG_INT = (1 << 0),
        /// <summary>Represents Zero as an Int (0)</summary>
        ZERO_INT = (1 << 1),
        /// <summary>Represents a positive Int</summary>
        POS_INT = (1 << 2),
        /// <summary>Represents a negative Float</summary>
        NEG_FLOAT = (1 << 3),
        /// <summary>Represents a Zero Float. (0.0)</summary>
        ZERO_FLOAT = (1 << 4),
        /// <summary>Represents a Positive Float. (e.g. 2.77)</summary>
        POS_FLOAT = (1 << 5),
        /// <summary>Represents a hex value.</summary>
        HEX_FORMAT = (1 << 7),  //Hex, Octal or bin formats

        ///////// CLASSES /////////
        /// <summary>Represents any negative number</summary>
        NEGITIVE = NEG_INT | NEG_FLOAT,
        /// <summary>Represents any float</summary>
        FLOAT = NEG_FLOAT | ZERO_FLOAT | POS_FLOAT,
        /// <summary>Represents any unsigned Int</summary>
        UINT = ZERO_INT | ZERO_INT | HEX_FORMAT,
        /// <summary>Represents any Int</summary>
        INT = NEG_INT | ZERO_INT | POS_INT | HEX_FORMAT,
    }


    public struct OpInfo
    {
        /// <summary>the reg number, always 0 - 255 or 0 - 511</summary>
        public uint reg;
        /// <summary>literal or value; if const or label this holds a hard value, if immd then holds the immd, else holds the reg number except</summary>
        public uint value;
        /// <summary>Specifies the allowed types for the operand.(e.g. VCC_LO | VCC_HI)</summary>
        public OpType flags;
        /// <summary>This is the datatype the user intended. (e.g. "-0.5" would be NEG_FLOAT, or 0b101011 would be ANY</summary>
        public DataDesc dataDisc;
    }

    public class InstInfo
    {
        /// <summary>A reference id for this instruction. This is also the location in the array.</summary>
        public readonly short id;
        /// <summary>The name of the instruction as defined in AMD's GCN ISA manual.</summary>
        public readonly string name;
        /// <summary>This is the output dataType of the instruction. Example: s4u for a scaler-4_byte_unsigned</summary>
        public readonly string sDestType;
        /// <summary>The 1st param input dataType of the instruction. Example: v8f for a vector-8_byte_float (aka, a double)</summary>
        public readonly string src0Type;
        /// <summary>The 2nd param input dataType of the instruction. Example: v8f for a vector-8_byte_float (aka, a double)</summary>
        public readonly string src1Type;
        /// <summary>The 3rd param input dataType of the instruction. Example: v8f for a vector-8_byte_float (aka, a double)</summary>
        public readonly string src2Type;
        /// <summary>The 4th param input dataType of the instruction. Example: v8f for a vector-8_byte_float (aka, a double)</summary>
        public readonly string src3Type;
        /// <summary>1st OpCode built-in inline constant dataType.</summary>
        public readonly string immd0Type;
        /// <summary>2nd OpCode built-in inline constant dataType.</summary>
        public readonly string immd1Type;
        /// <summary>The maximum number of operators for this asm instruction.</summary>
        public readonly uint opCtMin;
        /// <summary>The minimum number of operators for this asm instruction.</summary>
        public readonly uint opCtMax;
        /// <summary>These are notes from AMD's GCN ISA manual.</summary>
        public readonly string iSANotes;
        /// <summary>Ryan's notes, this is also a to-do notes.</summary>
        public readonly string otherNote;
        /// <summary> Opcode encoding dataType</summary>
        public readonly ISA_Enc encoding;
        /// <summary> 32 bit opcode value</summary>
        public readonly uint OpNum;
        /// <summary>ID of Alternate 64-bit version; 0 if none</summary>
        public readonly int vop3Version;
        /// <summary>Contains encoding with opcode </summary>
        public readonly uint opCode;
        /// <summary>Indicates that this instruction reads the PC bit.</summary>
        public readonly bool readsPC;
        /// <summary>Indicates that this instruction sets the PC bit.</summary>
        public readonly bool setsPC;
        /// <summary>Indicates that this instruction reads the SCC bit.</summary>
        public readonly bool readsSCC;
        /// <summary>Indicates that this instruction sets the SCC bit.</summary>
        public readonly bool setsSCC;
        /// <summary>Indicates that this instruction reads the VCC bit.</summary>
        public readonly bool readsVCC;
        /// <summary>Indicates that this instruction sets the VCC bit.</summary>
        public readonly bool setsVCC;
        /// <summary>Indicates that this instruction reads the EXEC bit.</summary>
        public readonly bool readsEXEC;
        /// <summary>Indicates that this instruction sets the EXEC bit.</summary>
        public readonly bool setsEXEC;
        /// <summary>if true, indicates that this instruction is skipped when EXEC is 0.</summary>
        public readonly bool skipsOnEXEC0;
        /// <summary> The minimum instruction size in dwords. This includes the possibility of growing to vop3 instructions and/or a 32-bit literal.</summary>
        public readonly int minSize;
        /// <summary> The maximum instruction size in dwords. This includes the possibility of growing to vop3 instructions and/or a 32-bit literal.</summary>
        public readonly int maxSize;

        /// <summary>
        /// Contains a bulk of information on an instruction.
        /// </summary>
        public InstInfo(short id, string name, string sDestType, string src0Type, string src1Type, string src2Type, string src3Type, string immd0Type, string immd1Type, uint opCtMin, uint opCtMax, string iSANotes, string otherNote, ISA_Enc encoding, uint OpNum, int vop3Version, uint opCode, int bitSpecs)
        //new InstInfo("S_LOAD_dword",     "s4b",                "s4u",      "none",          "none",            "none",             "none", "       none",          2,          3,         @"Read tr",           @"",       ISA_Enc.SMRD,        0,             0,                0xC0000000,         0x0140),
        {
            this.id = id;
            this.name = name;
            this.sDestType = sDestType;
            this.src0Type = src0Type;
            this.src1Type = src1Type;
            this.src2Type = src2Type;
            this.src3Type = src3Type;
            this.immd0Type = immd0Type;
            this.immd1Type = immd1Type;
            this.opCtMin = opCtMin;
            this.opCtMax = opCtMax;
            this.iSANotes = iSANotes;
            this.otherNote = otherNote;
            this.encoding = encoding;
            this.OpNum = OpNum;
            this.vop3Version = vop3Version;
            this.opCode = opCode;
            this.readsPC = (bitSpecs & (1 << 10)) != 0;
            this.setsPC = (bitSpecs & (1 << 9)) != 0;
            this.readsSCC = (bitSpecs & (1 << 8)) != 0;
            this.setsSCC = (bitSpecs & (1 << 7)) != 0;
            this.readsVCC = (bitSpecs & (1 << 6)) != 0;
            this.setsVCC = (bitSpecs & (1 << 5)) != 0;
            this.readsEXEC = (bitSpecs & (1 << 4)) != 0;
            this.setsEXEC = (bitSpecs & (1 << 3)) != 0;
            this.skipsOnEXEC0 = (bitSpecs & (1 << 2)) != 0;
            this.minSize = ((bitSpecs & (1 << 1)) != 0) ? 2 : 1;
            this.maxSize = ((bitSpecs & (1 << 0)) != 0) ? 2 : 1;
        }
    }
}
