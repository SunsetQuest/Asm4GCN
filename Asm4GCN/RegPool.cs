// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without any warranty.
using System;

namespace GcnTools
{
    struct Reg
    {
        /// <summary>This registers physical register number. This is not always contiguous.</summary>
        public int regNo;
        /// <summary>The beginning of the current contiguous set.</summary>
        public int beg;
        /// <summary>The last register in the current contiguous set.</summary>
        public int end { get { return beg + size - 1; } }
        /// <summary>The size of the current contiguous set.</summary>
        public int size;
        /// <summary>this is the serial id for the variable</summary>
        public uint usageSerialNumber;
    }

    /// <summary>RegPool stores an array of currently available registers. RegPool is initialized by a consecutive range of registers or by supplying a list of registers to work with.</summary>
    public class RegPool
    {
        /// <summary>Each slot is a 4 byte register.</summary>
        Reg[] slots;
        uint usageSerialNumber = 0;
        Log log;

        /// <summary>
        /// Initialize an empty Reg Pool. By default it will use registers 0-127 if no #v_pool/#s_pool is specified.
        /// </summary>
        /// <param name="log">An output log where warning/errors should write written.</param>
        public RegPool(Log log)
        {
            this.log = log;
        }


        /// <summary>Initialize a new RegPool with a contiguous register space.</summary>
        /// <param name="startAt">The starting register number.</param>
        /// <param name="count">The number of registers.</param>
        /// <param name="log">An output log where warning/errors should write written.</param>
        public RegPool(int startAt, int count, Log log)
        {
            InitializeContiguousSpace(startAt, count);
            this.log = log;
        }

        // 
        /// <summary>
        /// Initialize a new RegPool with a given set of allowed registers to use.
        /// </summary>
        /// <param name="regs">The register numbers that can be used.</param>
        /// <param name="log">An output log where warning/errors should write written.</param>
        public RegPool(int[] regs, Log log)
        {
            this.log = log;
            //Summary: 1) get list of free regs 
            //         2) create AdjacentRegs with contiguous regs except split out first reg if odd.

            Array.Sort(regs);

            slots = new Reg[regs.Length];

            // 0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15
            // - - F R - F R R - R -  -  R  R  R  R


            int contCt = 0;     // This is the number of contiguous registers in a row we have.
            int startedAt = 0;  // This is the beginning of the current contiguous set.
            int lastReg = -1;   // This is just for duplicate (two in a row) checking
            for (int i = 0; i < regs.Length; i++)
            {
                int reg = regs[i];
                slots[i].regNo = reg;


                // Check for duplicates
                if (lastReg == reg)
                {
                    log.Warning("In #(S,V)_POOL reg {0} is listed more then once. (ignoring)", reg);
                    continue;
                }
                lastReg = reg;

                // if starting a new contiguous set update startedAt 
                if (contCt == 0)
                    startedAt = reg;
                contCt++;

                // if ending a contiguous set isLastInSet
                bool isLastInSet;
                if (i + 1 == regs.Length)
                    isLastInSet = true;
                else if (i + 2 == regs.Length)
                    isLastInSet = regs[i + 1].IsEven();
                else
                    isLastInSet = regs[i + 1] != reg + 1
                        | (regs[i + 2] != reg + 2 && regs[i + 1].IsEven());

                
                // if "starting and odd" or "last and even" then split out register
                if ((contCt == 1 && reg.IsOdd()) | (isLastInSet && reg.IsEven())) 
                {
                    slots[i].size = 1;
                    slots[i].beg = reg;
                    contCt = 0;
                }
                else if (isLastInSet)// Back-fill if last in array or last in slot set
                {
                    for (int j = contCt - 1; j >= 0; j--)
                    {
                        slots[i - j].size = contCt;
                        slots[i - j].beg = startedAt;
                    }
                    contCt = 0;
                }
            }
        }

