// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without any warranty.
using GcnTools;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GcnTools
{

    public struct OpCode
    {
        /// <summary>
        /// Holds the main part of the 64-bit instructions or the entire instruction for 32-bit instructions.
        /// </summary>
        public UInt32 code;

        /// <summary>Optional 32 bit literal value or extended code value</summary>
        public UInt32? literal;

        /// <summary>Returns the size of the instruction in words. (1 = 4 byte or 2 = 8 byte)</summary>
        public int Size { get { return literal.HasValue ? 2 : 1; } }
    }

    internal static class ParseOperand
    {
       /// <summary>
       /// Parses unsigned constant values. Parses hex, oct, bin and exponent values as well.
       /// </summary>
       /// <param name="val">The string value to convert from. It can be hex, oct, bin, or nnnExx</param>
        /// <param name="maxBitSize"></param>
        /// <param name="paramNo">The parameter place for logging errors. </param>
        /// <param name="log"></param>
       /// <returns></returns>
        internal static uint parseUnSignedNumber(string val, int maxBitSize, int paramNo, Log log)
        {
            Match m = Regex.Match(val, RegexRecognizers.UnSignedNumber);

            if (!m.Groups[2].Success)
            {
                log.Error("param {0}: unable to recognize constant '{1}'", paramNo, val);
                return 0;
            }

            char opType = m.Groups[1].Value[0];
            string opVal = m.Groups[2].Value;

            if (opVal == "")
                log.Error("param {0}: compiler error 4360 '{1}'", paramNo, val);

            uint num;
            if (opType > '0' && opType <= '9')
                num = UInt32.Parse(opVal, NumberStyles.AllowLeadingSign | NumberStyles.AllowExponent);
            else // if (opType == 'x' || opType == 'o' || opType == 'b')
                num = Convert.ToUInt32(opVal, (opType == 'x') ? 16 : (opType == 'o') ? 8 : 2);

            if (num > ((1 << maxBitSize) - 1))
            {
                log.Error("param {0}: The value '{1}' will not fit in {2} bits", paramNo, val, maxBitSize);
                num &= (uint)((1 << maxBitSize) - 1);
            }

            return num;
        }


        internal static uint parseSignedNumber(string val, int maxBitSize, int paramNo, Log log)
        {
            Match m = Regex.Match(val, @"^(?i:" +
                @"(?:0(?<1>x)(?<2>[0-9a-f]+))|" + // hex values 0x12AB
                @"(?:0(?<1>o)(?<2>[0-7]+))|" + // oct values 0o7564
                @"(?:0(?<1>b)(?<2>[01]+))|" + // bin values 0b1101010
                @"(?<2>-?(?<1>\d)\d*(?:E[+-]?\d+)?)" +//simple number with optional exponent(nnnExx, nnnE+xx, and nnnE-xx)
                //@"|(?:(?<1>@)(?<2>[a-z_][0-9a-z_]+))" + //labels  @myLabel (removed: labels processed before parsing)
                @")$");

            if (!m.Groups[2].Success)
            {
                log.Error("param {0}: unable to recognize constant '{1}'", paramNo, val);
                return 0;
            }

            char opType = m.Groups[1].Value[0];
            string opVal = m.Groups[2].Value;

            if (opVal == "")
                log.Error("param {0}: compiler error 4360 '{1}'", paramNo, val);

            int num;
            if (opType > '0' && opType <= '9')
                num = Int32.Parse(opVal, NumberStyles.AllowLeadingSign | NumberStyles.AllowExponent);
            else // if (opType == 'x' || opType == 'o' || opType == 'b')
                num = Convert.ToInt32(opVal, (opType == 'x') ? 16 : (opType == 'o') ? 8 : 2);

            if (num > ((1 << maxBitSize - 1) - 1))
                log.Error("param {0}: The value '{1}' will not fit in {2} bits", paramNo, val, maxBitSize);

            if (num < -(1 << maxBitSize))
                log.Error("param {0}: The value '{1}' will not fit in {2} bits", paramNo, val, maxBitSize);

            int mask = ((1 << maxBitSize) - 1);

            return (uint)(num & mask); //returned as uint for b32
        }


        internal static OpInfo parseOperand(string val, OpType allowedTypesFlag, int paramNo, Log log)
        {
            //lets try and resolve any aliases
            OpInfo opInfo;

            if (ISA_DATA.sRegAliases.TryGetValue(val, out opInfo))
            {
                if (!allowedTypesFlag.HasFlag(opInfo.flags))
                    log.Error("The dataType '{0}' is not allowed for param #{1}.", paramNo, opInfo.flags, paramNo);
               return opInfo;
            }

            Match m = Regex.Match(val, RegexRecognizers.Operand);

            if (!m.Groups[2].Success)
            {
                log.Error("param {0}: unable to recognize operand '{1}'", paramNo, val);
                return opInfo;
            }

            char opType = m.Groups[1].Value[0];
            string opVal = m.Groups[2].Value;

            if (opVal == "")
                log.Error("param {0}: compiler error 4359 '{1}'", paramNo, val);

            switch (opType)
            {
                case 's': //scalier register
                    opInfo.reg = opInfo.value = UInt32.Parse(opVal);
                    opInfo.flags = ISA_DATA.GetFlagsFromRegNum(opInfo.value, log);
                    if (opInfo.reg > 255)
                        log.Error("param {0}: unable to use scalier greater then 255.", paramNo);
                    break;
                case 'v': //vector register
                    //uint v_offset = allowedTypesFlag.HasFlag(OpType.SCALAR_DST) ? (uint)256 : 0;
                    opInfo.reg = opInfo.value = 256 + UInt32.Parse(opVal);
                    opInfo.flags = OpType.VGPR;
                    if (opInfo.reg > (256 + 255))
                        log.Error("param {0}: unable to use vector greater then 255.", paramNo);
                    break;
                case 'x': //hex value
                case 'o': //hex value
                case 'b': //hex value
                    uint hexVal = Convert.ToUInt32(opVal, (opType == 'x') ? 16 : (opType == 'o') ? 8 : 2);
                    opInfo.value = hexVal;
                    opInfo.reg = ConvertIntToRegNum((int)hexVal);
                    if (opInfo.reg == 255) // LITERAL
                        opInfo.flags = OpType.LITERAL;
                    else if (hexVal > 0)
                        opInfo.flags = OpType.INLINE_INT_POS;
                    else if (hexVal == 0)
                        opInfo.flags = OpType.ZERO;
                    else // (val < 0)
                        opInfo.flags = OpType.INLINE_INT_NEG;

                    opInfo.dataDisc = DataDesc.HEX_FORMAT;
                    break;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':  //simple number or exponent 
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':

                    int decVal = 0;
                    if (!Int32.TryParse(opVal, NumberStyles.AllowLeadingSign | NumberStyles.AllowExponent, CultureInfo.CurrentCulture, out decVal))
                    {
                        log.Error("param {0}: unable to convert {1} to an int. It could be out of range.", paramNo, opVal);
                        decVal = 0;
                        break;
                    }

                    opInfo.reg = ConvertIntToRegNum(decVal);
                    if (opInfo.reg == 255) // LITERAL
                    {
                        opInfo.flags = OpType.LITERAL;
                        opInfo.dataDisc = (decVal > 0) ? DataDesc.POS_INT : DataDesc.NEG_INT;
                        opInfo.value = BitConverter.ToUInt32(BitConverter.GetBytes(decVal), 0);
                    }
                    else if (decVal > 0)
                    {
                        opInfo.dataDisc = DataDesc.POS_INT;
                        opInfo.flags = OpType.INLINE_INT_POS;
                    }
                    else if (decVal == 0)
                    {
                        opInfo.dataDisc = DataDesc.ZERO_INT;
                        opInfo.flags = OpType.ZERO;
                    }
                    else // (lit < 0)
                    {
                        opInfo.dataDisc = DataDesc.NEG_INT;
                        opInfo.flags = OpType.INLINE_INT_NEG;
                    }
                    break;
                case '.': //float value - The below should  only be hit if not found in sRegAliases... like 0.000 or -.5 
                    float temp = float.Parse(opVal, NumberStyles.Float);

                    if (temp == 0.0) { opInfo.reg = 240; opInfo.flags = OpType.ZERO; opInfo.dataDisc = DataDesc.ZERO_FLOAT; }
                    else if (temp == 0.5) { opInfo.reg = 240; opInfo.flags = OpType.INLINE_FLOAT_POS; opInfo.dataDisc = DataDesc.POS_FLOAT; }
                    else if (temp == -0.5) { opInfo.reg = 241; opInfo.flags = OpType.INLINE_FLOAT_NEG; opInfo.dataDisc = DataDesc.NEG_FLOAT; }
                    else if (temp == 1.0) { opInfo.reg = 242; opInfo.flags = OpType.INLINE_FLOAT_POS; opInfo.dataDisc = DataDesc.POS_FLOAT; }
                    else if (temp == -1.0) { opInfo.reg = 243; opInfo.flags = OpType.INLINE_FLOAT_NEG; opInfo.dataDisc = DataDesc.NEG_FLOAT; }
                    else if (temp == 2.0) { opInfo.reg = 244; opInfo.flags = OpType.INLINE_FLOAT_POS; opInfo.dataDisc = DataDesc.POS_FLOAT; }
                    else if (temp == -2.0) { opInfo.reg = 245; opInfo.flags = OpType.INLINE_FLOAT_NEG; opInfo.dataDisc = DataDesc.NEG_FLOAT; }
                    else if (temp == 4.0) { opInfo.reg = 246; opInfo.flags = OpType.INLINE_FLOAT_POS; opInfo.dataDisc = DataDesc.POS_FLOAT; }
                    else if (temp == -4.0) { opInfo.reg = 247; opInfo.flags = OpType.INLINE_FLOAT_NEG; opInfo.dataDisc = DataDesc.NEG_FLOAT; }
                    else
                    {
                        opInfo.reg = 255;
                        opInfo.flags = OpType.LITERAL;
                        opInfo.dataDisc = (opVal[0] == '-') ? DataDesc.NEG_FLOAT : DataDesc.POS_FLOAT;
                        opInfo.value = BitConverter.ToUInt32(BitConverter.GetBytes(temp), 0);
                    }
                    break;
                //case '@': // label //removed: we replace labels with literals before parsing. Depends on the distance I guess.
                //    labels[opVal].AddOccurrence(line);
                //    opInfo.dataDisc = DataDesc.LABEL_NAME;
                //    break;
                default: log.Error("param {0}: unable to decode operand '{1}'", paramNo, val); break;
            }

            if (!allowedTypesFlag.HasFlag(opInfo.flags))
                log.Error("param {0}: '{1}' is not in the allowed list of '{2}'", paramNo, opInfo.flags, allowedTypesFlag);

            return opInfo;
        }


        internal static uint parseOnlyVGPR(string val, int paramNo, Log log)
        {
            uint numVal;

            // lets try and resolve any aliases
            OpInfo opInfo;
            if (ISA_DATA.sRegAliases.TryGetValue(val, out opInfo))
            {
                if (!opInfo.flags.HasFlag(OpType.VGPR))
                    log.Error("param {0}: The dataType '{1}' is not allowed here. Only VGPRs are allowed.", paramNo, opInfo.flags);
                numVal = opInfo.reg & 0xFF; //should only be between 0-255 (not 256-511)
            }
            else
            {
                Match m = Regex.Match(val, @"(?ix)"+
                    @"(?:v(?<1>\d+))"+            // vector register in s# format
                    @"|(?:v\[(?<1>\d+):\d+\])");  // vector register in s[#:#] format


                if (!UInt32.TryParse(m.Groups[1].Value, out numVal))
                    log.Error("param {0}: '{1}' must be a VGPR or alias", paramNo, val);


                if (numVal > 255)
                    log.Error("param {0}: unable to use VGPRs greater then 255.", paramNo);
            }

            return numVal;
        }

        internal static uint parseOnlySGPR(string val, int paramNo, Log log, OpType allowedTypesFlag = OpType.SCALAR_DST)
        {
            OpInfo opInfo;

            // lets try and resolve any aliases
            if (ISA_DATA.sRegAliases.TryGetValue(val, out opInfo))
            {
                if (!allowedTypesFlag.HasFlag(opInfo.flags))
                    log.Error("param {0}: The dataType '{1}' is not allowed here. Only {2} are allowed.", paramNo, opInfo.flags, allowedTypesFlag);
            }
            else
            {
                Match m = Regex.Match(val, @"(?ix)" +
                    @"(?:s(?<1>\d+))" +          // scalar register in s# format
                    @"|(?:s\[(?<1>\d+):\d+\])"); // scalar register in s[#:#] format

                if (m.Groups[1].Success)
                    opInfo.reg = UInt32.Parse(m.Groups[1].Value);
                else
                    log.Error("param {0}: '{1}' must be a SGPR", paramNo, val);

                opInfo.flags = ISA_DATA.GetFlagsFromRegNum(opInfo.reg, log);
            }

            if (!allowedTypesFlag.HasFlag(opInfo.flags))
                log.Error("param {0}: '{1}' is not in the allowed list of '{2}'", paramNo, opInfo.flags, allowedTypesFlag);

            //System.Enum.Format(OpType, null, ""))
            return opInfo.reg;
        }

        /// <summary>
        /// Looks for a series of particular regex group matches and if they exist the specified values 
        /// are OR'ed together to form the resulting UINT output. 
        /// </summary>
        internal static uint setBitOnFound(Match m, string sa, int oa, string sb = null, int ob = 0, string sc = null,
                                   int oc = 0, string sd = null, int od = 0, string se = null, int oe = 0)
        {
            uint bits = (m.Groups[sa].Success ? (uint)1 << oa : 0);
            if (sb != null) bits |= (m.Groups[sb].Success ? (uint)1 << ob : 0);
            if (sc != null) bits |= (m.Groups[sc].Success ? (uint)1 << oc : 0);
            if (sd != null) bits |= (m.Groups[sd].Success ? (uint)1 << od : 0);
            if (se != null) bits |= (m.Groups[se].Success ? (uint)1 << oe : 0);
            return bits;
        }


        private static uint ConvertIntToRegNum(int val)
        {
            if (val < -16 || val > 64)
                return 255; //Literal
            else
                return (uint)(val + (val >= 0 ? 128 : 208));
        }

    }
}
