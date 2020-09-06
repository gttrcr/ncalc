using System;
using System.Linq;
using System.Numerics;

namespace NCalc
{
    public static class Extension
    {
        public static Complex ToComplex(this object obj)
        {
            Type objType = obj.GetType();
            if (objType == typeof(Complex))
                return (Complex)obj;

            Type[] CommonTypes = new[] { typeof(Int32), typeof(Int64), typeof(Double), typeof(Boolean), typeof(String), typeof(Decimal), typeof(Single) };
            if (CommonTypes.Contains(objType))
                return new Complex(Convert.ToDouble(obj), 0);

            throw new ArgumentException("Type not found");
        }

        public static Complex Round(this Complex c, int decimals)
        {
            return new Complex(Math.Round(c.Real, decimals), Math.Round(c.Imaginary, decimals));
        }

        public static double Range(this Random rnd, double min, double max)
        {
            return min + (rnd.NextDouble() * (max - min));
        }
    }
}