        /// <summary>Performs a binary search to find the index of the needed register number.</summary>
        /// <param name="regNum">The register number to search for.</param>
        /// <returns>Returns the index of match or -1 if the value was not found.</returns>
        private int GetRegIndex(int regNum)
        {
            IntializedIfNeeded();

            int hi = slots.Length - 1;
            int lo = 0;

            while (true)
            {
                int mid = (hi + lo) / 2;

                if (slots[mid].regNo > regNum)
                    hi = mid-1; 
                else if (slots[mid].regNo < regNum)
                    lo = mid+1;
                else 
                    return mid;

                if (hi - lo < 2)
                {
                    if (slots[hi].regNo == regNum)
                        return hi;
                    else if (slots[lo].regNo == regNum)
                        return lo;
                    else
                        return -1;

                }
            }
        }

        // 0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15
        // - - F R - F R R - R -  -  R  R  R  R

        // Reserves a given number of consecutive registers
        /// <summary>
        /// Reserves one or more consecutive registers from a reg pool. This function will try to find a 
        /// good location by fitting the register(s) in the smallest contiguous location.
        /// </summary>
        /// <param name="sizeNeeded">This is the number contiguous physical registers needed. The size in words.</param>
        /// <returns>The register number the variable was assigned to or -1 if unable to reserve.</returns>
        public int ReserveRegs(int sizeNeeded, int alignment)
        {
            if (sizeNeeded <= 0)
            {
                log.Error("A register ({0}) cannot be reserved start with a size less then 1.", sizeNeeded);
                return -1;
            }
            
            IntializedIfNeeded();

            // score of 1001(best) if unused and exact fit in contiguous space
            // score of (1000-bestFreeSlotSize+sizeNeeded) if fits anywhere
            // score of -1 if will not fit           

            //int bestSz = 0;
            int bestBegIdx = -1;
            int bestScore = 0;
            int curSz = 0; //we are walking through free regs, this is the current size of the contiguous set
            int curBegIdx = 0; //we are walking through free regs, this is the start of the current contiguous set
            bool aligned = (slots[0].regNo % alignment == 0);

            for (int i = 0; i < slots.Length; i++)
            {
                curSz++;

                if (slots[i].usageSerialNumber > 0)
                {
                    curBegIdx = i + 1;
                    curSz = 0;
                    aligned = false;
                    continue;
                }

                aligned |= (slots[i].regNo % alignment == 0);
                if (!aligned)
                {
                    curBegIdx = i + 1;
                    // curSz = 0;
                    continue;
                }

                if (slots[i].end == slots[i].regNo  // if this is last reg in local set, then this is last
                    || slots[i + 1].usageSerialNumber > 0) // if next is used, then this is last
                    //|| curSz >= sizeNeeded + 16)  // for performance, lets stop around with a free chunk with a size of 16 instead of to 128
                {
                    if (curSz >= sizeNeeded)
                    {
                        // if exact size of reg then score 1000 and return
                        if (sizeNeeded == slots[i].size)
                        {
                            bestBegIdx = curBegIdx;
                            break; // winner found, this is the best case so stop looking
                        }

                        //// it seems like we hit the ending free space, for performance lets just stop here and not go to 128.
                        //if (curSz >= sizeNeeded + 16)
                        //{
                        //    bestBegIdx = curBegIdx;
                        //    break;
                        //}

                        // if size fits in partial used slot
                        int fitScore = 1000 - curSz + sizeNeeded;
                        if (fitScore > bestScore)
                        {
                            //bestSz = curSz;
                            bestBegIdx = curBegIdx;
                            bestScore = fitScore;
                        }
                    }
                    curBegIdx = i + 1;
                    curSz = 0;
                    aligned = false;
                }

            }
            

            // only reserve the regs if a space was found
            if (bestBegIdx >= 0)
            {
                usageSerialNumber++;
                for (int i = bestBegIdx; i < bestBegIdx + sizeNeeded; i++)
                    slots[i].usageSerialNumber = usageSerialNumber;

                //Console.WriteLine(slots[bestBegIdx].regNo);
                return slots[bestBegIdx].regNo;
            }
            else
            {
                log.Error("Out of free register space. Unable to reserve register space.");
                return -1;
            }
        }


