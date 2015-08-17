using System;
using System.Diagnostics;
using System.Linq;

namespace UnitTests
{
    class Program
    {
        static void Main(string[] args)
        {
            (new DoubleValueUsingRegs()).Run();
            (new DoubleValueUsingVars()).Run();
            (new DoubleValueUsingOpenCL()).Run();
            (new FastWavefrontReductions()).Run();

            Console.ReadKey();
        }
    }
}
