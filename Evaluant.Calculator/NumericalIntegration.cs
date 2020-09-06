using MathNet.Numerics.Integration;
using System;
using System.Numerics;

namespace NCalc
{
    public class NumericalIntegration
    {
        private readonly string _strExpression;
        private Expression _expression;

        public enum Interval
        {
            Infinity,
            MinusInfinity
        }

        private Complex f(double x)
        {
            _expression.Parameters["x"] = x;
            return _expression.Evaluate().ToComplex();
        }

        private Complex valueToInf(double x, double value)
        {
            _expression.Parameters["x"] = value + x / (1 - x);
            Complex val = _expression.Evaluate().ToComplex();
            return val / Math.Pow(1 - x, 2);
        }

        private Complex fromMinusInfToValue(double x, double value)
        {
            _expression.Parameters["x"] = value - (1 - x) / x;
            Complex val = _expression.Evaluate().ToComplex();
            return val / Math.Pow(x, 2);
        }

        public NumericalIntegration(string expression)
        {
            _strExpression = expression;
            _expression = new Expression(_strExpression, EvaluateOptions.IgnoreCase);
        }

        public Complex Calculate(params object[] args)
        {
            //Check position of interval
            bool negative = false;
            if (args[0].GetType() == typeof(Interval) && (Interval)args[0] == Interval.Infinity)
            {
                object tmpArg = args[1];
                args[1] = args[0];
                args[0] = tmpArg;
                negative = !negative;
            }

            double real;
            double imaginary;

            //From -infinity...
            if (args[0].GetType() == typeof(Interval) && (Interval)args[0] == Interval.MinusInfinity)
            {
                //...to +infinity
                if (args[1].GetType() == typeof(Interval) && (Interval)args[1] == Interval.Infinity)
                {
                    return Calculate(Interval.MinusInfinity, 0) + Calculate(0, Interval.Infinity);
                }
                //...to value
                else
                {
                    double value = Convert.ToDouble(args[1]);
                    real = GaussLegendreRule.Integrate((x) => fromMinusInfToValue(x, value).Real, 0d, 1d, 1024);
                    imaginary = GaussLegendreRule.Integrate((x) => fromMinusInfToValue(x, value).Imaginary, 0d, 1d, 1024);
                }
            }
            //From value...
            else
            {
                //...to +infinity
                if (args[1].GetType() == typeof(Interval) && (Interval)args[1] == Interval.Infinity)
                {
                    double value = Convert.ToDouble(args[0]);
                    real = GaussLegendreRule.Integrate((x) => valueToInf(x, value).Real, 0d, 1d, 1024);
                    imaginary = GaussLegendreRule.Integrate((x) => valueToInf(x, value).Imaginary, 0d, 1d, 1024);
                }
                //...to value
                else
                {
                    real = GaussLegendreRule.Integrate((x) => f(x).Real, Convert.ToDouble(args[0]), Convert.ToDouble(args[1]), 1024);
                    imaginary = GaussLegendreRule.Integrate((x) => f(x).Imaginary, Convert.ToDouble(args[0]), Convert.ToDouble(args[1]), 1024);
                }
            }

            real *= negative ? (-1d) : (1d);
            imaginary *= negative ? (-1d) : (1d);
            return new Complex(real, imaginary);
        }
    }
}