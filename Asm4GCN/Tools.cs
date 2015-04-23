// Asm4GCN Assembler by Ryan S White (sunsetquest) http://www.codeproject.com/Articles/872477/Assembler-for-AMD-s-GCN-GPU
// Released under the Code Project Open License (CPOL) http://www.codeproject.com/info/cpol10.aspx 
// Source & Executable can be used in commercial applications and is provided AS-IS without any warranty.
using System;

namespace GcnTools
{
    public static class Extensions
    {
        public static bool IsEven(this int value)
        {
            return value % 2 == 0;
        }

        public static bool IsOdd(this int value)
        {
            return value % 2 == 1;
        }

        /// <summary>
        /// Between check <![CDATA[min <= value <= max]]> 
        /// Source: Hansjörg 2012 http://stackoverflow.com/questions/5023213/is-there-a-between-function-in-c
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">the value to check</param>
        /// <param name="min">Inclusive minimum border</param>
        /// <param name="max">Inclusive maximum border</param>
        /// <returns>return true if the value is between the min and max else false</returns>
        public static bool IsBetweenInc<T>(this T value, T min, T max) where T : IComparable<T>
        {
            return (min.CompareTo(value) <= 0) && (value.CompareTo(max) <= 0);
        }

        /// <summary>
        /// Between check <![CDATA[min <= value <= max]]>
        /// Source: Hansjörg 2012 http://stackoverflow.com/questions/5023213/is-there-a-between-function-in-c
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">the value to check</param>
        /// <param name="min">Exclusive minimum border</param>
        /// <param name="max">Inclusive maximum border</param>
        /// <returns>return true if the value is between the min and max else false</returns>
        public static bool IsBetweenEI<T>(this T value, T min, T max) where T : IComparable<T>
        {
            return (min.CompareTo(value) < 0) && (value.CompareTo(max) <= 0);
        }

        /// <summary>
        /// between check <![CDATA[min <= value <= max]]>
        /// Source: Hansjörg 2012 http://stackoverflow.com/questions/5023213/is-there-a-between-function-in-c
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">the value to check</param>
        /// <param name="min">Inclusive minimum border</param>
        /// <param name="max">Exclusive maximum border</param>
        /// <returns>return true if the value is between the min and max else false</returns>
        public static bool IsBetweenIE<T>(this T value, T min, T max) where T : IComparable<T>
        {
            return (min.CompareTo(value) <= 0) && (value.CompareTo(max) < 0);
        }

        /// <summary>
        /// between check <![CDATA[min <= value <= max]]>
        /// Source: Hansjörg 2012 http://stackoverflow.com/questions/5023213/is-there-a-between-function-in-c
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">the value to check</param>
        /// <param name="min">Exclusive minimum border</param>
        /// <param name="max">Exclusive maximum border</param>
        /// <returns>return true if the value is between the min and max else false</returns>
        public static bool IsBetween<T>(this T value, T min, T max) where T : IComparable<T>
        {
            return (min.CompareTo(value) < 0) && (value.CompareTo(max) < 0);
        }
    }
}
