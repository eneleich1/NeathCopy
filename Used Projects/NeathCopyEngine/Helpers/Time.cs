using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeathCopyEngine.Helpers
{
    public struct Time
    {
        public long Miliseconds;
        public int Seconds;
        public int Minutes;
        public int Hours;
        public long AllMiliseconds;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ms">Miliseconds</param>
        public Time(long ms)
        {
            AllMiliseconds = ms;

            var mh=1000 * 60 * 60;
            var mm = 1000 * 60;
            var mse = 1000;

            long rest = 0;

            Hours = (int)(ms/mh);
            rest = ms % mh;//ms - (Hours * mh);

            Minutes = (int)(rest / mm);
            rest = rest % mm; //Minutes * mm;

            Seconds = (int)(rest / mse);
            rest = rest % mse;//Seconds * mse;

            Miliseconds = rest;
        }

        public static Time operator +(Time a, Time b)
        {
            return new Time(a.Miliseconds + b.Miliseconds);
        }

        public static Time operator -(Time a, Time b)
        {
            var dif = a.Miliseconds - b.Miliseconds;
            var seconds = dif > 0 ? dif : 0;
            return new Time(seconds);
        }

        public override string ToString()
        {
            return string.Format("{0:d2}:{1:d2}:{2:d2}", Hours, Minutes, Seconds);
        }
    }
}