        /// <summary>
        /// Marks registers as being used if they are in the pool. If they are not in the pool then -1 is returned. 
        /// </summary>
        /// <param name="regNum">The first register number to mark as used.</param>
        /// <param name="sizeNeeded">This is the number contiguous physical registers needed. The size in words.</param>
        /// <returns>The register number the variable was assigned to or -1 if unable to reserve.</returns>
        public int ReserveSpecificRegs(int regNum, int sizeNeeded)
        {
            //make sure multi-register reservations do not start with an odd number
            if (sizeNeeded > 1 && regNum.IsOdd())
            {
                log.Error("A multi-register reservation (for reg{0}) cannot start with an odd number register.", regNum);
                return -1; 
            }

            //if (sizeNeeded <= 0)
            //{
            //    log.Error("A register ({0}) cannot be reserved start with a size less then 1.", sizeNeeded);
            //    return -1; 
            //}

            IntializedIfNeeded(); 

            int index = GetRegIndex(regNum);

            if (index < 0)
            {
                log.Error("The register {0} is not in the allowed register space.", regNum);
                return -1; //the regNum was not found in the RegPool so no need to reserve it.
            }

            if (slots[index].end - regNum + 1 < sizeNeeded)
            {
                log.Error("The tail of register {0} is past the end of the register space.", regNum);
                return -1; //the regNum was only partially found in the RegPool so we will not to reserve it.
            }

            usageSerialNumber++;
            for (int j = index; j < index + sizeNeeded; j++)
                if (slots[j].usageSerialNumber == 0)
                    slots[j].usageSerialNumber = usageSerialNumber;
                else
                {
                    log.Error("Unable to use register {0} with a size of {1} because register {2} is already in use.", regNum, sizeNeeded, slots[j].regNo);
                    break;
                }

            return index;
        }


        /// <summary>
        /// Given a starting register number, it marks the original registers used free.
        /// </summary>
        /// <param name="regId">The register id number.</param>
        public void FreeReg(int regId)
        {
            IntializedIfNeeded();

            int index = GetRegIndex(regId);

            if (index < 0)
            {
                log.Error("'Free' is unable to locate register " + regId + " in the allowed register pool.");
                return;
            }

            // Starting with the first slot of the matching regNum to when the userID changes, fill userID with 0
            uint id = slots[index].usageSerialNumber;
            for (int j = index; j < slots.Length; j++)
                if (slots[j].usageSerialNumber == id)
                    slots[j].usageSerialNumber = 0;
                else
                    return;
        }


        private void IntializedIfNeeded()
        {
            if (slots == null)
            {
                InitializeContiguousSpace(0, 128);
                //log.Info("Initializing with default reg space from 0 to 127.");
            }
        }


        private void InitializeContiguousSpace(int startAt, int count)
        {
            slots = new Reg[count];
            for (int i = 0; i < count; i++)
                slots[i] = new Reg() { regNo = startAt + i, beg = startAt, size = count };
        }
    }
}


// For testing...
// 0 1 2 3 4 5 6 7 8 9 10 11 12 13 14
// - - F R - F R R - R -  -  R  R  R
//RegPool regPool = new RegPool(new int[] { 2, 3, 5, 6, 7, 9, 12, 13, 14 });
//regPool.ReserveSpecificReg(4, 2, log);
//int regA2 = regPool.ReserveRegs(2, log);
//int regB1 = regPool.ReserveRegs(1, log);
//int regC1 = regPool.ReserveRegs(1, log);
//regPool.FreeReg(regA2, log);
//int regD2 = regPool.ReserveRegs(4, log);
           