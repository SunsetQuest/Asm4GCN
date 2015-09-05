// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without any warranty.

using System;
using System.Diagnostics;
using System.Linq;
using NOpenCL;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using GcnTools;
using System.Text;
using System.Management; // for ManagementObjectSearcher

namespace OpenClWithGcnNS
{
    /// <summary>
    /// OpenClWithGCN compiles source code with __asm4GCN blocks into a cl_program.  Work-flow: (1) locates __asmBlocks in source and compiles them (2) replaces __asm4GCN blocks with dummy kernels in source  (3) compiles OpenCL source  (4) finds the binary for the dummy kernels and then replaces them with the _Asm4GCN binaries.
    /// </summary>
    public class OpenClWithGCN
    {
        /// <summary>This contains many of the OpenCl classes and states that are used.  It is encapsulated here so that it is easier to use from new programmers. By default it automatically initiates.</summary>
        public OpenClEnvironment env;

        /// <summary>Stopwatch for tuning code. This is optionally initialized below. If not initialized the times will not show in the log.</summary>
        Stopwatch sw;

        // Setup OpenClEnvironment
        public OpenClWithGCN(bool EnableStopwatch = false)
        {
            if (EnableStopwatch)
                sw = Stopwatch.StartNew();

            env = OpenClEnvironment.InitializeWithDefaults();
        }

        /// <summary>Initialize with an existing OpenClEnvironment.</summary>
        public OpenClWithGCN(OpenClEnvironment env, bool EnableStopwatch = false)
        {
            if (EnableStopwatch)
                sw = Stopwatch.StartNew();

            this.env = env;
        }
                
        /// <summary>
        /// Compiles the source code with _Asm4GCN blocks into a program.
        /// </summary>
        /// <param name="source">The OpenCL Program with _Asm4GCN_ blocks</param>
        /// <returns>returns true if successful </returns>
        public bool GcnCompile(string source)
        {
            StringBuilder log = new StringBuilder();
            if (sw != null) sw.Restart();

            /////////// Step: Lets first do any text templates ///////////
            if (source.Contains("[["))
                if (!T44.Expand(source, out source))
                {
                    env.lastMessage = "ERROR: There is an error in the text templates [[...]]\r\n";
                    return false;
                }
            if (sw != null) log.AppendFormat("T44.Expand {0}ms ->", sw.ElapsedMilliseconds);

            /////////// Step: pull out the Asm4GCN Blocks into asmBlocks ///////////
            bool success = false;
            env.asmBlocks = ExtractAsm4GCNBlocks(ref source, log, out success);
            if (sw != null) log.AppendFormat("ExtractAsm4GCNBlocks {0}ms ->", sw.ElapsedMilliseconds);
            if (!success) { env.lastMessage = log.ToString(); return false; }

            /////////// Step: compile the pulled Asm4GCN blocks to binary and note the byteSize, 
            ///////////        sReg and vReg counts. Blocks are compiled with temporary registers.
            //future: step here for inline

            /////////// Step: compile Asm4GCN Blocks into binary ///////////
            CompileGcnBlocks(log, env.asmBlocks, out success);
            if (sw != null) log.AppendFormat("CompileGcnBlocks {0}ms ->", sw.ElapsedMilliseconds);
            if (!success) { env.lastMessage = log.ToString(); return false; }

            /////////// Step: Replace __Asm Blocks with dummy code (using byteSize, sReg and vReg counts above)
            string sourceWithDummyKernels = ReplaceAsm4GCNBlocksWithDummyKernel(source, env.asmBlocks);
            if (sw != null) log.AppendFormat("ReplaceAsm4GCNBlocksWithDummyKernel {0}ms ->", sw.ElapsedMilliseconds);

            /////////// Step: Create Program From OpenCl source with dummy kernels
            success = CreateBinaryFromOpenClSource(sourceWithDummyKernels, log);
            if (sw != null) log.AppendFormat("CreateBinaryFromOpenClSource {0}ms ->", sw.ElapsedMilliseconds);
            if (!success) { env.lastMessage = log.ToString() + "ERROR: CreateBinaryFromOpenClSource() failed"; return false; }

            /////////// Step: Extract the Binaries for the compiled dummy kernels
            env.dummyBin = env.program.Binaries[0];
            if (sw != null) log.AppendFormat("GetBinariesFromProgram {0}ms ->", sw.ElapsedMilliseconds);
            if (!success) { env.lastMessage = log.ToString() + "ERROR: GetBinariesFromProgram() failed"; return false; }

            /////////// Step: Replace the Dummy binary code with the compiled GCN binary code
            env.patchedBin = new byte[env.dummyBin.Length];
            Buffer.BlockCopy(env.dummyBin, 0, env.patchedBin, 0, env.dummyBin.Length);
            ReplaceDummyBinaryWithAsm4GCNBinary(env.asmBlocks, env.deviceCt, env.patchedBin);
            if (sw != null) log.AppendFormat("Reload the modified Binaries {0}ms ->", sw.ElapsedMilliseconds);

            /////////// Step: reload the modified Binaries 
            byte[][] bins = {env.patchedBin};
            ReloadModifiedBinaries(env.deviceCt, env.patchedBin.Length, bins);
            
            env.lastMessage = log.ToString();

            return true;
        }


