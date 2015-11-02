using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GcnTools
{

    /// <summary>
    /// Contains RegEx language recognizers for GCN.
    /// </summary>
    public static class RegexRecognizers
    {
        public const string UnSignedNumber =
            @"^(?i:" +
                @"(?:0(?<1>x)(?<2>[0-9a-f]+))|" +   // hex values 0x12AB
                @"(?:0(?<1>o)(?<2>[0-7]+))|" +      // oct values 0o7564
                @"(?:0(?<1>b)(?<2>[01]+))|" +       // bin values 0b1101010
                @"(?<2>(?<1>\d)\d*(?:E[+-]?\d+)?)" + //simple number with optional exponent(nnnExx, nnnE+xx, and nnnE-xx)
                //@"|(?:(?<1>@)(?<2>[a-z_][0-9a-z_]+))" + //labels  @myLabel  (removed: labels processed before parsing)
                @")$";

        public const string Operand =
                @"^(?i:" +
                @"(?:(?<1>s)(?<2>\d+))|" +  //scalier register snnn
                @"(?:(?<1>s)\[(?<2>\d+):\d+\])|" + //scalier register s[nnn:nnn]
                @"(?:(?<1>v)(?<2>\d+))|" +  //vector register vnnn
                @"(?:(?<1>v)\[(?<2>\d+):\d+\])|" + //vector register v[nnn:nnn]
                @"(?:0(?<1>x)(?<2>[0-9a-f]+))|" + //hex values 0x12AB
                @"(?:0(?<1>o)(?<2>[0-7]+))|" + //oct values 0o7564
                @"(?:0(?<1>b)(?<2>[01]+))|" + //bin values 0b1101010
                @"(?:(?<2>-?\d*(?<1>\.)\d*)(?:E[+-]?\d+)?)|" + //float values with optional exponent -2.1E45
                //@"(?:(?<1>)(?<2>[+-]?\d+))|"+ //simple number 
                @"(?<2>-?(?<1>\d)\d*(?:E[+-]?\d+)?)" +//simple number with optional exponent(nnnExx, nnnE+xx, and nnnE-xx)
                //@"|(?:(?<1>@)(?<2>[a-z_][0-9a-z_]+))" + //labels  @myLabel (removed: labels processed before parsing)
                @")$";

    }
}
