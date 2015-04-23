// Source: http://www.codeproject.com/Articles/867045/Csharp-Based-Template-Transformation-Engine
using System;

namespace OpenClWithGcnNS
{
    public static class T44 //calling it T44 after Microsoft's T4
    {
        public static bool Expand(string input, out string output)
        {
            // For [[CODE]] ,  [[~FULL_LINE_OF_CODE  &  [[!SKIP_ME]]
            // style uncomment the next 9 lines of code
            const string REG = 
                @"(?<txt>.*?)" +                     // grab any normal text
                @"(?<dataType>\[\[[!~=]?)" + // get dataType of code block
                @"(?<code>.*?)" +                    // get the code or expression
                @"(\]\]|(?<=\[\[~[^\r\n]*?)\r\n)";   // terminate the code or expression
            const string NORM = @"[[";  
            const string FULL = @"[[~"; 
            const string EXPR = @"[[="; 
            const string TAIL = @"]]";
            
            //// For /*:CODE:*/ ,  //:FULL_LINE_OF_CODE  &  //!SKIP_ME
            //// style uncomment the next 9 lines of code
            //const string REG =
            //    @"(?<txt>.*?)" +                       // grab any normal text
            //    @"((/\*\*/.*?/\*\*/)|(?<dataType>/(/!|\*:|\*=|/:|\*!))" + // get code 
            //    @"(?<code>.*?)" +                      // get the code or expression
            //    @"(:\*/|(?<=//[:|!][^\r\n]*)\r\n))"; // terminate the code or expression
            //const string NORM = @"/*:";               
            //const string FULL = @"//:"; 
            //const string EXPR = @"/*="; 
            //const string TAIL = @":*/";
            
            //////////////// Step 1 - Build the generator program ////////////////
            System.Text.StringBuilder prog = new System.Text.StringBuilder();
            prog.AppendLine(
@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
    class T44Class { 
    static StringBuilder sb = new StringBuilder();
    public string Execute() {");
            foreach (System.Text.RegularExpressions.Match m in
                System.Text.RegularExpressions.Regex.Matches(input + NORM + TAIL, REG,
                System.Text.RegularExpressions.RegexOptions.Singleline))
            {
                prog.Append(" sb.Append(@\"" + m.Groups["txt"].Value.Replace("\"", "\"\"") + "\");");
                string txt = m.Groups["code"].Value;
                switch (m.Groups["dataType"].Value)
                {
                    case NORM: prog.Append(txt); break;  // text to be added
                    case FULL: prog.AppendLine(txt); break;
                    case EXPR: prog.Append(" sb.Append(" + txt + ");"); break;
                }
                    }
            prog.AppendLine(
@"  return sb.ToString();}
static void Write<T>(T val) { sb.Append(val);}
static void Format(string format, params object[] args) { sb.AppendFormat(format,args);}
static void WriteLine(string val) { sb.AppendLine(val);}
static void WriteLine() { sb.AppendLine();}
}");

            //cleanup
            string program = prog.ToString();
            //program = program.Replace(@"""); sb.Append(@""", "");
            //program = program.Replace(@"; sb.Append(@""", @" + @""");
            //program = program.Replace(@"""); sb.Append(", @""" + ");

            //////////////// Step 2 - Compile the generator program ////////////////
            var res = (new Microsoft.CSharp.CSharpCodeProvider()).CompileAssemblyFromSource(
                new System.CodeDom.Compiler.CompilerParameters()
                {
                    GenerateInMemory = true, // note: this is not really "in memory"
                    ReferencedAssemblies = { "System.dll", "System.Core.dll" } // for linq
                },
                program
            );

            res.TempFiles.KeepFiles = false; //clean up files in temp folder

            // Print any errors with the source code and line numbers
            if (res.Errors.HasErrors)
            {
                int cnt = 1;
                output = "There is one or more errors in the template code:\r\n";
                foreach (System.CodeDom.Compiler.CompilerError err in res.Errors)
                    output += "[Line " + err.Line + " Col " + err.Column + "] " + err.ErrorText + "\r\n";
                output += "\r\n================== Source (for debugging) =====================\r\n";
                output += "     0         10        20        30        40        50        60\r\n";
                output += "   1| " +System.Text.RegularExpressions.Regex.Replace(prog.ToString(), "\r\n",
                    m => { cnt++; return "\r\n" + cnt.ToString().PadLeft(4) + "| "; });
                return false;
            }

            //////////////// Step 3 - Run the program to collect the output ////////////////
            var type = res.CompiledAssembly.GetType("T44Class");
            var obj = System.Activator.CreateInstance(type);
            output = (string)type.GetMethod("Execute").Invoke(obj, new object[] { });
            return true;
        }   
    }
}