        /// <summary>Reload the modified Binaries</summary>
        private void ReloadModifiedBinaries(int deviceCt, int binarySize, byte[][] bins)
        {
            env.program = env.context.CreateProgramWithBinary(bins, env.devices);
            env.program.Build();
        }


        /// <summary>Replace the Dummy binary code with the compiled GCN binary code</summary>
        private void ReplaceDummyBinaryWithAsm4GCNBinary(List<AsmBlock> asmBlocks, int deviceCt, byte[] dummyBinary)
        {
            foreach (AsmBlock asmBlock in asmBlocks)
            {
                // Modify Binaries as needed here
                int start = ByteSearch(dummyBinary, new byte[] { 0xff, 0x02, 0x16, 0x7e, 0x00, 0xc0, 0x9a, 0x78, 0xff, 0x02 }, new byte[] { 0xFF, 0xFF, 0xFB, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, 0) - 0;

                // newer 14.501.1003.0 (11/20/2014): 02FF789AC0007E1602FF (rev: ff 02 16 7e 00 c0 9a 78 ff 02 18 7e 01 c0)
                // older 13.251.9001.0 (4 /23/2014): 02FF789AC0007E1202FF (02 FF 78 9A C0 00 7E 12 02 ff)

                if (start > 0)
                    Buffer.BlockCopy(asmBlock.bin, 0, dummyBinary, start, asmBlock.bin.Length);
                else
                    Console.WriteLine("Unable to find Asm4GCN block {0} - this could be cause by unsupported driver version", asmBlock.funcName);
            }
            if (sw != null) Console.WriteLine("Modify Binaries ms: {0}", sw.ElapsedMilliseconds);
        }

        /// <summary>This is a copy of the last dummy source code so we can see if it changed.</summary>
        private string  last_sourceWithDummy = "";
        /// <summary>This the success result of the cached copy of the last dummy source code.</summary>
        private bool    last_success = false;
        /// <summary>This the success result of the cached copy of the last dummy source code.</summary>
        private Program last_program;
        /// <summary>Create Program From OpenCl source and dummy kernels</summary>
        private bool CreateBinaryFromOpenClSource(string sourceWithDummys, StringBuilder log)
        {
            // already cached? If so, lets use the last program we built
            if (last_sourceWithDummy == sourceWithDummys)
            {
                env.program = last_program;
                return last_success;
            }

            env.program = env.context.CreateProgramWithSource(sourceWithDummys);
            if (sw != null) Console.WriteLine("CreateProgramWithSource ms: {0}", sw.ElapsedMilliseconds);

            try
            {
                env.program.Build();
            }
            catch (Exception x)
            {
                log.AppendFormat("OpenCL Build error: {0}", x.Message);
            }

            if (sw != null) Console.WriteLine("BuildProgram ms: {0}", sw.ElapsedMilliseconds);
            
            BuildStatus bs = env.program.GetBuildStatus(env.program.Devices[0]);
            if (bs != BuildStatus.Success)
                Console.WriteLine("\nError in GetProgramBuildInfo: " + env.program.GetBuildLog(env.program.Devices[0]));

            last_sourceWithDummy = sourceWithDummys;
            last_program = env.program;
            last_success = (bs == BuildStatus.Success);

            return (bs == BuildStatus.Success);
        }


        /// <summary>Replace __asm4GCN Blocks with dummy kernel (using byteSize, sReg and vReg counts above)</summary>
        /// <param name="source">the OpenCl source code with the __asm4GCN blocks</param>
        /// <param name="asmBlocks">This contains information about each __asm4GCN gathered earlier.</param>
        /// <returns>new OpenCl source code with dummy kernels</returns>
        private string ReplaceAsm4GCNBlocksWithDummyKernel(string source, List<AsmBlock> asmBlocks)
        {
            StringBuilder sourceWithDummies = new StringBuilder();
            int cur = 0;
            foreach (AsmBlock blk in asmBlocks)
            {
                sourceWithDummies.Append(source.Substring(cur, blk.locInSource - cur));
                string dummyCode = BuildDummyKernel(blk.funcName, 100, blk.sRegUsage.Count(), blk.binSize / 4, blk.paramCt);
                sourceWithDummies.Append(dummyCode);
                cur = blk.locInSource + blk.lenInSource;
            }
            sourceWithDummies.Append(source.Substring(cur, source.Length - cur));

            return sourceWithDummies.ToString();
        }

        Regex commentSpace = new Regex(@" /\*(.*?)\*/|//(.*?)\r?\n|""((\\[^\n]|[^""\n])*)""|@(""[^""]*"")+", // source Timwi '10 http://stackoverflow.com/questions/3524317/regex-to-strip-line-comments-from-c-sharp 
    RegexOptions.Singleline | RegexOptions.Compiled);

        const string space = @"(\s*?)";//@"(\s*?|(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/)*";   // This regex represents whitespace and/or comments  source: Qtax '12 http://stackoverflow.com/questions/3524317/regex-to-strip-line-comments-from-c-sharp 
        Regex matchAsm4GcnBlock = new Regex(@"
(?<headerArea>" + // catch whole header so we count newlines for error line number alignment
    "__asm4GCN" + space + @"(?<funcName>[a-zA-Z_][a-zA-Z0-9_]*)" + space + @"\(" + space + @"
    (
        " + space + @"
        (?<dataType>(unsigned|signed)?[a-z]{3,6}(\s*\*)?)" + space + @"
        (?<name>|[a-zA-Z_][a-zA-Z0-9_]*)" + space + @",?
    )*
    " + space + @"?\)" + space + @"\{" + space + @"
)
(?<block>[^\}]*?)
\}" + space + @";?", RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>Extracts all __asm4GCN blocks and removes comments from source</summary>
        /// <param name="source">This is the device source code.  I can contain __kernels(OpenCL) and/or __asmBlocks.</param>
        /// <returns>A list of AsmBlocks. An AsmBlock contains the __asmBlock's information and binary code.</returns>
        private List<AsmBlock> ExtractAsm4GCNBlocks(ref string source, StringBuilder log, out bool success)
        {
            success = true; // true if either the Asm4GCNBlock format is incorrect

            // This remove comments
            source = commentSpace.Replace(source,
            me =>
            {
                if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                    return me.Value.StartsWith("//") ? Environment.NewLine : "";
                return me.Value; // Keep the literal strings
            });

            // Decode __asm4GCN block
            MatchCollection matches = matchAsm4GcnBlock.Matches(source);

            List<AsmBlock> asmBlocks = new List<AsmBlock>();

            if (matches.Count == 0)
            {
                log.AppendLine("Info: There were no __asm4GCN blocks found.");
                return asmBlocks;
            }

            foreach (Match m in matches)
            {
                AsmBlock blk = new AsmBlock();
                blk.funcName = m.Groups["funcName"].Value; ;
                blk.codeBlock = m.Groups["block"].Value;
                blk.locInSource = m.Index;
                blk.lenInSource = m.Length;
                blk.paramCt = m.Groups["dataType"].Captures.Count;
                blk.newlineCtInHeader = m.Groups["headerArea"].Value.Split('\n').Length - 1;


                for (int c = 0; c < blk.paramCt; c++)
                {
                    string type = m.Groups["dataType"].Captures[c].Value;
                    string name = m.Groups["name"].Captures[c].Value;

                    if (name == "") 
                        name = "auto_gen_var_name_" + c;
                    else
                        log.AppendLine("WARN: Parameter names are not used in __asm4GCN. Example Use: __asm4GCN myFunc(uint, uint) {...}");

                    type = new string(type.ToCharArray().Where(w => !Char.IsWhiteSpace(w)).ToArray()); // remove whitespace
                    
                    string gcnType = "";
                    if (type.EndsWith("*"))
                        gcnType = "s4u";
                    else
                        switch (type)
                        {
                            case "int": gcnType = "s4i"; break;
                            case "uint": gcnType = "s4u"; break;
                            case "char": gcnType = "s1i"; break;
                            case "float": gcnType = "s4f"; break;
                            case "double": gcnType = "s8f"; break;
                            case "short": gcnType = "s2i"; break;
                            case "ushort": gcnType = "s2u"; break;
                            case "long": gcnType = "s8i"; break;
                            case "ulong": gcnType = "s8u"; break;
                            case "uchar": gcnType = "s1u"; break;
                            case "bool": gcnType = "s4b"; break;
                            case "void": gcnType = "s4u"; break;
                            case "signedint": gcnType = "s4i"; break;
                            case "unsignedint": gcnType = "s4u"; break;
                            case "signedchar": gcnType = "s1i"; break;
                            case "unsighedchar": gcnType = "s1u"; break;
                            case "signedshort": gcnType = "s2i"; break;
                            case "unsighedshort": gcnType = "s2u"; break;
                            case "signedlong": gcnType = "s8i"; break;
                            case "unsighedlong": gcnType = "s8u"; break;
                        
                            default:
                                throw new Exception("ERROR: " + type + " is not a recognized param dataType for an _asm4GCN block.");
                        }
                    blk.paramVars.Add(new GcnVar { cppType = type, gcnType = gcnType, name = name });
                }

                // Count number of lines in the header so the error-line-# reports correctly
                if (blk.newlineCtInHeader < 0)
                {
                    log.AppendLine("ERROR: The GCN function header should be on its own line.");
                    success = false;
                    continue;
                }
                asmBlocks.Add(blk);
            }
            return asmBlocks;
        }

