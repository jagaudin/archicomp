using System;
using System.Linq;
using static System.Math;
using System.Collections.Generic;

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

        public static IEnumerable<T> accumulate<T>(
            IEnumerable<T> enumerable, 
            Func<T, T, T> func, 
            T init
        ) where T:class
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

	public class Vector
	{
		// Constructors
		public Vector(double x, double y, double z=0)
		{
			this.x = x;
            this.y = y;
            this.z = z;
		}

        //Properties
        public double x { get; private set; }
        public double y { get; private set; }
        public double z { get; private set; }
        public double length { get { return Sqrt(x * x + y * y + z * z); } }

        // Operators
        public static Vector operator +(Vector v1, Vector v2)
		{
			return new Vector(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
		}

        // Public methods
        public override string ToString()
        {
            return string.Format("Vector: {0} {1} {2}", x, y, z);
        }
    }


	public class Point : Vector
	{
		// Constructors
		public Point(double x, double y, double z=0) : base(x, y, z) { }

        // Operators
		public static Point operator +(Point p1, Point p2)
		{
			return new Point(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);
		}

        // Public methods
        public double DistanceTo(Point p)
        {
            return Sqrt(Pow(x - p.x, 2) + Pow(y - p.y, 2) + Pow(z - p.z, 2));
        }

        public override string ToString()
        {
            return string.Format("Point: {0} {1} {2}", x, y, z);
        }
    }
}
