using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeathCopyEngine.Helpers
{
    public struct MySize
    {
        public long Bytes;
        public double Kb;
        public double Mb;
        public double Gb;
        public double Tb;

        public MySize(long bytes)
        {
            Bytes = bytes;
            Kb=bytes/1024d;
            Mb = Kb / 1024d;
            Gb = Mb / 1024d;
            Tb = Gb / 1024;
        }

        public override string ToString()
        {
            double size = 0;
            var type = "";

            if (Tb >= 1)
            {
                size = Tb;
                type = "Tb";
            }
            else if (Gb >= 1)
            {
                size = Gb;
                type = "Gb";
            }
            else if (Mb >= 1)
            {
                size = Mb;
                type = "Mb";
            }
            else if (Kb >= 1)
            {
                size = Kb;
                type = "Kb";
            }
            else
            {
                size = Bytes;
                type = "Bytes";
            }

            size = Math.Round(size, 2);

            return string.Format("{0:f1} {1}", size, type);
        }
        public override bool Equals(object obj)
        {
            MySize b;
            try
            {
                b = (MySize)obj;
            }
            catch (Exception)
            {
                return false;
            }

            if (b == null) return false;
            return this == b;
        }
        public override int GetHashCode()
        {
            return Bytes.GetHashCode();
        }

        public static MySize operator +(MySize a, MySize b)
        {
            return new MySize(a.Bytes + b.Bytes);
        }
        public static MySize operator +(MySize a, long bytes)
        {
            return new MySize(a.Bytes + bytes);
        }

        public static MySize operator -(MySize a, MySize b)
        {
            var dif=a.Bytes - b.Bytes;
            var size = dif > 0 ? dif : 0;
            return new MySize(size);
        }
        public static MySize operator -(MySize a, long bytes)
        {
            return new MySize(a.Bytes - bytes);
        }
        public static bool operator >(MySize a, MySize b)
        {
            return a.Bytes > b.Bytes;
        }
        public static bool operator <(MySize a, MySize b)
        {
            return a.Bytes < b.Bytes;
        }
        public static bool operator ==(MySize a, MySize b)
        {
            return a.Bytes == b.Bytes;
        }
        public static bool operator !=(MySize a, MySize b)
        {
            return a.Bytes != b.Bytes;
        }

       
        public static bool operator >(MySize a, long b)
        {
            return a.Bytes > b;
        }
        public static bool operator <(MySize a, long b)
        {
            return a.Bytes < b;
        }
        public static bool operator ==(MySize a, long b)
        {
            return a.Bytes == b;
        }
        public static bool operator !=(MySize a, long b)
        {
            return a.Bytes != b;
        }
    }
}
