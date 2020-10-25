using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace NeathCopyEngine
{
    public static class Extensions
    {
        /// <summary>
        /// Table wich contains all powers of 2 in it's respective index.
        /// Example:
        /// Powers2[0] = 1
        /// Powers2[1] = 2
        /// Powers2[2] = 4
        /// Powers2[3] = 8
        /// ...
        /// </summary>
        public static ulong[] Powers2;
        static Extensions()
        {
            Powers2 = new ulong[64];

            for (int i = 0; i < 64; i++)
                Powers2[i] = (ulong)(Math.Pow(2, i));
        }

        /// <summary>
        /// Cut this string by the specific char count and add '..' string sequence.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="maxCharsCount"></param>
        /// <returns></returns>
        public static string ShortVersion(this string s,int maxCharsCount)
        {
            if (s.Length <= maxCharsCount) return s;

            string res = "";
            int count = 0;

            for (int i = 0; i < s.Length && count<=maxCharsCount; i++)
            {
                res += s[i];
                count++;
            }

            return res + "..";
        }

        /// <summary>
        /// Get the result string by removing all specifics chars
        /// </summary>
        /// <param name="s"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static string RemoveChars(this string s, char[] chars)
        {
            var res = "";

            for (int i = 0; i < s.Length; i++)
            {
                if (!Contained(s[i], chars))
                    res += s[i];
            }

            return res;
        }
        public static bool Contained(char c, char[] chars)
        {
            for (int i = 0; i < chars.Length; i++)
                if (c == chars[i]) return true;

            return false;
        }
        public static long SetHightWord(this long n, int hw)
        {
            return Windef.MAKELONG((uint)hw, 0);
        }
        public static long SetLowWord(this long n, int lw)
        {
            return Windef.MAKELONG(0, (uint)lw);
        }
        /// <summary>
        /// The HIWORD retrieves the high-order word from the given 64-bit value.
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static uint HIWORD(this long l)
        {
            return (uint)(((ulong)l) >> 32);
        }
        /// <summary>
        /// The LOWORD retrieves the low-order word from the given 64-bit value.
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static uint LOWORD(this long l)
        {
            return (uint)((ulong)l);
        }

        //public static long SetHightWord(this long n,int hw)
        //{
        //    /*
        //      * n = _ _ _ ... _ _ _   x x x ... x x x 
        //      *     63            32  31            0         
        //      *                    i 
        //     *     
        //      * hw = y y y ... y y y
        //      *      31            0                                     
        //      *                    j
        //     *                    
        //      * res = y y y ... y y y  x x x ... x x x 
        //     *        63           32 31             0       
        //      */

        //    //Erasee the hiword => copy to 0 the lowword
        //    long res = 0;
        //    byte count = 32;
        //    for (int j = 0; count > 0; j++, count--)
        //    {
        //        if (((uint)n & Powers2[j]) != 0)
        //            res = (long)((ulong)res | Powers2[j]);
        //    }

        //    count = 32;
        //    for (int i = 32, j = 0; count > 0; i++, j++, count--)
        //    {
        //        if (((uint)hw & Powers2[j]) != 0)
        //            res = (long)((ulong)res | Powers2[i]);
        //    }

        //    return res;

        //}
        //public static long SetLowWord(this long n,int lw)
        //{
        //    /*
        //     * n = x x x ... x x x   _ _ _ ... _ _ _ 
        //     *     63            32  31            0         
        //     *                                     i
        //    *     
        //     * lw = y y y ... y y y
        //     *      31            0                                     
        //     *                    j
        //    *                    
        //     * res = x x x ... x x x  y y y ... y y y   
        //    *        63           32 31             0       
        //     */

        //    //Erasee the lowword => copy to 0 the hiword
        //    long res = 0;
        //    byte count = 32;
        //    for (int j = 32; count > 0; j++, count--)
        //    {
        //        if (((uint)n & Powers2[j]) != 0)
        //            res = (long)((ulong)res | Powers2[j]);
        //    }

        //    count = 32;
        //    for (int i = 0, j = 0; count > 0; i++, j++, count--)
        //    {
        //        if (((uint)lw & Powers2[j]) != 0)
        //            res = (long)((ulong)res | Powers2[i]);
        //    }

        //    return res;
        //}

        //public static string ToBits(this long n)
        //{
        //    byte length = 64;
        //    string res = "";

        //    for (int i = length - 1; i >= 0; i--)
        //    {
        //        if ((Powers2[i] & (ulong)n) != 0)
        //            res += '1';
        //        else res += '0';
        //    }

        //    return res;
        //}
    }

    public static class Windef
    {
        public static long MAKELONG(uint hw, uint lw)
        {
            //((LONG)(((WORD)(((DWORD_PTR)(a)) & 0xffff)) | ((DWORD)((WORD)(((DWORD_PTR)(b)) & 0xffff))) << 16))

            return (long)((((ulong)hw) << 32) | ((ulong)lw));
        }
        /// <summary>
        /// The HIWORD retrieves the high-order word from the given 64-bit value.
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static uint HIWORD(long l)
        {
            return (uint)(((ulong)l) >> 32);
        }
        /// <summary>
        /// The LOWORD retrieves the low-order word from the given 64-bit value.
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static uint LOWORD(long l)
        {
            return (uint)((ulong)l);
        }
    }
}
