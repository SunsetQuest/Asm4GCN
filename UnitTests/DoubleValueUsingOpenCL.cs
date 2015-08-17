using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenClWithGcnNS;
using NOpenCL;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class DoubleValueUsingOpenCL
    {
        [TestMethod]
        public void Run()
        {
            Console.WriteLine("================== " + GetType().Name + "================== ");

            string source = @"
                __kernel void myOpenClFunc ( __global float* cl_input, __global float* cl_output ) 
                { 
                    size_t i = get_global_id(0);
                    cl_output[i] = cl_input[i] + cl_input[i];
                }; ";



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
            Kernel kernel = env.program.CreateKernel("myOpenClFunc");
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
            int correct = Enumerable.Range(0, count).Select(i => results[i] == data[i] * 2).Count();
            Console.WriteLine("{0} - Computed {1}/{2} correct values!",
                correct == count ? "PASS" : "FAIL", correct.ToString(), count.ToString());

            Assert.AreEqual(correct, count);
        }
    }
}
