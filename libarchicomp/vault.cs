using System;
using System.Collections.Generic;
using System.Linq;

using MathNet.Spatial.Euclidean;
using MathNet.Numerics;
using MathNet.Numerics.RootFinding;

using libarchicomp.structure;
using libarchicomp.utils;
using Constants = libarchicomp.utils.Constants;

namespace libarchicomp.vaults
{
	public interface IVaultPoints : IStructure2DDiscretePoints
	{
		Point3D ElasticCenter { get; }
	}

	public interface IIntegrals
	{
		double XSq { get; }
        double ZSq { get; }
	}

	public interface ICompute
	{
		Point3D ElasticCenter();
		double IntXSq();
		double IntZSq();
	}

	public abstract class Vault : IDiscretizableStructure2D, IVaultPoints, IIntegrals, ICompute
	{
		// Constructors
		public Vault(
			double w,
			double h,
			double d,
			double t,
			int n,
			Restraint restraint
		)
		{
			W = w;
			H = h;
			D = d;
			T = t;
			N = n;
			Restraint = restraint;
		}

		// Properties
		public double W { get; private set; }
		public double H { get; private set; }
		public double D { get; private set; }
		public double T { get; private set; }
		public int N { get; private set; }
		public Restraint Restraint { get; private set; }
		public double L { get { return XToLength(W / 2); } }

		protected double Prec { get { return Constants.Prec; } }

        protected ICompute Compute => this;

        public IStructure2DDiscretCoord Coord => this;

        public IVaultPoints Points => this;

        private Point3D _Start = Point3D.NaN;
        Point3D IStructure2DDiscretePoints.Start
        {
            get
            {
                if (_Start.IsNaN())
                {
                    _Start = new Point3D(-W / 2, 0, 0);
                }
                return _Start;
            }
        }

        private Point3D _End = Point3D.NaN;
        Point3D IStructure2DDiscretePoints.End
        {
            get
            {
                if (_End.IsNaN())
                {
                    _End = new Point3D(-W / 2, 0, 0);
                }
                return _End;
            }
        }

        private Point3D _ElasticCenter = Point3D.NaN;
        Point3D IVaultPoints.ElasticCenter
        {
            get
            {
                if (_ElasticCenter.IsNaN())
                {
                    _ElasticCenter = Compute.ElasticCenter();
                }
                return _ElasticCenter;
            }
        }

        Point3D ICompute.ElasticCenter()
        {
            double xcoord = Integrate.OnClosedInterval(
                x => x * dL(x) / L,
                -W / 2,
                W / 2,
                Prec
            );
            double zcoord = Integrate.OnClosedInterval(
                x => F(x) * dL(x) / L,
                -W / 2,
                W / 2,
                Prec
            );
            return new Point3D(xcoord, 0, zcoord);
        }

		private List<double> _SegmentX = null;
		List<double> IStructure2DDiscretCoord.SegmentX
		{
			get
			{
				if (_SegmentX == null)
                {
                    _SegmentX =
                        (from i in Enumerable.Range(0, N+1)
						 select LengthToX(L / N* i)).ToList();
				}
                return _SegmentX;
            }
        }

        private List<double> _MidSegmentX = null;
        List<double> IStructure2DDiscretCoord.MidSegmentX
        {
            get
            {
                if (_MidSegmentX == null)
                {
                    _MidSegmentX =
                        (from i in Enumerable.Range(0, N)
                         select LengthToX(L / N * (i + .5))).ToList();
                }
                return _MidSegmentX;
            }
        }

        private List<Point3D> _MidSegment = null;
        List<Point3D> IStructure2DDiscretePoints.MidSegment
        {
            get
            {
                if (_MidSegment == null)
                {
                    _MidSegment =
                        (from x in Coord.MidSegmentX
                         select new Point3D(x, 0, F(x))).ToList();
                }
                return _MidSegment;
            }
        }

        private List<Point3D> _Segment = null;
        List<Point3D> IStructure2DDiscretePoints.Segment
        {
            get
            {
                if (_Segment == null)
                {
                    _Segment = 
                        (from x in Coord.SegmentX
                         select new Point3D(x, 0, F(x))).ToList();
                }
                return _Segment;
            }
        }

        public IIntegrals Integrals => this;

        private double _XSq = double.NaN;
        double IIntegrals.XSq
        {
            get
            {
                if (double.IsNaN(_XSq))
                {
                    _XSq = Compute.IntXSq();
                }
                return _XSq;
            }
        }

        double ICompute.IntXSq()
        {
            Func<double, double> func;
            switch (Restraint)
            {
                case Restraint.Fixed:
                    double Ox = Points.ElasticCenter.X;
                    func = u => Math.Pow(u - Ox, 2) * dL(u);
                    break;

                case Restraint.Pinned:
                    func = u => Math.Pow(u, 2) * dL(u);
                    break;

                default:
                    var msg = "Restraint is unknown";
                    throw new InvalidOperationException(msg);
            }
            return Integrate.OnClosedInterval(func, -W / 2, W / 2, Prec);
        }

        private double _ZSq = double.NaN;
        double IIntegrals.ZSq
        {
            get
            {
                if (double.IsNaN(_ZSq))
                {
                    _ZSq = Compute.IntZSq();
                }
                return _ZSq;
            }
        }

        double ICompute.IntZSq()
        {
            Func<double, double> func;
            switch (Restraint)
            {
                case Restraint.Fixed:
                    double Oz = Points.ElasticCenter.Z;
                    func = u => Math.Pow(F(u) - Oz, 2) * dL(u);
                    break;

                case Restraint.Pinned:
                    func = u => Math.Pow(F(u), 2) * dL(u);
                    break;

                default:
                    var msg = "Restraint is unknown";
                    throw new InvalidOperationException(msg);
            }
            return Integrate.OnClosedInterval(func, -W / 2, W / 2, Prec);
        }


        // Methods
        public abstract double F(double x);

        public virtual double DerivF(double x)
        {
            return Differentiate.FirstDerivative(F, x);
        }

        public virtual double InvF(double z)
        {
            if (z > H)
            {
                return 0;
            }
            return Bisection.FindRoot(x => F(x) - z, -Prec, W / 2 + Prec, Prec);
        }

        public virtual double DerivInvF(double z)
        {
            return Differentiate.FirstDerivative(InvF, z);
        }

        public virtual double XToLength(double x)
        {
            return Integrate.OnClosedInterval(dL, -W / 2, x);
        }

        public virtual double LengthToX(double arcLength)
        {
            return FindRoots.OfFunction(
                x => XToLength(x) - arcLength, 
                -W / 2, 
                W / 2
            );
        }

        public virtual double dL(double x)
        {
            return Math.Sqrt(1 + Math.Pow(DerivF(x), 2));
        }

        public virtual double dLz(double z)
        {
            return Math.Abs(DerivInvF(z) * dL(InvF(z)));
        }
    }

    public class GenericVault : Vault
    {
        public GenericVault(
            Func<double, double> F, 
            double w, 
            double h, 
            double d, 
            double t, 
            int N, 
            Restraint restraint) : 
            base(w, h, d, t, N, restraint)
        {
            _F = F;
        }

        private Func<double, double> _F { get; }

        public override double F(double x)
        {
            return _F(x);
        }
    }
}