        /// <summary>Assembles each AsmBlock into its binary form.</summary>
        private static bool CompileGcnBlocks(StringBuilder log, List<AsmBlock> asmBlocks, out bool success)
        {

            success = true;
            foreach (AsmBlock blk in asmBlocks)
            {
                string emptyHeaderLines = new String('\n', blk.newlineCtInHeader);

                string Asm4GCNSource = emptyHeaderLines
                    //+String.Concat(from v in blk.paramVars select "#REF "+v.gcnType+" "+v.name+"; ") //future: for inline
                    + blk.codeBlock;

                string[] srcLines = Asm4GCNSource.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);


                GcnBlock Asm4GCN = new GcnBlock();
                bool compileSuccessful;
                blk.bin = Asm4GCN.CompileFull(srcLines, out blk.stms, out blk.sRegUsage, out blk.vRegUsage,
                    out blk.binSize, out blk.compileLog, out compileSuccessful);
                //blk.bin = Asm4GCN.CompileForBin(srcLines, out blk.stms, out blk.binSize, out blk.compileLog, out compileSuccessful);
                success &= compileSuccessful;

                log.Append(blk.compileLog);
            }
            return success;
        }

        string BuildDummyKernel(string funcName, int vRegCt, int sRegCt, int codeSize, int paramCt)
        {
            vRegCt = Math.Max(vRegCt, 15); // 15 minimum so v_mov_b32 is on top
            StringBuilder programSource = new StringBuilder();
            programSource.AppendLine(@"#define VREG_CT " + vRegCt);
            programSource.AppendLine(@"#define SIZE_CT " + codeSize);
            //programSource.AppendLine(@"__attribute__((reqd_work_group_size(64,1,1)))"); // if added then localGroupSizePtr in EnqueueNDRangeKernel must be null
            programSource.Append(@"__kernel void " + funcName + "(");

            for (int i = 0; i < paramCt; i++)
                programSource.Append(@"__global float* i" + i + (i + 1 == paramCt ? "" : ","));

            programSource.AppendLine(@" )
 { 
	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);

	 union {uint i; uint u; float f; uint *up; float *fp;} x;
	//float *f =  (float *)0x28ff44;
	x.u = 0x789ABCDF;
	//x.f= x.fp[0x789ABCDE];  							 // An ID for this kernel; 0- 1000

	// Find inputs");

            //for (int i = 0; i < paramCt; i++)
            //    programSource.Append(@"x.u <<= i" + i + ";");
            //programSource.AppendLine();
            programSource.AppendLine(@"
    x.f /= *i0; x.f /= *i1;
    
	// Process VREG_CT
	uint temp[VREG_CT];
	for (int i = 0; i < VREG_CT; i++)
		temp[i] = 0x789AC000 + i;
	x.u ^= temp[x.u];	 

	x.u &= 0x789AB000;  							 // An ID for this kernel; 0- 1000
	x.u ^= get_local_id(1);
	x.u &= get_global_id(0); 
	x.u |= get_group_id(0); 

	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);
");
            for (int i = 0; i < sRegCt / 4; i++)
                programSource.AppendLine("for(int a=0x789ABCD0;  a=popcount(a);)");

            programSource.AppendLine(@"
	{
		 if (x.u < 0xFFFFFFF0) return; // prevents hang in case we accidentally run this
		
		// Add to kernel's overall size
		if(SIZE_CT>100)
        {
			float tmp = *(float*)&x;
			#pragma unroll 
			for(long i=0; i<SIZE_CT-100; i++)
				tmp = rsqrt(tmp);
			x.f = *(uint*)&tmp;
        }
	}

	mem_fence(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);barrier(CLK_LOCAL_MEM_FENCE | CLK_GLOBAL_MEM_FENCE);

	x.fp[0x0000FFFF] = x.f;
};");
            return programSource.ToString();
        }


        /// <summary>Outputs some general GPU information in text format.</summary>
        public static string CheckGPUAndVersion()
        {
            // Create a list of valid versions. The list must be sorted.
            Version[] testedOK = new Version[]   
            {
                Version.Parse("13.251.9001.0"),
                Version.Parse("14.501.1003.0"),
                Version.Parse("15.200.1062.1004")
            };
            const string recommend = "(Known working with 14.501.1003 or 15.200.1062)";

            StringBuilder msg = new StringBuilder();
            bool anyAmdGpuFound = false;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (ManagementObject mo in searcher.Get())
            {
                string desc = mo.Properties["Name"].Value.ToString();

                //////////////////// Lets check if this is an AMD GPU (not 100% accurate) /////////////////////////
                if (Regex.Match(desc,
                    @"((AMD|ASUS|ATI)\s+(Radeon|RADEON|Mobility\sRadeon|FireGL|FireStream|Firepro|FirePro|R7|R9|HD)[\s\(\d].*)" +
                    @"|(Radeon\s?(\(TM\))?\s?(HD|R5|R7|R9)[\s\(\d].*)" +
                    @"|(ASUS\sEAH6450.*)").Success)
                {
                    anyAmdGpuFound = true;

                    //////////////////// Lets check the Driver version /////////////////////////
                    string driverVer = mo.Properties["DriverVersion"].Value.ToString();
                    Version driver = System.Version.Parse(driverVer);
                    if (testedOK.Contains(driver))
                        msg.AppendFormat("INFO: AMD Driver version, {0}, has been verified as working.\r\n", driverVer);
                    else if ((from t in testedOK where t.Major == driver.Major && t.Minor == driver.Minor select t).Any())
                        msg.AppendFormat("INFO: AMD driver version, {0}, is near a known working version{1}\r\n", driverVer, recommend);
                    else if (driver < testedOK[0])
                        msg.AppendFormat("WARN: AMD driver version, {0}, might be too low (warning below {1}){2}\r\n", driverVer, testedOK[0], recommend);
                    else if (driver > testedOK.Last())
                        msg.AppendFormat("WARN: AMD driver version, {0}, might be too new (warning above {1}){2}\r\n", driverVer, testedOK.Last(), recommend);
                    else
                        msg.AppendFormat("WARN: AMD driver version, {0}, has not been tested {2}\r\n", driverVer, testedOK.Last(), recommend);

                    //////////////////// Lets check the GPU dataType using the Video card name /////////////////////////
                    if (Regex.Match(desc,
                        @"((AMD|ASUS)\s+Radeon\s*(\(TM\))?\s*(HD|R7|R9|)\s*(7[789]\d|818|821|825|824|828|833|837|847|857|867|840|857|867|876|877|886|895|897|899|855|857|859|867|869|873|875|877|879|883|885|887|889|893|895|897|899)0[MDG]?(\s.*)?)" +
                        @"|((AMD|ASUS)\s+(Radeon|R7|R9)\s*?(\(TM\))?\s*?(R[79])?\s+(2[0456789][05]X?)\s.*)" +
                        @"||((AMD|ASUS)\s+Firepro\s*?(\(TM\))?\s*?M(40|41|51|60)00\s.*)").Success)
                    {
                        msg.AppendLine("INFO: Found GPU with GCN - " + desc);
                    }
                    else if (Regex.Match(desc,
                        @"((AMD|ATI)\s+FirePro\s+(2270|2460|A300|3800|V[345789][89]00)\s.*?)" +                 // No FirePro support GCN
                        @"|(AMD FireStream 93[57]0\s.*?)" +                                                     // No FireStream 93xx support GCN
                        @"|((AMD|ATI|VisionTek)\s+(Mobility\s+)?Radeon\s+(HD\s+)?5[0456789]\d0(X2)?[\s/].*?)" + // No 5000 support GCN
                        @"|((AMD|ATI)\s+Radeon\s+(HD\s+)?E?6[2-9]\d0[DGMA]?[\s/].*?)" +                         // No 6000 support GCN
                        @"|((AMD|ATI)\s+Radeon\s+(HD\s+)?7[03456]\d0[DGM]?[\s/].*?)" +                          // No 7000-7600 support GCN
                        @"|((AMD\s+)?Radeon\s+(R5\s+)2[123][05]X?[\s/].*?)").Success)                           // No 200-235 support GCN
                    {
                        msg.AppendLine("ERROR: GPU does not support GCN (" + desc + ")");
                    }
                    else
                    {
                        msg.AppendLine("WARN: AMD GPU Found: (unknown if GPU supports GCN) " + desc);
                    }

                }
            } // end foreach VideoController

            if (!anyAmdGpuFound)
                msg.AppendLine("ERROR: No known AMD GPU found. ");

            return msg.ToString();
        }

        /// <summary>Outputs some general GPU information in text format.</summary>
        public static string GetGPUInfo()
        {
            StringBuilder sb = new StringBuilder();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            int cnt = 0;
            foreach (ManagementObject mo in searcher.Get())
            {
                string desc = mo.Properties["Name"].Value.ToString();
                sb.AppendFormat("[{0}] {1}___________________________\r\n", cnt++, desc);
                sb.AppendFormat("  ClassPath: {0}", mo.ClassPath.ToString());
                if (mo.Options != null)
                    foreach (var o in mo.Options.Context)
                        if (mo.Options != null) sb.AppendFormat("  Context: {0}\r\n", o.ToString());
                if (mo.GetType() != null) sb.AppendFormat("  Type: {0}\r\n", mo.GetType().ToString());
                if (mo.Options != null) sb.AppendFormat("  Options: {0}\r\n", mo.Options.ToString());
                if (mo.Path != null) sb.AppendFormat("  Path: {0}\r\n", mo.Path.ToString());
                if (mo.Qualifiers != null)
                    foreach (var q in mo.Qualifiers)
                        sb.AppendFormat("  Qualifier: {0} -> {1}\r\n", q.Name, q.Value.ToString());
                if (mo.Scope != null) sb.AppendFormat("  Scope: {0}\r\n", mo.Scope.ToString());
                if (mo.Site != null) sb.AppendFormat("  Site: {0}\r\n", mo.Site.ToString());
                if (mo.SystemProperties != null)
                    foreach (var p in mo.SystemProperties)
                        sb.AppendFormat("  SystemProperty: {0} -> {1}\r\n", p.Name, p.Value.ToString());
                foreach (PropertyData pd in mo.Properties)
                    sb.AppendFormat("  {0} --> {1}\r\n", pd.Name, pd.Value);
            }
            return sb.ToString();
        }


        /// <summary>
        /// Modified version of Bilal's search function.
        /// Source: Bilal - http://boncode.blogspot.com/2011_02_01_archive.html  (Nov 2014)
        /// </summary>
        private static int ByteSearch(byte[] searchIn, byte[] searchBytes, byte[] maskBytes, int start = 0)
        {
            bool matched = false;
            // only look at this if we have a populated search array and search bytes with a sensible start
            if (searchIn.Length > 0
                && searchBytes.Length > 0
                && start <= (searchIn.Length - searchBytes.Length)
                && searchIn.Length >= searchBytes.Length)
            {
                // iterate through the array to be searched
                for (int i = start; i <= searchIn.Length - searchBytes.Length; i++)
                {
                    // if the start bytes match we will start comparing all other bytes
                    if ((maskBytes[0] & searchIn[i]) == (maskBytes[0] & searchBytes[0])) 
                    {
                        // multiple bytes to be searched we have to compare byte by byte
                        matched = true;
                        for (int y = 1; y <= searchBytes.Length - 1; y++)
                            if ((maskBytes[y] & searchIn[i+y]) != (maskBytes[y] & searchBytes[y]))
                            {
                                matched = false;
                                break;
                            }

                        if (matched)
                            return i; // everything matched up
                    }
                }
            }
            return -1;
        }
    } // end class OpenClEnvironment
} // end namespace



