using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenClWithGcnNS;
using NOpenCL;
using System.Linq;

namespace UnitTests
{
    /// <summary>
    /// A fast wavefront sum reduction using 18 instructions and no shared memory.
    /// </summary>
    [TestClass]
    public class FastWavefrontReductions
    {
        bool printDetails = false;

        [TestMethod]
        public void Run()
        {
            Console.WriteLine("================== " + GetType().Name + "================== ");

            string source = @"__asm4GCN myAsmFunc (float*,float*,float*)
{
  // ======== Pre-Loaded Registers =========
  // s[2:3] - PTR_UAV_TABLE
  // s[2:3] +0x60 - base_resource_const1(#T)
  // s[2:3] +0x68 - base_resource_const2(#T)
  // s[4:7] - IMM_CONST_BUFFER0
  // s[4:7] +0x00 - Grid Size
  // s[4:7] +0x04 - Local Size
  // s[4:7] +0x18 - baseGlobalId
  // s[8:11] -IMM_CONST_BUFFER1
  // s[8:11]+0x00 - param1 offset
  // s[8:11]+0x04 - param2 offset 
  // s[8:11]+0x08 - param3 offset
  // s1     - threadgroupId
  // s12    - groupId
  // v0     - laneId
  
  // Use a #define to shorten tbuffer_load instruction
  #define _F32_ 0 offen format:[BUF_DATA_FORMAT_32,BUF_NUM_FORMAT_FLOAT]
  
  // Variable decelerations with specific registers (pre-loaded regs)
  s8u  resConsts s[2:3]   // base resource constants
  s16u uavBuff   s[4:7], paramsOffset s[8:11], groupId s12
  v4b  laneId    v0       // v0 is pre-loaded with lane id
  
  // Setup Resource Constant
  s_load_dwordx4 s16u resConst1, resConsts, 0x68             
  
  // establish GlobalID
  s_buffer_load_dword s4u baseGlobalId,uavBuff, 0x18 
  s_buffer_load_dword s4u localSize, uavBuff, 0x04  
  s_waitcnt     lgkmcnt(0)                      
  v_mov_b32     v4u vLocalSize, localSize                  
  v_mul_i32_i24     vLocalSize, groupId, vLocalSize           
  v_add_i32     v4u localSizeIdx, vcc, laneId, vLocalSize 
  v_add_i32     v4u vGlobalID, vcc, baseGlobalId, localSizeIdx
  v_lshlrev_b32 v4u vGlobalOffset, 2, vGlobalID

  // create a tool called 'ShowVar' to easily output a variable
  #define _ShowVar_(VarToPrint) \
    v_mov_b32 v4u debugToPrint, VarToPrint;\
    tbuffer_store_format_x debugToPrint, debugOffset,resConst1,_F32_ 
  s_buffer_load_dword s4u param3Offset, paramsOffset, 0x08
  s_waitcnt     lgkmcnt(0)                      
  v_add_i32 v4u debugOffset, vcc, param3Offset, vGlobalOffset
  
  // Fetch the value that we want to multiply by 2.
  s_buffer_load_dword s4u sPara1Ptr, paramsOffset, 0x00         
  s_load_dwordx4 s16u resConst0, resConsts, 0x60
  s_waitcnt     lgkmcnt(0) 
  v_add_i32     v4b baseOffset, vcc, sPara1Ptr, vGlobalOffset
  tbuffer_load_format_x v4b val, baseOffset, resConst0, _F32_
  s_waitcnt     vmcnt(0)                              

  // Fast 64-lane wavefront SUM reduction
  // Product, Avg, Min, Max can also be easily achieved with minor edits.
  v4f tmp;
  [[ for (int i = 2; i <7; i++) {]]
    ds_swizzle_b32 tmp, val, tmp, tmp offset1:[[= 1<<i ]] offset0:0b00011111
    s_waitcnt     lgkmcnt(0)                      
    v_add_f32 val, tmp, val  // can also use v_min_f32, v_max_f32, or v_mul_f32
  [[ } ]]
  v_readfirstlane_b32 s4u sum, val  
  v_add_f32 val, sum, val
  v_readlane_b32 sum, val, 32

  
  // Write the results back to memory.
  v_mov_b32 val, sum
  s_buffer_load_dword s4u param2Offset, paramsOffset, 0x04
  s_waitcnt     lgkmcnt(0)                            
  s_waitcnt     vmcnt(0)                              
  v_add_i32     v4u dstOffset, vcc, param2Offset,vGlobalOffset
  free param2Offset  // var is freed and register returned to pool.
  tbuffer_store_format_x val, dstOffset,resConst1,_F32_ 
  
  _ShowVar_(val)
  
  // Exit the kernel
  s_endpgm                                            
};";


            // https://visualstudiogallery.msdn.microsoft.com/46c0c49e-f825-454b-9f6a-48b216797eb5/view/Reviews/0?showReviewForm=True

            // Initialize OpenClWithGCN 
            OpenClWithGCN gprog = new OpenClWithGCN();
            OpenClEnvironment env = gprog.env;  // use the default environment
            bool success = gprog.GcnCompile(source);
            Console.Write(env.lastMessage);
            if (!success) return;

            // Create some random data  
            var random = new Random(3);
            const int count = 640000;
            const int dataSz = count * sizeof(float);
            float[] data = (from i in Enumerable.Range(0, count)
                            select (float)random.NextDouble()).ToArray();

            // Create a kernel from our modProgram   
            Kernel kernel = env.program.CreateKernel("myAsmFunc");

            // Allocate an input and output memory buffers
            Mem input = env.context.CreateBuffer(MemoryFlags.ReadOnly, dataSz);
            Mem output = env.context.CreateBuffer(MemoryFlags.WriteOnly, dataSz);
            Mem debug = env.context.CreateBuffer(MemoryFlags.WriteOnly, dataSz);

            // Copy our host buffer of random values to input device buffer 
            env.cmdQueue.EnqueueWriteBuffer(input, true, 0, dataSz, data);

            // Set the arguments to our kernel, and enqueue it for execution 
            kernel.Arguments[0].SetValue(input);
            kernel.Arguments[1].SetValue(output);
            kernel.Arguments[2].SetValue(debug);

            // Enqueue and run the kernel.
            env.cmdQueue.EnqueueNDRangeKernel(kernel, count, 256);

            // Wait until all commands finish.
            env.cmdQueue.Finish();

            // Read back the results.
            float[] results = new float[count];
            env.cmdQueue.EnqueueReadBufferAndWait(output, results);
            float[] debugOutput = new float[count];
            env.cmdQueue.EnqueueReadBufferAndWait(debug, debugOutput);

            // Print Debug information
            if (printDetails)
                for (int i = 0; i < 256; i++)
                    Console.WriteLine("Debug: {0}: {1} -> {2}", i, data[i], debugOutput[i]);

            // Validate and print a brief summary detailing the results
            int correct = 0;
            for (int i = 0; i < (count / 64); i++)
            {
                float cpuSum = 0;
                for (int j = 0; j < 64; j++)
                    cpuSum += data[i * 64 + j];

                float gpuSum = results[i * 64];

                if (cpuSum > gpuSum * 0.999999
                    && cpuSum < gpuSum * 1.000001)
                    correct++;
            }
            Console.WriteLine("Computed {0}/{1} correct values!", correct, count / 64);
            Assert.AreEqual(correct, count / 64);
        }
    }
}
