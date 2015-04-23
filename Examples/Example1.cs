using System;
using System.Linq;
using NOpenCL;
using OpenClWithGcnNS;

namespace TestNS
{
    static class Example1
    {
        // The example below is an OpenCL example by Derek Gerstmann(UWA). It's been modified for OpenCL.Net use.
        public static void Run()
        {
            string log;
            ErrorCode err;
            Event lastEvent;
            IntPtr nullPtr;

            /************ Create and build a program from our OpenCL-C source code ***************/
            //const string source = GCN_NS.Code.DevCode;

            string source = @"
                __asm4GCN myAsmFunc ( float*, float*)
                {
                  #define _ZeroInHex_ 0x00
                  #define _32Float_   0 offen format:[BUF_DATA_FORMAT_32,BUF_NUM_FORMAT_FLOAT]
                                                                      // Loc| Binary |BinaryExt
                  s_buffer_load_dword  s0, s[4:7], 0x04               // 000|C2000504|
                  s_buffer_load_dword  s1, s[4:7], 0x18               // 004|C2008518|
                  s_waitcnt     lgkmcnt(0)                            // 008|BF8C007F|
                  s_min_u32     s0, s0, 0x0000ffff                    // 00C|8380FF00|0000FFFF
                  s_buffer_load_dword  s4, s[8:11], _ZeroInHex_       // 014|C2020900|
                  v_mov_b32     v1, s0                                // 018|7E020200|
                  v_mul_i32_i24 v1, s12, v1                           // 01C|1202020C|
                  v_add_i32     v0, vcc, v0, v1                       // 020|4A000300|
                  v_add_i32     v0, vcc, s1, v0                       // 024|4A000001|
                  v_lshlrev_b32 v0, 2, v0                             // 028|34000082|
                  s_load_dwordx4 s[12:15], s[2:3], 0x60               // 02C|C0860360|
                  s_waitcnt     lgkmcnt(0)                            // 030|BF8C007F|
                  v_add_i32     v1, vcc, s4, v0                       // 034|4A020004|
                  tbuffer_load_format_x  v1, v1, s[12:15], _32Float_  // 038|EBA01000|80030101
                  s_buffer_load_dword  s0, s[8:11], 0x04              // 040|C2000904|
                  s_load_dwordx4 s[4:7], s[2:3], 0x68                 // 044|C0820368|
                  s_waitcnt     lgkmcnt(0)                            // 048|BF8C007F|
                  v_add_i32     v0, vcc, s0, v0                       // 04C|4A000000|
                  s_waitcnt     vmcnt(0)                              // 050|BF8C0F70|
                  v_add_f32     v1, v1, v1                            // 054|10020301|
                  tbuffer_store_format_x  v1, v0, s[4:7], _32Float_   // 058|EBA41000|80010100
                  s_endpgm                                            // 060|BF810000|
                };

                // This is not used here but is what generates above when de-assembled.
                // __kernel and __asm4GCN blocks can be used in the same clProgram
                __kernel void myOpenClFunc ( __global float* cl_input, __global float* cl_output ) 
                { 
                    size_t i = get_global_id(0);
                    cl_output[i] = cl_input[i] + cl_input[i];
                }; ";


            /************ Initialize OpenClWithGCN    ***************************************/
            // OpenClEnvironment env = SetupOpenClEnvironment(); // for manual setup
            OpenClWithGCN gprog = new OpenClWithGCN(); 
            OpenClEnvironment env = gprog.env;  // Let just use the built in environment
            bool success = gprog.GcnCompile(source, out log);
            Console.Write(log);
            if (!success) return;

            /************ Create some random data   *******************************************/
            // create some random data for testing
            var random = new Random();
            const int count = 1024 * 1024;
            const int dataSz = count * sizeof(float);
            float[] data = (from i in Enumerable.Range(0, count) select (float)random.NextDouble()).ToArray();

            /************ Create a kernel from our modProgram    *******************************/
            Kernel kernel = Cl.CreateKernel(env.program, "myAsmFunc", out err); //myOpenClFunc myAsmFunc
            if (GetError(err, "CreateKernel()")) return;

            /************ Allocate cl_input, and fill with data ********************************/
            // OpenCL: cl_mem cl_input = clCreateBuffer(context, CL_MEM_READ_ONLY, dataSz, host_ptr, NULL);
            IMem cl_input = Cl.CreateBuffer(env.context, MemFlags.ReadOnly, dataSz, out err);
            if (GetError(err, "CreateBuffer()")) return;

            /************ Create an cl_output memory buffer for our results    *****************/
            // OpenCL: cl_mem cl_output = clCreateBuffer(context, CL_MEM_WRITE_ONLY, dataSz, host_ptr, NULL);
            IMem cl_output = (Mem)Cl.CreateBuffer(env.context, MemFlags.WriteOnly, dataSz, out err);
            if (GetError(err, "CreateBuffer()")) return;

            /************ Copy our host buffer of random values to cl_input device buffer ******/
            // OpenCL: clEnqueueWriteBuffer(cmdQueue, cl_input, CL_TRUE, 0, dataSz, data, 0, NULL, NULL);
            err = Cl.EnqueueWriteBuffer(env.cmdQueue, cl_input, Bool.True, IntPtr.Zero,
                new IntPtr(dataSz), data, 0, null, out lastEvent);
            if (GetError(err, "EnqueueWriteBuffer()")) return;

            /************ Get the max number of work items supported for this kernel/device ****/
            // OpenCL: clGetKernelWorkGroupInfo(kernel,dev,CL_KERNEL_WORK_GROUP_SIZE,sizeof(int),&local,NULL);
            InfoBuffer local = new InfoBuffer(new IntPtr(4));
            err = Cl.GetKernelWorkGroupInfo(kernel, env.devices[0], KernelWorkGroupInfo.WorkGroupSize,
                new IntPtr(sizeof(int)), local, out nullPtr);
            if (GetError(err, "GetKernelWorkGroupInfo()")) return;

            /************ Set the arguments to our kernel, and enqueue it for execution ********/
            // OpenCL Version: clSetKernelArg(kernel, 0, sizeof(cl_mem), &cl_input);
            // OpenCL Version: clSetKernelArg(kernel, 1, sizeof(cl_mem), &cl_output);
            // OpenCL Version: clSetKernelArg(kernel, 2, sizeof(unsigned int), &count);
            err = Cl.SetKernelArg(kernel, 0, new IntPtr(4), cl_input);
            if (GetError(err, "SetKernelArg()")) return;
            err = Cl.SetKernelArg(kernel, 1, new IntPtr(4), cl_output);
            if (GetError(err, "SetKernelArg()")) return;
            IntPtr[] workGroupSizePtr = new IntPtr[] { new IntPtr(count) };
            IntPtr[] localGroupSizePtr = null;//must be null if reqd_work_group_size attribute is used in src
            //IntPtr[] localGroupSizePtr = new IntPtr[] {new IntPtr(256), new IntPtr(1), new IntPtr(1)}; 

            /************ Enqueue and run the kernel *******************************************/
            // OpenCL: clEnqueueNDRangeKernel(cmdQueue, kernel, 1, NULL, &globalMem  &localMem, 0, NULL,NULL);
            err = Cl.EnqueueNDRangeKernel(env.cmdQueue, kernel, 1, null, workGroupSizePtr,
                localGroupSizePtr, 0, null, out lastEvent);
            if (GetError(err, "EnqueueNDRangeKernel()")) return;

            /************ Force command queue to get processed, wait until all commands finish **/
            env.err = Cl.Finish(env.cmdQueue);
            if (GetError(err, "Finish()")) return;

            /************ Read back the results ************************************************/
            // OpenCL: clEnqueueReadBuffer(cmdQueue, cl_output, CL_TRUE, 0, dataSz, results, 0, NULL, NULL); 
            float[] results = new float[count];
            env.err = Cl.EnqueueReadBuffer(env.cmdQueue, cl_output, Bool.True, IntPtr.Zero,
                new IntPtr(dataSz), results, 0, null, out lastEvent);
            if (GetError(err, "EnqueueReadBuffer()")) return;

            /************ Validate our results *************************************************/
            int correct = 0;
            for (int i = 0; i < count; i++)
                correct += (results[i] == data[i] + data[i]) ? 1 : 0;
            int matches = Enumerable.Range(0, 10).Select(i => 100 + 10 * i).ToArray().Count();

            /************ Print a brief summary detailing the results **************************/
            Console.WriteLine("{0} - Computed {1}/{2} correct values!",
                correct==count?"PASS":"FAIL", correct.ToString(), count.ToString());
        }

        private static bool GetError(ErrorCode err, string errorWith = "")
        {
            bool Success = (err == ErrorCode.Success);
            if (!Success)
                Console.WriteLine("ERROR: {0} generated {1}", errorWith, err);
            return (!Success);
        }
    }
}


