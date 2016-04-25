using System;
using System.Diagnostics;
using System.Linq;

namespace UnitTests
{
    class Program
    {
        static void Main(string[] args)
        {
            (new DoubleValueUsingOpenCL()).Run();
            (new DoubleValueUsingRegs()).Run();
            (new DoubleValueUsingVars()).Run();
            (new FastWavefrontReductions()).Run();
            (new FriendlyStatements()).Run();
            (new SharedMemory()).Run();
            (new TestCachedPrograms()).Run();
            Console.ReadKey();
        }
    }
}
