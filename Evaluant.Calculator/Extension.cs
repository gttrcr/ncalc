﻿using System;
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

            Type[] CommonTypes = new[] { typeof(Int32), typeof(Int64), typeof(Double), typeof(Boolean), typeof(String), typeof(Decimal) };
            if (CommonTypes.Contains(objType))
                return new Complex(Convert.ToDouble(obj), 0);

            throw new ArgumentException("Type not found");
        }
    }
}