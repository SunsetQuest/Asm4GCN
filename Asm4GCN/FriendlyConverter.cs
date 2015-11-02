using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GcnTools
{
    static class FriendlyConverter
    {


        public static string FriendlyFormatToAsmFormater(string stmtText, Variables vars, int ISA_Gen, Log log)
        {
            // find: v4u d = a + b ---> v_add_i32  v4u d, vcc, a, b 
            Match m = Regex.Match(stmtText,
                @"(?<![a-z_0-9])(?<1>(?<2>s|v)(?<3>1|2|4|8|16)(?<4>[fiub])[ \t]+)?(?<5>[A-Za-z_0-9\.]+)[ \t]*=[ \t]*(?<6>[A-Za-z_0-9\.]*)" +
                @"[ \t]*(?<7>\+|\*|\-|\>\>|\<\<)" +
                @"[ \t]*(?<8>[A-Za-z_0-9\.]*)");

            if (!m.Success)
                return stmtText;

            char opp = m.Groups[7].Value[0];
            string destName = m.Groups[5].Value, src0Name = m.Groups[6].Value, src1Name = m.Groups[8].Value;
            char destType = 'b', src0Type = 'b', src1Type = 'b';
            char destScaler = '_', src0Scaler = '_', src1Scaler = '_';
            int destSize = 0, src0Size = 0, src1Size = 0; // 0 = unknown
            bool hasDecloration = m.Groups[1].Success;
            string declorationType = "";
            int junk;

            // Lets figure out the destination type
            if (hasDecloration)
            {
                destScaler = m.Groups[2].Value[0];
                destSize = int.Parse(m.Groups[3].Value);
                destType = m.Groups[4].Value[0];
                declorationType = m.Groups[1].Value + " ";
            }
            else if (vars.varsByName.ContainsKey(destName))
            {
                Variable v = vars.varsByName[destName];
                destScaler = v.isScaler ? 's' : 'v';
                destSize = v.size;
                destType = v.type;
            }
            else if (destName[0] == 's' | destName[1] == 'v' &&
                int.TryParse(destName.Substring(1), out junk))
            {
                destScaler = destName[0];
            }
            else
            {
                log.Error("Unable to find the destination data-type of '{0}'.", stmtText);
                return "";
            }


            // Lets figure out the source0 and source1 types
            DecodeVal(stmtText, src0Name, ref src0Type, ref src0Scaler, ref src0Size, vars, log);
            DecodeVal(stmtText, src1Name, ref src1Type, ref src1Scaler, ref src1Size, vars, log);
            
            if (src0Type == 'c' && src1Type == 'c')
            {
                log.Error("Both sources cannot be constants. '{0}'", stmtText);
                return "";
            }
            
            if (opp == '+')
            {
                if (destScaler == 's')
                {
                    if (destSize != 4)
                    {
                        log.Error("Scalar destination size must be 4 bytes. '{0}'", stmtText);
                        return "";
                    }

                    if (!((src0Size == 4) || (src0Size == 0)
                        || (src1Size == 4) || (src1Size == 0)))
                    {
                        log.Error("Scalar source size must be 4. '{0}'", stmtText);
                        return "";
                    }

                    if (destType == 'i')
                    { // s4i = ??? + ???
                        if (src0Type != 'c' && src1Type != 'c')
                            return "s_add_i32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else if (src0Type == 'c' && src0Size == 2)
                            return "s_addk_i32 " + declorationType + destName + ", " + src1Name + ", " + src0Name;
                        else if (src0Type == 'c') //for sizes -16-64 or larger then 16-bit use normal add
                            return "s_add_i32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else
                        {
                            log.Error("Error decoding friendly format for '{0}'.", stmtText);
                            return "";
                        }
                    }
                    else if (destType == 'u' || destType == 'b')
                    { // s4u = ??? + ???
                        if (src0Type != 'c' && src1Type != 'c')
                            return "s_add_u32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else if (src0Type == 'c' && src0Size == 2)
                            return "s_addk_i32 " + declorationType + destName + ", " + src1Name + ", " + src0Name;
                        else if (src0Type == 'c') //for sizes -16-64 or larger then 16-bit use normal add
                            return "s_add_u32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else
                        {
                            log.Error("Error decoding friendly format for '{0}'.", stmtText);
                            return "";
                        }
                    }
                    else if (destType == 'f')
                    {
                        log.Error("Scalar destinations of type float are not supported. '{0}'", stmtText);
                        return "";
                    }
                    else
                    {
                        log.Error("Unknown error when processing: '{0}'", stmtText);
                        return "";
                    }
                }
                else // is vector
                {
                    if (!((destSize == 2) || (destSize == 4) || (destSize == 8) || (destSize == 0)))
                    {
                        log.Error("Vector destination size must be 2, 4, or 8 bytes. '{0}'", stmtText);
                        return "";
                    }

                    if (!((src0Size == 2) || (src0Size == 4) || (src0Size == 8) || (src0Size == 0)
                        || (src1Size == 2) || (src1Size == 4) || (src1Size == 8) || (src1Size == 0)))
                    {
                        log.Error("Vector source size must be 2, 4, or 8 bytes. '{0}'", stmtText);
                        return "";
                    }

                    if (destSize == 2)
                    {
                        if (destType == 'i') // v2i = ??? + ???
                            return "v_add_i16 " + declorationType + destName + "," + src0Name + "," + src1Name;
                        else if (destType == 'u' || destType == 'b') // v2u = ??? + ???
                            return (ISA_Gen == 2 ? "v_add_i16 " : "v_add_u16 ") + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        //return "v_add_u16 " + destName + "," + src0Name + "," + src1Name;
                        else if (destType == 'f') // v2f = ??? + ???
                            return "v_add_f16 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else
                        {
                            log.Error("Vector destination type not supported. '{0}'", stmtText);
                            return "";
                        }
                    }
                    else if (destSize == 4)
                    {
                        if (destType == 'i')
                            return "v_add_i32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else if (destType == 'u' || destType == 'b')
                            return (ISA_Gen == 2 ? "v_add_i32 " : "v_add_u32 ") + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else if (destType == 'f')
                            return "v_add_f32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else
                        {
                            log.Error("Vector destination type not supported. '{0}'", stmtText);
                            return "";
                        }
                    }
                    else if (destSize == 8)
                    {
                        if (destType == 'f')
                            return "v_add_f64 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else
                        {
                            log.Error("64-bit add operations are not supported. '{0}'", stmtText);
                            return "";
                        }
                    }
                }
            }
            else if (opp == '-')
            {
                if (destScaler == 's')
                {
                    if (destSize != 4)
                    {
                        log.Error("Scalar destination size must be 4 bytes. '{0}'", stmtText);
                        return "";
                    }

                    if (!((src0Size == 4) || (src0Size == 0)
                        || (src1Size == 4) || (src1Size == 0)))
                    {
                        log.Error("Scalar source size must be 4. '{0}'", stmtText);
                        return "";
                    }

                    if (destType == 'i')
                    { // s4i = ??? + ???
                        return "s_sub_i32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                    }
                    else if (destType == 'u' || destType == 'b')
                    { // s4u = ??? + ???
                        return "s_sub_u32 " + declorationType + destName + ", " + src0Name + ", " + src1Name; 
                    }
                    else if (destType == 'f')
                    {
                        log.Error("Scalar destinations of type float are not supported. '{0}'", stmtText);
                        return "";
                    }
                    else
                    {
                        log.Error("Unknown error when processing: '{0}'", stmtText);
                        return "";
                    }
                }
                else // is vector
                {
                    if (!((destSize == 2) || (destSize == 4) || (destSize == 8) || (destSize == 0)))
                    {
                        log.Error("Vector destination size must be 2, 4, or 8 bytes. '{0}'", stmtText);
                        return "";
                    }

                    if (!((src0Size == 2) || (src0Size == 4) || (src0Size == 8) || (src0Size == 0)
                        || (src1Size == 2) || (src1Size == 4) || (src1Size == 8) || (src1Size == 0)))
                    {
                        log.Error("Vector source size must be 2, 4, or 8 bytes. '{0}'", stmtText);
                        return "";
                    }

                    if (destSize == 2)
                    {
                        if (destType == 'i') // v2i = ??? - ???
                            return "v_sub_i16 " + declorationType + destName + "," + src0Name + "," + src1Name;
                        else if (destType == 'u' || destType == 'b') // v2u = ??? + ???
                            return (ISA_Gen == 2 ? "v_sub_i16 " : "v_sub_u16 ") + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        //return "v_add_u16 " + destName + "," + src0Name + "," + src1Name;
                        else if (destType == 'f') // v2f = ??? - ???
                            return "v_sub_f16 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else
                        {
                            log.Error("Vector destination type not supported. '{0}'", stmtText);
                            return "";
                        }
                    }
                    else if (destSize == 4)
                    {
                        if (destType == 'i')
                            return "v_sub_i32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else if (destType == 'u' || destType == 'b')
                            return (ISA_Gen == 2 ? "v_sub_i32 " : "v_sub_u32 ") + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else if (destType == 'f')
                            return "v_sub_f32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else
                        {
                            log.Error("Vector destination type not supported. '{0}'", stmtText);
                            return "";
                        }
                    }
                    else if (destSize == 8)
                    {
                        log.Error("64-bit vector subtract operations not supported. '{0}'", stmtText);
                        return "";
                    }
                }
            }
            else if (opp == '*')
            {
                if (destScaler == 's')
                {
                    if (destSize != 4)
                    {
                        log.Error("Scalar destination size must be 4 bytes. '{0}'", stmtText);
                        return "";
                    }

                    if (destType == 'f')
                    {
                        log.Error("Scalar destinations of type float are not supported for multiply. '{0}'", stmtText);
                        return "";
                    }

                    if (src0Type != 'c' && src1Type != 'c')
                        return "s_mul_i32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                    else if (src0Type == 'c' && src0Size == 2)
                        return "s_mulk_i32 " + declorationType + destName + ", " + src1Name + ", " + src0Name;
                    else if (src0Type == 'c') //for +/- 0.5 1.0 2.0 4.0  use normal multiply
                        return "s_mul_i32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                    else
                    {
                        log.Error("Error decoding friendly format for '{0}'.", stmtText);
                        return "";
                    }
                }
                else // is vector
                {
                    if (destSize == 2)
                    {
                        if (destType == 'f')
                            return "v_mul_f16 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else
                        {
                            log.Error("With a 16-bit vector destination, only floats are supported. '{0}'", stmtText);
                            return "";
                        }
                    }
                    else if (destSize == 4)
                    {
                        if (destType == 'i')
                            return "v_mul_i32_i24 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else if (destType == 'u' || destType == 'b')
                            return "v_mul_u32_u24 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else if (destType == 'f')
                            return "v_mul_f32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else
                        {
                            log.Error("Vector destination type not supported. '{0}'", stmtText);
                            return "";
                        }
                    }
                    else if (destSize == 8)
                    {
                        if (destType == 'f')
                            return "v_mul_f32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else
                        {
                            log.Error("Vector destination type not supported. '{0}'", stmtText);
                            return "";
                        }
                    }
                }
            }
            else if (opp == '>')
            {
                if (destScaler == 's')
                {
                    if (destSize == 4)
                    {
                        if (destType == 'i')    // s4i = ??? >> ???
                            return "s_lshr_i32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else                    // s4u = ??? >> ???
                            return "s_lshr_b32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                    }
                    else if (destSize == 8)
                    {
                        if (destType == 'i')    // s8i = ??? >> ???
                            return "s_lshr_i64 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                        else                    // s8u = ??? >> ???
                            return "s_lshr_b64 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                    }
                    else
                    {
                        log.Error("The destination size is not supported. '{0}'", stmtText);
                        return "";
                    }
                }
                else // is vector
                {
                    if (destSize == 2)
                    { 
                        if (destType == 'i') // v2i = ??? >> ???
                            return "v_ashrrev_i16 " + declorationType + destName + "," + src1Name + "," + src0Name;
                        else 
                            return "v_lshrrev_b16 " + declorationType + destName + ", " + src1Name + "," + src0Name;
                    }
                    else if (destSize == 4)
                    {
                        if (destType == 'i') // v4i = ??? >> ???
                            return "v_ashrrev_i32 " + declorationType + destName + ", " + src1Name + "," + src0Name;
                        else
                            return "v_lshrrev_b32 " + declorationType + destName + ", " + src1Name + "," + src0Name;
                    }
                    else if (destSize == 8)
                    {
                        if (destType == 'i') // v4i = ??? >> ???
                            return "v_ashrrev_i64 " + declorationType + destName + ", " + src1Name + "," + src0Name;
                        else
                            return "v_lshrrev_b64" + declorationType + destName + ", " + src1Name + "," + src0Name;
                    }
                    else
                    {
                        log.Error("The destination size is not supported. '{0}'", stmtText);
                        return "";
                    }
                }
            }
            else if (opp == '<') // for '<<'
            {
                if (destScaler == 's')
                {
                    if (destSize == 4)
                        return "s_lshl_b32 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                    else if (destSize == 8)
                       return "s_lshl_i64 " + declorationType + destName + ", " + src0Name + ", " + src1Name;
                    else
                    {
                        log.Error("The destination size is not supported. '{0}'", stmtText);
                        return "";
                    }
                }
                else // is vector
                {
                    if (destSize == 2)
                        return "v_lshlrev_b16 " + declorationType + destName + ", " + src1Name + "," + src0Name;
                    else if (destSize == 4)
                        return "v_lshlrev_b32 " + declorationType + destName + ", " + src1Name + "," + src0Name;
                    else if (destSize == 8)
                        return "v_lshlrev_b64 " + declorationType + destName + ", " + src1Name + "," + src0Name;
                    else
                    {
                        log.Error("The destination size is not supported. '{0}'", stmtText);
                        return "";
                    }
                }
            }
            else
            {
                log.Error("Friendly conversion not recognized. '{0}'", stmtText);
                return "";
            }

            return "";
        }

        public static void DecodeVal(string stmtText, string src0Name, ref char src0Type, ref char src0Scaler, ref int src0Size, Variables vars, Log log)
        {
            char firstChar = src0Name[0];
            int junk;
            if (vars.varsByName.ContainsKey(src0Name))
            {
                Variable v = vars.varsByName[src0Name];
                src0Scaler = v.isScaler ? 's' : 'v';
                src0Size = v.size;
                src0Type = v.type;
            }
            else if (firstChar == 's' | firstChar == 'v' &&
                int.TryParse(src0Name.Substring(1), out junk))
            {
                src0Scaler = firstChar;
            }
            else if ((firstChar >= '0' && firstChar <= '9') || firstChar == '-')
            {
                src0Scaler = 'c';
                src0Size = 0;
                int temp;
                if (int.TryParse(src0Name, out temp))
                {
                    src0Type = 'b';
                    if (temp >= -16 && temp <= 64)
                        src0Size = 1;
                    else if ((temp & 0xFFFF0000) == 0)
                        src0Size = 2;
                    else
                        src0Size = 4;
                }
                else if (src0Name == "-0.5" || src0Name == "0.5" || src0Name == "-1.0" || src0Name == "1.0" || src0Name == "-2.0" || src0Name == "2.0" || src0Name == "-4.0" || src0Name == "4.0")
                {
                    src0Size = 1;
                    src0Type = 'f';
                }
                else
                    log.Error("Unable to decode '{0}' in '{1}'.", src0Name, stmtText);
            }
            else
            {
                log.Error("Unable to find the source data-type of '{0}'.", stmtText);
            }
        }
    }
}
