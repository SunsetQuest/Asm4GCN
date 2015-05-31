// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without any warranty.
using NOpenCL;
using System;
using System.Collections.Generic;
using GcnTools;

namespace OpenClWithGcnNS
{
    public class OpenClEnvironment
    {
        public ErrorCode lastError; // Last error

        public string lastMessage;  // Last message

        /// <summary>The host plus a collection of devices managed by the OpenCL framework that allow an application to share resources and execute kernels on devices in the platform.(source: bjurkovski on codeplex)</summary>
        public Platform[] platforms;

        /// <summary>A device is a hardware unit on a system. It can be made up of CPUs, GPUs, and other types of OpenCL compatible devices.</summary>
        public Device[] devices;

        /// <summary>Defines the entire OpenCL environment, including OpenCL kernels, devices, memory management, command-queues, etc.(source: bjurkovski on CodePlex)</summary>
        public Context context;

        /// <summary>the OpenCL command-queue, as the name may suggest, is an object where OpenCL commands are enqueued to be executed by the device. "The command-queue is created on a specific device in a context [...] Having multiple command-queues allows applications to queue multiple independent commands without requiring synchronization." (source: OpenCL Specification and bjurkovski on codeplex)</summary>
        public CommandQueue cmdQueue;

        /// <summary>A Program is a set of kernels, variables, functions, classes, and structures. It can be thought of as a text document that is compiled.  The whole document is compiled together.</summary>
        public Program program;

        public byte[] dummyBin;  //future: should be byte[][] because we have many blocks

        public byte[] patchedBin;  //future: should be byte[][] because we have many blocks

        public int deviceCt;

        public List<AsmBlock> asmBlocks;

        /// <summary> Setup an default environment with to work with.  (optional)</summary>
        public static OpenClEnvironment InitializeWithDefaults()
        {
            OpenClEnvironment env = new OpenClEnvironment();
            env.platforms = Platform.GetPlatforms(); 
            env.devices =  env.platforms[0].GetDevices(DeviceType.Gpu); //todo: get rid of[0]
            env.context = Context.Create(env.devices);
            env.cmdQueue = env.context.CreateCommandQueue(env.devices[0]);
            return env;
        }
    }

    public class GcnVar
    {
        /// <summary>This is the GCN asm data dataType.</summary>
        public string gcnType;

        /// <summary>This is the equivalent c data dataType.</summary>
        public string cppType;

        /// <summary>This is the name of the variable.</summary>
        public string name;
    }

    public class AsmBlock
    {
        /// <summary>The number of parameters in the __asm block header.</summary>
        public int paramCt;

        /// <summary>This is a list of all the parameters for the __asm4GCN block.</summary>
        public List<GcnVar> paramVars = new List<GcnVar>();

        /// <summary>This is the starting location of where the __asm4GCN block was found in the source code.</summary>
        public int locInSource;

        /// <summary>lenInSource is the length of text in the __asm4GCN block.</summary>
        public int lenInSource;

        /// <summary>This is all the GCN Asm extracted from the block.</summary>
        public string codeBlock;

        /// <summary>This is the name of the asm Block.</summary>
        public string funcName;

        /// <summary>This is the binary code result after the Asm4GCN has been compiled.</summary>
        public byte[] bin;

        /// <summary>size of bin in bytes</summary>
        public int binSize;

        /// <summary>This is the deciphered asm statement.</summary>
        public List<GcnStmt> stms;

        /// <summary>This is a list of registers used in the program.</summary>
        public List<RegUsage> sRegUsage, vRegUsage;

        /// <summary>This is the output log. It contains information, warnings, and errors.</summary>
        public string compileLog;

        public int newlineCtInHeader;
    }
}
