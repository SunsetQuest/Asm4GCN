using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenClWithGcnNS;
using NOpenCL;
using System.Linq;

namespace UnitTests
{
    /// <summary>
    /// This test makes sure that we can reuse the same OpenClWithGCN and that caching 
    /// does not cause any issues.
    /// </summary>
    [TestClass]
    public class TestCachedPrograms
    {
        [TestMethod]
        public void Run()
        {
            Console.WriteLine("================== " + GetType().Name + "================== ");

            string source = @"
    __asm4GCN myAsmFunc ( float*, float*)
    {
 
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
 
    // Fetch the value that we want to multiply by 2.
    s_buffer_load_dword s4u sPara1Ptr, paramsOffset, 0x00         
    s_load_dwordx4 s16u resConst0, resConsts, 0x60
    s_waitcnt     lgkmcnt(0)
    v_add_i32     v4b baseOffset, vcc, sPara1Ptr, vGlobalOffset
    tbuffer_load_format_x v4b val, baseOffset, resConst0, _F32_
    s_waitcnt     vmcnt(0)                              

    // Perform the 'sum = val * 2'
    v_mul_f32 v4f sum, 2.0, val

    // Write the results back to memory.
    s_buffer_load_dword s4u param2Offset, paramsOffset, 0x04
    s_waitcnt lgkmcnt(0)
    s_waitcnt     vmcnt(0)
    v_add_i32     v4u dstOffset, vcc, param2Offset, vGlobalOffset
    free param2Offset  // var is freed and register returned to pool.
    tbuffer_store_format_x sum, dstOffset, resConst1, _F32_

    // Exit the kernel
    s_endpgm
    }";


            /************ Initialize OpenClWithGCN    ***************************************/
            OpenClWithGCN gprog = new OpenClWithGCN();
            OpenClEnvironment env = gprog.env;

            /************ Create some random data   *******************************************/
            // create some random data for testing
            var random = new Random();
            const int count = 1024 * 1024;
            const int dataSz = count * sizeof(float);
            float[] data = (from i in Enumerable.Range(0, count) select (float)random.NextDouble()).ToArray();

            ///////////////////  ROUND 1  (mult by 2) ////////////////////////////////////
            {
                /************ Build and run the kernel  *******************************************/
                bool success = gprog.GcnCompile(source);
                Console.Write(env.lastMessage);
                if (!success)
                {
                    Assert.Fail();
                    return;
                }
                Kernel kernel = env.program.CreateKernel("myAsmFunc");
                Mem cl_input = env.context.CreateBuffer(MemoryFlags.ReadOnly, dataSz);
                Mem cl_output = env.context.CreateBuffer(MemoryFlags.WriteOnly, dataSz);
                env.cmdQueue.EnqueueWriteBuffer(cl_input, true, 0, dataSz, data);
                kernel.Arguments[0].SetValue(cl_input);
                kernel.Arguments[1].SetValue(cl_output);
                env.cmdQueue.EnqueueNDRangeKernel(kernel, count, 256);
                env.cmdQueue.Finish();

                /************ Read back and Validate the results ***********************************/
                float[] results = new float[count];
                env.cmdQueue.EnqueueReadBufferAndWait(cl_output, results, dataSz);
                int correct = Enumerable.Range(0, count).Where(i => results[i] == data[i] * 2).Count();
                Console.WriteLine("{0} - Computed {1}/{2} correct values!",
                    correct == count ? "PASS" : "FAIL", correct.ToString(), count.ToString());
                Assert.AreEqual(correct, count);
            }

            ///////////////////  ROUND 2  (same kernel) ////////////////////////////////////
            {
                /************ Build and run the kernel  *******************************************/
                bool success = gprog.GcnCompile(source);
                Console.Write(env.lastMessage);
                if (!success)
                {
                    Assert.Fail();
                    return;
                }
                Kernel kernel = env.program.CreateKernel("myAsmFunc");
                Mem cl_input = env.context.CreateBuffer(MemoryFlags.ReadOnly, dataSz);
                Mem cl_output = env.context.CreateBuffer(MemoryFlags.WriteOnly, dataSz);
                env.cmdQueue.EnqueueWriteBuffer(cl_input, true, 0, dataSz, data);
                kernel.Arguments[0].SetValue(cl_input);
                kernel.Arguments[1].SetValue(cl_output);
                env.cmdQueue.EnqueueNDRangeKernel(kernel, count, 256);
                env.cmdQueue.Finish();

                /************ Read back and Validate the results ***********************************/
                float[] results = new float[count];
                env.cmdQueue.EnqueueReadBufferAndWait(cl_output, results, dataSz);
                int correct = Enumerable.Range(0, count).Where(i => results[i] == data[i] * 2).Count();
                Console.WriteLine("{0} - Computed {1}/{2} correct values!",
                    correct == count ? "PASS" : "FAIL", correct.ToString(), count.ToString());

                Assert.AreEqual(correct, count);
            }
            ///////////////////  ROUND 3  (modified kernel - squaring value instead) /////////////////
            {
                source = @"
    __asm4GCN mySquareFunc ( float*, float*, float*)
    {
 
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
 
    // Fetch the value that we want to multiply by 2.
    s_buffer_load_dword s4u sPara1Ptr, paramsOffset, 0x00         
    s_load_dwordx4 s16u resConst0, resConsts, 0x60
    s_waitcnt     lgkmcnt(0)
    v_add_i32     v4b baseOffset, vcc, sPara1Ptr, vGlobalOffset
    tbuffer_load_format_x v4b val, baseOffset, resConst0, _F32_
    s_waitcnt     vmcnt(0)                              

    // Perform a square.
    v_mul_f32 v4f square, val, val

    // Write the results back to memory.
    s_buffer_load_dword s4u param2Offset, paramsOffset, 0x08 //param 3
    s_waitcnt lgkmcnt(0)
    s_waitcnt     vmcnt(0)
    v_add_i32     v4u dstOffset, vcc, param2Offset, vGlobalOffset
    free param2Offset  // var is freed and register returned to pool.
    tbuffer_store_format_x square, dstOffset, resConst1, _F32_

    // Exit the kernel
    s_endpgm
    }";
                /************ Build and run the kernel  *******************************************/
                bool success = gprog.GcnCompile(source);
                Console.Write(env.lastMessage);
                if (!success)
                {
                    Assert.Fail();
                    return;
                }
                Kernel kernel = env.program.CreateKernel("mySquareFunc");
                Mem cl_input = env.context.CreateBuffer(MemoryFlags.ReadOnly, dataSz);
                Mem cl_input2 = env.context.CreateBuffer(MemoryFlags.ReadOnly, dataSz); //not used
                Mem cl_output = env.context.CreateBuffer(MemoryFlags.WriteOnly, dataSz);
                env.cmdQueue.EnqueueWriteBuffer(cl_input, true, 0, dataSz, data);
                kernel.Arguments[0].SetValue(cl_input);
                kernel.Arguments[1].SetValue(cl_input2); //not used
                kernel.Arguments[2].SetValue(cl_output);
                env.cmdQueue.EnqueueNDRangeKernel(kernel, count, 256);
                env.cmdQueue.Finish();

                /************ Read back and Validate the results ***********************************/
                float[] results = new float[count];
                env.cmdQueue.EnqueueReadBufferAndWait(cl_output, results, dataSz);
                int correct=0;
                for (int i = 0; i < count; i++)
                    if (Math.Abs(results[i] - data[i] * data[i]) < 0.000001f)
                        correct++;

                Console.WriteLine("{0} - Computed {1}/{2} correct values!",
                    correct == count ? "PASS" : "FAIL", correct.ToString(), count.ToString());
                
                Assert.AreEqual(correct, count);
            }


        }
    }
}
