// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without any warranty.
using System;
using System.Collections.Generic;
using System.Text;

namespace GcnTools
{
     public struct RegUsage
     {
         public int regSize;
         public int timesUsed;
     }
    
    /// <summary>Keeps a usage count of each register size. It also remembers the maximums and on what line it was on.</summary>
    public class RegUsageCalc : IFormattable
    {
        /// <summary>contains the max register usage</summary>
        public int[] curSizeCts = new int[64];
        /// <summary>This is the max utilization for each particular size</summary>
        public int[] maxSizeCts = new int[64];
        /// <summary>This is first statement where register pressure is highest. This is a good place to try and lower register usage.</summary>
        public Stmt firstStmtWithMostRegs;
        /// <summary>This is the end of the list for maxCurSz.  It is also the largest register used.</summary>
        public int largestSz = 0;
        /// <summary>Should RegUsageCalc calculate worst case mode or very good case mode.  A good case(false) calculation returns a lower bound of registers that are needed. While a worst case(true) would return the worst case.  There are two variables that determin how well we can pack registers.  The first is how many breaks there are in a register space.  In the best case there would be one contigous region of registers.  In this situation we need less registers.  The other end of the spectrum is the bare minum number of registers that are need all broken up. (2 Int32, 3 Doubles would be broken up into 5 different spaces).  When this is the case registers cannot be compacted as well. 
        /// The second main factor is how well variables are moved around (or defragged) to fit in the memory available.  The very good case (false) is when the register space is broken up into the point of the program when most regs are used and the compiler/progrogrammer moves every thing around to fit stuff as best as possible.</summary>
        public bool worstCaseMode = true;

        public RegUsageCalc(bool worstCaseMode)
        {
            this.worstCaseMode = worstCaseMode;
        }

        /// <summary>Adds 1 to the usage count for the needed register size.</summary>
        /// <param name="size">Size of reg in bytes.</param>
        /// <param name="stmt">The statement where a new register is being requested.</param>
        public void AddToCalc(int size, Stmt stmt)
        {
            // grow maxCurSz if needed
            largestSz = Math.Max(size, largestSz);

            curSizeCts[size]++;

            //lets see if there is a free spot with the exact size needed
            if (worstCaseMode)
            {
                if (maxSizeCts[size] < curSizeCts[size])
                {
                    maxSizeCts[size] = curSizeCts[size];
                    firstStmtWithMostRegs = stmt;
                }
            }
            // does curSizeCt fit in current maxSizeCts? if not then grow it
            else
            {
                int[] leftovers = new int[largestSz + 1];
                for (int i = largestSz; i > 0; i--)
                {
                    bool doesNotFit = true;
                    leftovers[i] += maxSizeCts[i] - curSizeCts[i];

                    // lets see if we can borrow from a larger register space
                    for (int j = i; j <= largestSz; j++)
                        if (leftovers[j] > 0)
                        {
                            leftovers[j]--;
                            leftovers[j - i]++;
                            doesNotFit = false;
                            break;
                        }
                    if (doesNotFit)
                    {
                        maxSizeCts[size]++;
                        firstStmtWithMostRegs = stmt;
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// When freeing a register we most deduct from the curSizeCts[]. 
        /// </summary>
        /// <param name="size">Size of reg in bytes.</param>
        public void RemoveFromCalc(int size)
        {
            curSizeCts[size]--;
        }

        /// <summary>Returns a list with each register size and the maximum usage count at one time.</summary>
        public List<RegUsage> GetUsage()
        {
            List<RegUsage> regUsage = new List<RegUsage>();
            for (int i = 0; i <= largestSz; i++)
                if (maxSizeCts[i] > 0)
                    regUsage.Add(new RegUsage { regSize = i, timesUsed = maxSizeCts[i] });
            return regUsage;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= largestSz; i++)
                if (maxSizeCts[i] > 0)
                    sb.AppendLine("[" + i + "] current: " + curSizeCts[i] + " high: " + maxSizeCts[i]);
            return sb.ToString();
        }
    }
}

