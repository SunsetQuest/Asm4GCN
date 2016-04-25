using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenClWithGcnNS;
using NOpenCL;
using System.Linq;

namespace UnitTests
{
    /// <summary>
    /// This example/test makes sure that we can write and then read from shared memory.
    /// </summary>
    [TestClass]
    public class SharedMemory
    {
        [TestMethod]
        public void Run()
        {
            Console.WriteLine("================== " + GetType().Name + "================== ");

            // Create and build a program from our OpenCL-C source code 
            string source = @"__asm4GCN myAsmFunc (float*,float*)
    {
      // ======== Pre-Loaded Registers =========
      // s[2:3]       - UAV table Pointer
      // s[2:3] +0x60 - base_resource_const1(#T)
      // s[2:3] +0x68 - base_resource_const2(#T)
      // s[4:7]       - Imm Const Buffer 0
      // s[4:7] +0x00 - Grid Size
      // s[4:7] +0x04 - Local Size
      // s[4:7] +0x18 - baseGlobalId
      // s[8:11]      - Imm Const Buffer 1
      // s[8:11]+0x00 - param1 offset
      // s[8:11]+0x04 - param2 offset 
      // s[8:11]+0x08 - param3 offset (not used here)
      // s12          - Group ID
      // v0           - Local ID


      s_mov_b32   m0, 0x00010000
      s_movk_i32  s0, 0x0000
      s_movk_i32  s1, 0x0000
      s_movk_i32  s2, 0xffff
      s_mov_b32   s3, 0x08027fac
      v_ashrrev_i32   v1, 31, v0
      s_load_dword    s4, s[8:11], 0x04
      v_lshrrev_b32   v1, 29, v1
      v_add_i32   v1, vcc, v0, v1
      v_and_b32   v1, -8, v1
      v_lshlrev_b32   v2, 2, v0
      //  v_mov_b32 v2, 4
      v_sub_i32   v1, vcc v0 v1
      v_lshlrev_b32   v0, 2, v0
        //v_cvt_f32_i32   v2, v2
      v_lshlrev_b32   v1, 2, v1
      ds_write_b32    v0, v0, v2
      s_waitcnt   lgkmcnt(0)
        v_add_i32   v0, vcc, s4, v0
      ds_read_b32 v1, v1
        v_cmp_gt_u32    vcc, -1, v0
         s_and_saveexec_b64  s[8:11], vcc
      s_waitcnt   lgkmcnt(0)
        buffer_store_dword  v1, v0, s[0:3], 0, offen
                                  
      s_endpgm   
    };";



            // Initialize OpenClWithGCN 
            OpenClWithGCN gprog = new OpenClWithGCN();
            OpenClEnvironment env = gprog.env;  // use the default environment
            bool success = gprog.GcnCompile(source);
            Console.Write(env.lastMessage);
            if (!success) return;

            // Create some random data  
            var random = new Random();
            const int count = 256;
            const int dataSz = count * sizeof(float);
            float[] data = (from i in Enumerable.Range(0, count)
                            select (float)random.NextDouble()).ToArray();

            // Create a kernel from our modProgram   
            Kernel kernel = env.program.CreateKernel("myAsmFunc");

            // Allocate an input and output memory buffers
            Mem input = env.context.CreateBuffer(MemoryFlags.ReadOnly, dataSz);
            Mem output = env.context.CreateBuffer(MemoryFlags.WriteOnly, dataSz);

            // Copy our host buffer of random values to input device buffer 
            env.cmdQueue.EnqueueWriteBuffer(input, true, 0, dataSz, data);

            // Set the arguments to our kernel, and enqueue it for execution 
            kernel.Arguments[0].SetValue(input);
            kernel.Arguments[1].SetValue(output);

            // Enqueue and run the kernel.
            env.cmdQueue.EnqueueNDRangeKernel(kernel, count, 256);

            // Wait until all commands finish.
            env.cmdQueue.Finish();

            // Read back the results.
            int[] results = new int[count];
            env.cmdQueue.EnqueueReadBufferAndWait(output, results);

            // Print information
            //for (int i = 0; i < 256; i++)
            //    Console.WriteLine("Debug: {0}: {1}", i, results[i]);

            // Validate and print a brief summary detailing the results
            int correct = 0;
            for (int i = 0; i < count; i++)
                correct += (results[i] == ((i * 4) % 32)) ? 1 : 0;
            Console.WriteLine("{0} - Computed {1}/{2} correct values!",
                correct == count ? "PASS" : "FAIL", correct, count);

            Assert.AreEqual(correct, count);
        }
    }
}
