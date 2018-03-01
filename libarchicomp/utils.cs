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

        public static double PreviousValue(double x, List<double>list)
        {
            return list.Aggregate(
                (u, v) => (u > v) || (v > x) ? u : v // TODO
            );
        }

        public static int PreviousValue(int x, List<int> list)
        {
            return PreviousValue(x, list);
        }

        public static double NextValue(double x, List<double> list)
        {
            return list.Aggregate(
                (u, v) => (u > v) || (v > x) ? u : v //TODO
            );
        }

        public static int NextValue(int x, List<int> list)
        {
            return NextValue(x, list);
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
