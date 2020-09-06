using System;
using System.Numerics;
using static NCalc.NumericalIntegration;

namespace NCalc
{
    public class MultiNumericalIntegration
    {
        private readonly string _strExpression;
        private readonly string[] _variables;
        private readonly Expression _expression;
        private readonly int _variablesNumber;

        private Complex f(params double[] args)
        {
            for (int i = 0; i < _variablesNumber; i++)
                _expression.Parameters[_variables[i]] = args[i];

            return _expression.Evaluate().ToComplex();
        }

        public MultiNumericalIntegration(string expression, params string[] variables)
        {
            _strExpression = expression;
            _variables = variables;
            _variablesNumber = _variables.Length;
            _expression = new Expression(_strExpression, EvaluateOptions.IgnoreCase);
        }

        public Complex Calculate(params object[] args)
        {
            bool negative = false;
            double value;

            //Check position of intervals
            for (int i = 0; i < args.Length - 1; i += 2)
            {
                if ((args[i].GetType() == typeof(Interval) && (Interval)args[0] == Interval.Infinity) ||
                    (args[i + 1].GetType() == typeof(Interval) && (Interval)args[1] == Interval.MinusInfinity) ||
                    (double.TryParse(args[i].ToString(), out value) && double.TryParse(args[i + 1].ToString(), out double value2) && value > value2))
                {
                    negative = !negative;
                    object tmpArg = args[1];
                    args[i] = args[i + 1];
                    args[i + 1] = tmpArg;
                }
            }

            //Check what integral has infinity interval
            Func<double, double>[] correctionParameter = new Func<double, double>[_variablesNumber];
            Func<double[], Complex> modifiedFunction = p => f(p);
            Tuple<double, double>[] intervalTuple = new Tuple<double, double>[_variablesNumber];
            for (int i = 0; i < 2 * _variablesNumber; i += 2)
            {
                if (args[i].GetType() == typeof(Interval) && (Interval)args[i] == Interval.MinusInfinity &&
                    args[i + 1].GetType() == typeof(Interval) && (Interval)args[i + 1] == Interval.Infinity)
                {
                    correctionParameter[i / 2] = t => (1 + Math.Pow(t, 2)) / Math.Pow(1 - Math.Pow(t, 2), 2);
                    intervalTuple[i / 2] = new Tuple<double, double>(-1, 1);
                }
                else if (args[i].GetType() == typeof(Interval) && (Interval)args[i] == Interval.MinusInfinity &&
                    double.TryParse(args[i + 1].ToString(), out value))
                {
                    correctionParameter[i / 2] = t => 1 / Math.Pow(t, 2);
                    intervalTuple[i / 2] = new Tuple<double, double>(0, 1);
                }
                else if (double.TryParse(args[i].ToString(), out value) &&
                    args[i + 1].GetType() == typeof(Interval) && (Interval)args[i + 1] == Interval.Infinity)
                {
                    correctionParameter[i / 2] = t => 1 / Math.Pow(1 - t, 2);
                    intervalTuple[i / 2] = new Tuple<double, double>(0, 1);
                }
                else if (double.TryParse(args[i].ToString(), out value) && double.TryParse(args[i + 1].ToString(), out double value2))
                {
                    intervalTuple[i / 2] = new Tuple<double, double>(value, value);
                }
            }

            //Calculate volume
            double volume = 1;
            for (int i = 0; i < _variablesNumber; i++)
                volume *= (intervalTuple[i].Item2 - intervalTuple[i].Item1);

            //Monte Carlo Integration
            Random rnd = new Random();
            Complex sum = 0;
            double N = 0;
            double[] rndVector = new double[_variablesNumber];
            for (int i = 0; i < 1000; i++)
            {
                for (int v = 0; v < _variablesNumber + 1; v++)
                    rndVector[v] = rnd.Range(intervalTuple[v].Item1, intervalTuple[v].Item2);

                Complex correction = 1;
                for (int v = 0; v < _variablesNumber - 1; v++)
                    correction *= correctionParameter[v].Invoke(rndVector[v]);

                correction *= modifiedFunction.Invoke(rndVector);
            }

            Complex result = sum * volume / N;
            result *= negative ? (-1d) : (1d);

            return result;
        }
    }
}