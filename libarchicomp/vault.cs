using System;
using System.Collections.Generic;
using System.Linq;

using MathNet.Numerics;

using libarchicomp.structure;
using libarchicomp.utils;
using Constants = libarchicomp.utils.Constants;

namespace libarchicomp.vaults
{
	public interface IVaultPoints : IPoints
	{
		Point ElasticCenter { get; }
	}

	public interface IIntegrals
	{
		double XSq { get; }
		double YSq { get; }
	}

	public interface ICompute
	{
		Point ElasticCenter();
		double IntXSq();
		double IntYSq();
	}

	public abstract class Vault : IStructure, IVaultPoints, IIntegrals, ICompute
	{
		// Constructors
		public Vault(
			double w,
			double h,
			double d,
			double t,
			int N,
			Restraint restraint
		)
		{
			this.w = w;
			this.h = h;
			this.d = d;
			this.t = t;
			this.N = N;
			this.restraint = restraint;
		}

		// Properties
		public double w { get; private set; }
		public double h { get; private set; }
		public double d { get; private set; }
		public double t { get; private set; }
		public int N { get; private set; }
		public Restraint restraint { get; private set; }
		public double L { get { return XToLength(w / 2); } }

		protected double Prec { get { return Constants.Prec; } }

		protected ICompute Compute
		{
			get
			{
				return this;
			}
		}

		public ICoord Coord
		{
			get
			{
				return this;
			}
		}

        public IVaultPoints Points
        {
            get
            {
                return this;
            }
        }

        private Point _Start = null;
        Point IPoints.Start
        {
            get
            {
                if (_Start == null)
                {
                    _Start = new Point(-w / 2, 0);
                }
                return _Start;
            }
        }

        private Point _End = null;
        Point IPoints.End
        {
            get
            {
                if (_End == null)
                {
                    _End = new Point(-w / 2, 0);
                }
                return _End;
            }
        }

        private Point _ElasticCenter = null;
        Point IVaultPoints.ElasticCenter
        {
            get
            {
                if (_ElasticCenter == null)
                {
                    _ElasticCenter = Compute.ElasticCenter();
                }
                return _ElasticCenter;
            }
        }

        Point ICompute.ElasticCenter()
        {
            double xcoord = Integrate.OnClosedInterval(
                x => x * dL(x) / L,
                -w / 2,
                w / 2,
                Prec
            );
            double ycoord = Integrate.OnClosedInterval(
                x => F(x) * dL(x) / L,
                -w / 2,
                w / 2,
                Prec
            );
            return new Point(xcoord, ycoord);
        }

		private List<double> _SegmentX = null;
		List<double> ICoord.SegmentX
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
        List<double> ICoord.MidSegmentX
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

        private List<Point> _MidSegment = null;
        List<Point> IPoints.MidSegment
        {
            get
            {
                if (_MidSegment == null)
                {
                    _MidSegment =
                        (from x in Coord.MidSegmentX
                         select new Point(x, F(x))).ToList();
                }
                return _MidSegment;
            }
        }

        private List<Point> _Curve = null;
        List<Point> IPoints.Curve
        {
            get
            {
                if (_Curve == null)
                {
                    _Curve = new List<Point>();
                    _Curve.Add(Points.Start);
                    _Curve.AddRange(Points.MidSegment);
                    _Curve.Add(Points.End);
                }
                return _Curve;
            }
        }

        public IIntegrals Integrals
        {
            get
            {
                return this;
            }
        }

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
            switch (restraint)
            {
                case Restraint.Fixed:
                    double Ox = Points.ElasticCenter.x;
                    func = u => Math.Pow(u - Ox, 2) * dL(u);
                    break;

                case Restraint.Pinned:
                    func = u => Math.Pow(u, 2) * dL(u);
                    break;

                default:
                    var msg = "Restraint is unknown";
                    throw new InvalidOperationException(msg);
            }
            return Integrate.OnClosedInterval(func, -w / 2, w / 2, Prec);
        }

        private double _YSq = double.NaN;
        double IIntegrals.YSq
        {
            get
            {
                if (double.IsNaN(_YSq))
                {
                    _YSq = Compute.IntYSq();
                }
                return _YSq;
            }
        }

        double ICompute.IntYSq()
        {
            Func<double, double> func;
            switch (restraint)
            {
                case Restraint.Fixed:
                    double Oy = Points.ElasticCenter.y;
                    func = u => Math.Pow(F(u) - Oy, 2) * dL(u);
                    break;

                case Restraint.Pinned:
                    func = u => Math.Pow(F(u), 2) * dL(u);
                    break;

                default:
                    var msg = "Restraint is unknown";
                    throw new InvalidOperationException(msg);
            }
            return Integrate.OnClosedInterval(func, -w / 2, w / 2, Prec);
        }


        // Methods
        public abstract double F(double x);

        public virtual double DerivF(double x)
        {
            return Differentiate.FirstDerivative(F, x);
        }

        public virtual double XToLength(double x)
        {
            return Integrate.OnClosedInterval(dL, -w / 2, x);
        }

        public virtual double LengthToX(double arcLength)
        {
            return FindRoots.OfFunction(
                x => XToLength(x) - arcLength, 
                -w / 2, 
                w / 2
            );
        }

        protected virtual double dL(double x)
        {
            return Math.Sqrt(1 + Math.Pow(DerivF(x), 2));
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
            Restraint restraint) : base(w, h, d, t, N, restraint)
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
