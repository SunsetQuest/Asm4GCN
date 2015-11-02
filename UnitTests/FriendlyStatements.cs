using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenClWithGcnNS;
using NOpenCL;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class FriendlyStatements
    {
        [TestMethod]
        public void Run()
        {
            Console.WriteLine("================== " + GetType().Name + "================== ");

            string source = @"
    __asm4GCN myAsmFunc ( float*, float*)
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
    vLocalSize       = groupId      * vLocalSize    // v_mul_i32_i24     vLocalSize, groupId, vLocalSize
    v4u localSizeIdx = laneId       + vLocalSize    // v_add_i32     v4u localSizeIdx, vcc, laneId, vLocalSize
    v4u vGlobalID    = baseGlobalId + localSizeIdx  // v_add_i32     v4u vGlobalID, vcc, baseGlobalId, localSizeIdx
    v4u vGlobalOffset= vGlobalID << 2               // v_lshlrev_b32 v4u vGlobalOffset, 2, vGlobalID
 
    // Fetch the value that we want to multiply by 2.
    s_buffer_load_dword s4u sPara1Ptr, paramsOffset, 0x00         
    s_load_dwordx4 s16u resConst0, resConsts, 0x60
    s_waitcnt     lgkmcnt(0)
    v4b baseOffset = sPara1Ptr + vGlobalOffset      // v_add_i32     v4b baseOffset, vcc, sPara1Ptr, vGlobalOffset
    tbuffer_load_format_x v4b val, baseOffset, resConst0, _F32_
    s_waitcnt     vmcnt(0)                              

    // Perform the 'sum = val * 2'
    v4f sum = 2.0 * val                             // v_mul_f32 v4f sum, 2.0, val

    // Write the results back to memory.
    s_buffer_load_dword s4u param2Offset, paramsOffset, 0x04
    s_waitcnt lgkmcnt(0)
    s_waitcnt vmcnt(0)
    v4u dstOffset = param2Offset + vGlobalOffset    // v_add_i32     v4u dstOffset, vcc, param2Offset, vGlobalOffset
    free param2Offset  // var is freed and register returned to pool.
    tbuffer_store_format_x sum, dstOffset, resConst1, _F32_

    // Exit the kernel
    s_endpgm
    }";


            /************ Initialize OpenClWithGCN    ***************************************/
            OpenClWithGCN gprog = new OpenClWithGCN();
            OpenClEnvironment env = gprog.env;
            bool success = gprog.GcnCompile(source);
            Console.Write(env.lastMessage);
            if (!success)
            {
                Assert.Fail();
                return;
            }

            /************ Create some random data   *******************************************/
            // create some random data for testing
            var random = new Random();
            const int count = 1024 * 1024;
            const int dataSz = count * sizeof(float);
            float[] data = (from i in Enumerable.Range(0, count) select (float)random.NextDouble()).ToArray();

            /************ Build and run the kernel  *******************************************/
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
    }
}
