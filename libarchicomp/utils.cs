using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;

using MathNet.Spatial.Euclidean;


namespace libarchicomp.utils
{
    public class Constants
    {
        public static double Scope = 1E+3;
        public static double Prec = 1E-6;
    }

    public enum Restraint { Fixed, Pinned };

    public class StaticMethods
    {
        public static double ClosestValue(double x, List<double> list)
        {
            return list.Aggregate(
                (u, v) => Abs(u - x) < Abs(v - x) ? u : v
            );
        }

        public static int ClosestValue(int x, List<int> list)
        {
            return ClosestValue(x, list);
        }


        public static List<double> ValuesBetween(double start, double end, List<double> list)
        {
            return list.SkipWhile(u => u <= start).TakeWhile(u => u <= end).ToList();
        }

        public static IEnumerable<T> accumulate<T>(
            IEnumerable<T> enumerable, 
            Func<T, T, T> func, 
            T init
        ) where T:struct
        {
            T total;
            IEnumerator<T> iter = enumerable.GetEnumerator();
            total = (T)init;
            yield return total;
			while(iter.MoveNext())
            {
                T el = iter.Current;
                total = func(total, el);
                yield return total;
            }
		}
    }

    public static class Point3D_Extensions {
        public static bool IsNaN(this Point3D point)
        {
            return (
                double.IsNaN(point.X) ||
                double.IsNaN(point.Y) ||
                double.IsNaN(point.Z)
            );
        }
    }
}
