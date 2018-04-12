using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

using MathNet.Numerics;
using MathNet.Spatial.Euclidean;
using static MathNet.Spatial.Euclidean.UnitVector3D;

using static libarchicomp.utils.StaticMethods;
using libarchicomp.structure;
using libarchicomp.loadcase;

namespace libarchicomp.vaults
{
	public interface IVaultResults : IResults
	{
		double IntM { get; set; }
		double IntMx { get; set; }
		double IntMy { get; set; }
		double SumMb { get; set; }
	}

    public class VaultPointLoad : PointLoad, IDiscretizableLoad<Vault>
	{
        public VaultPointLoad(double x, Vector3D force): base(new Point3D(x, 0, 0), force)
		{
		}

        public VaultPointLoad(Point3D loc, Vector3D force) : base(loc, force)
        {
        }

        public IDiscretizableLoad<Vault> Load => this;


        List<PointLoad> IDiscretizableLoad<Vault>.ToProjectedPointLoads(Vault structure)
        {
            double loc = ClosestValue(Loc.X, (structure as IDiscretizableStructure2D).MidSegmentX);

            var res = new List<PointLoad>();
            foreach(var axis in new UnitVector3D[]{XAxis, ZAxis})
            {
                if (Abs(Force.DotProduct(axis)) > Prec)
                {
                    res.Add(
                        new VaultPointLoad(
                            new Point3D(loc, 0, structure.F(loc)),
                            Force.ProjectOn(axis))
                    );
                }
            }
            return res;
        }
	}


    public abstract class VaultDistributedLoad : DistributedLoad
    {
        protected VaultDistributedLoad(Func<double, Vector3D> load, UnitVector3D axis, double start, double end)
        {
            Axis = axis;
            _Force = p => load(p.ToVector3D().DotProduct(Axis));
            Boundaries[Axis] = new Boundaries(start, end);
        }

        protected UnitVector3D Axis;

        public Vector3D Force(double x)
        {
            return _Force((x * Axis).ToPoint3D());
        }

        public List<PointLoad> DiscretizeForce(Func<double, double> force, Vault structure, UnitVector3D projection)
        {
            var res = new List<PointLoad>();
            Boundaries bound = Boundaries[Axis];
            for (int i = 0; i < structure.Points.MidSegment.Count; i++)
            {
                double coord_prev = structure.Points.Segment[i].ToVector3D().DotProduct(Axis);
                double coord_next = structure.Points.Segment[i + 1].ToVector3D().DotProduct(Axis);

                if (coord_prev > coord_next)
                {
                    var temp = coord_next;
                    coord_next = coord_prev;
                    coord_prev = temp;
                }
                
                if (coord_next >= bound.Min && coord_prev <= bound.Max)
                {
                    var point = structure.Points.MidSegment[i];

                    double min = Max(coord_prev, bound.Min);
                    double max = Min(coord_next, bound.Max);
                    double force_integral;

                    if (Abs(min - max) > Prec)
                    {
                        force_integral = Integrate.OnClosedInterval(force, min, max);
                    }
                    else
                    {
                        max = point.ToVector3D().DotProduct(Axis);
                        force_integral = 2 * Integrate.OnClosedInterval(force, min, max);
                    }
                    
                    if (Abs(force_integral) > Prec)
                    {
                        res.Add(new VaultPointLoad(point, force_integral * projection));
                    }
                }
            }
            return res;
        }
    }


    public class VaultDLoadOverX : VaultDistributedLoad, IDiscretizableLoad<Vault>
    {
        public VaultDLoadOverX(Func<double, Vector3D> load, double start, double end) :
            base(load, XAxis, start, end)
        {
        }

        public List<PointLoad> ToProjectedPointLoads(Vault structure)
        {
            var res = new List<PointLoad>();
            res.AddRange(DiscretizeForce(x => Force(x).DotProduct(XAxis), structure, XAxis));
            res.AddRange(DiscretizeForce(x => Force(x).DotProduct(ZAxis), structure, ZAxis));
            return res;
        }
    }


    public class VaultDLoadOverZ : VaultDistributedLoad, IDiscretizableLoad<Vault>
    {
        public VaultDLoadOverZ(Func<double, Vector3D> load, double start, double end) :
            base(load, ZAxis, start, end)
        {
        }

        public List<PointLoad> ToProjectedPointLoads(Vault structure)
        {
            var res = new List<PointLoad>();
            res.AddRange(DiscretizeForce(z => Force(z).DotProduct(XAxis), structure, XAxis));
            res.AddRange(DiscretizeForce(z => Force(z).DotProduct(ZAxis), structure, ZAxis));
            return res;
        }
    }

    public class VaultDLoadOverXByLength : VaultDistributedLoad, IDiscretizableLoad<Vault>
    {
        public VaultDLoadOverXByLength(Func<double, Vector3D> load, double start, double end): 
            base(load, XAxis, start, end)
        {
        }

        string warning =
            "Loads distributed over axis X applied over length of structure may generate inaccuracies " +
            "or errors on steep parts of the structure.";

        public List<PointLoad> ToProjectedPointLoads(Vault structure)
        {
            var res = new List<PointLoad>();
            res.AddRange(DiscretizeForce(x => Force(x).DotProduct(XAxis) * structure.dL(x), structure, XAxis));
            res.AddRange(DiscretizeForce(x => Force(x).DotProduct(ZAxis) * structure.dL(x), structure, ZAxis));
            return res;
        }
    }

    public class VaultDLoadOverZByLength : VaultDistributedLoad, IDiscretizableLoad<Vault>
    {
        public VaultDLoadOverZByLength(Func<double, Vector3D> load, double start, double end) :
            base(load, ZAxis, start, end)
        {
        }

        string warning =
            "Loads distributed over axis Z applied over length of structure may generate inaccuracies " +
            "or errors on flat parts of the structure.";

        public List<PointLoad> ToProjectedPointLoads(Vault structure)
        {
            var res = new List<PointLoad>();
            res.AddRange(DiscretizeForce(z => Force(z).DotProduct(XAxis) * structure.dLz(z), structure, XAxis)); 
            res.AddRange(DiscretizeForce(z => Force(z).DotProduct(ZAxis) * structure.dLz(z), structure, ZAxis)); 
            return res;
        }
    }


    public class VaultLoadCase : DiscreteLoadCase2D<Vault>, IVaultResults
	{
		public VaultLoadCase(Vault structure, IEnumerable<IDiscretizableLoad<Vault>> loadinput) : base(structure, loadinput)
		{
		}

		double IVaultResults.IntM { get; set; } = 0;

		double IVaultResults.IntMx { get; set; } = 0;

		double IVaultResults.IntMy { get; set; } = 0;

		double IVaultResults.SumMb { get; set; } = 0;

        public void integrate()
        {

        }
	}
}
