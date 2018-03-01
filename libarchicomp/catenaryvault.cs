using System;
using System.Diagnostics;

using MathNet.Spatial.Euclidean;
using MathNet.Numerics;
using static MathNet.Numerics.Trig;

using libarchicomp.utils;
using Constants = libarchicomp.utils.Constants;


namespace libarchicomp.vaults
{
    internal interface ICatenary
    {
        double a { get; }
        double ComputeCoeff();
    }

    public class CatenaryVault : Vault, ICompute, ICatenary
    {
        // Constructors
        public CatenaryVault(
            double w, 
            double h, 
            double d, 
            double t, 
            int N, 
            Restraint restraint) : base(w, h, d, t, N, restraint)
        {
        }

        // Properties

        private double scope { get { return Constants.Scope; } }

        private ICatenary Catenary
        {
            get
            {
                return this;
            }
        }

        private double _a = double.NaN;
        public double a
        {
            get
            {
                if (double.IsNaN(_a))
                {
                    _a = Catenary.ComputeCoeff();
                }
                return _a;
            }
        }

        double ICatenary.ComputeCoeff()
        // #TODO: refine bounds?
        {
            double[] bound = { w / scope, w * scope };
            Debug.Assert(F(w / 2, bound[0]) * F(w / 2, bound[1]) < 0);
            return FindRoots.OfFunction(
                a => F(w / 2, a),
                bound[0],
                bound[1],
                Prec
            );
        }

        /* Methods */

        public double F(double x, double a)
        {
            return h + a * (1 - Cosh(x / a));
        }

        public override double F(double x)
        {
            return F(x, a);
        }

        public override double DerivF(double x)
        {
            return -Sinh(x / a);
        }

        public override double XToLength(double x)
        {
            return a * (Sinh(w / 2 / a) + Sinh(x / a));
        }

        public override double LengthToX(double len)
        {
            return a * Asinh((len - a * Sinh(w / 2 / a)) / a);
        }

        /* End methods */

        Point3D ICompute.ElasticCenter()
        {
            double ycoord = ((h + a) * L - a / 2 * (a * Sinh(w / a) + w)) / L;
            return new Point3D(0, ycoord, 0);
        }

        double ICompute.IntXSq()
        {
            return Math.Pow(w, 2) * L / 4 - 2 * a * (w * (h + a) - a * L);
        }

        double ICompute.IntYSq()
        {
            Func<double, double> func = u =>
                Math.Pow(h + a - u, 2) * L
                - a * (h + a - u) * (a * Sinh(w / a) + w)
                + Math.Pow(a, 2) * L
                + Math.Pow(L, 3) / 12;
            switch (restraint)
            {
                case Restraint.Fixed:
                    double Oy = Points.ElasticCenter.Y;
                    return func(Oy);

                case Restraint.Pinned:
                    return func(0);

                default:
                    var msg = "Restraint is unknown";
                    throw new InvalidOperationException(msg);
            }
        }
    }
}
