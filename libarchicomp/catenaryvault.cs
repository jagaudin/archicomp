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
        double A { get; }
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
            int n, 
            Restraint restraint) : base(w, h, d, t, n, restraint)
        {
        }

        // Properties

        private double Scope => Constants.Scope;

        ICatenary Catenary => this;

        private double _A = double.NaN;
        public double A
        {
            get
            {
                if (double.IsNaN(_A))
                {
                    _A = Catenary.ComputeCoeff();
                }
                return _A;
            }
        }

        double ICatenary.ComputeCoeff()
        // TODO: refine bounds?
        {
            double[] bound = { W / Scope, W * Scope };
            Debug.Assert(F(W / 2, bound[0]) * F(W / 2, bound[1]) < 0);
            return FindRoots.OfFunction(
                a => F(W / 2, a),
                bound[0],
                bound[1],
                Prec
            );
        }

        /* Methods */

        public double F(double x, double a)
        {
            return H + a * (1 - Cosh(x / a));
        }

        public override double F(double x)
        {
            return F(x, A);
        }

        public override double DerivF(double x)
        {
            return -Sinh(x / A);
        }

        public override double InvF(double z)
        {
            if (z >= 2)
            {
                return 0;
            }
            return A * Math.Log((1+(H-z)/A)+Math.Sqrt(Math.Pow(1 + (H - z) / A, 2)-1));
        }

        public override double DerivInvF(double z)
        {
            if (z >= 2)
            {
                return double.MinValue;
            }
            return - A / Math.Sqrt((H - z) * (2 * A + H -z));
        }

        public override double XToLength(double x)
        {
            return A * (Sinh(W / 2 / A) + Sinh(x / A));
        }

        public override double LengthToX(double arcLength)
        {
            return A * Asinh((arcLength - A * Sinh(W / 2 / A)) / A);
        }

        /* End methods */

        Point3D ICompute.ElasticCenter()
        {
            double zcoord = ((H + A) * L - A / 2 * (A * Sinh(W / A) + W)) / L;
            return new Point3D(0, 0, zcoord);
        }

        double ICompute.IntXSq()
        {
            return Math.Pow(W, 2) * L / 4 - 2 * A * (W * (H + A) - A * L);
        }

        double ICompute.IntZSq()
        {
            Func<double, double> func = u =>
                Math.Pow(H + A - u, 2) * L
                - A * (H + A - u) * (A * Sinh(W / A) + W)
                + Math.Pow(A, 2) * L
                + Math.Pow(L, 3) / 12;
            switch (Restraint)
            {
                case Restraint.Fixed:
                    double Oz = Points.ElasticCenter.Z;
                    return func(Oz);

                case Restraint.Pinned:
                    return func(0);

                default:
                    var msg = "Restraint is unknown";
                    throw new InvalidOperationException(msg);
            }
        }
    }
}
