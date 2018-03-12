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


    public abstract class VaultDistributedLoad : DistributedLoad, IDiscretizableLoad<Vault>
    {
        protected VaultDistributedLoad(Func<double, Vector3D> load, UnitVector3D axis, double start, double end)
        {
            Axis = axis;
            Force = p => load(p.ToVector3D().DotProduct(Axis));
            Boundaries[Axis] = new Boundaries(start, end);
        }

        protected UnitVector3D Axis;

        public List<PointLoad> DiscretizeForce(Func<double, double> force, Vault structure)
        {
            var res = new List<PointLoad>();
            Boundaries bound = Boundaries[Axis];
            for (int i = 0; i < structure.Points.MidSegment.Count; i++)
            {
                double coord_prev = structure.Points.Segment[i].ToVector3D().DotProduct(Axis);
                double coord_next = structure.Points.Segment[i + 1].ToVector3D().DotProduct(Axis);
                
                if (coord_next >= bound.Min && coord_prev <= bound.Max)
                {
                    var point = structure.Points.MidSegment[i];
                    double min = Max(coord_prev, bound.Min);
                    double max = Min(coord_next, bound.Max);

                    double force_integral = Integrate.OnClosedInterval(force, min, max);

                    if (Abs(force_integral) > Prec)
                    {
                        res.Add(new VaultPointLoad(point, force_integral * Axis));
                    }
                }
            }
            return res;
        }

        public List<PointLoad> ToProjectedPointLoads(Vault structure)
        {
            var res = new List<PointLoad>();
            res.AddRange(DiscretizeForce(x => Force(new Point3D(x, 0, 0)).DotProduct(XAxis) * structure.dL(x), structure));
            res.AddRange(DiscretizeForce(x => Force(new Point3D(x, 0, 0)).DotProduct(ZAxis) * structure.dL(x), structure));
            return res;
        }
    }


    public class VaultDistributedLoadOverX : VaultDistributedLoad
    {
        public VaultDistributedLoadOverX(Func<double, Vector3D> load, double start, double end): 
            base(load, XAxis, start, end)
        {
        }
    }

    public class VaultDistributedLoadOverZ : VaultDistributedLoad
    {
        public VaultDistributedLoadOverZ(Func<double, Vector3D> load, double start, double end) :
            base(load, ZAxis, start, end)
        {
        }
    }


    public class VaultLoadCase : LoadCase<Vault>, IVaultResults
	{
		public VaultLoadCase(IStructure structure) : base(structure)
		{
		}

		public IVaultResults Results
		{
			get
			{
				return this;
			}
		}

		double IVaultResults.IntM { get; set; } = 0;

		double IVaultResults.IntMx { get; set; } = 0;

		double IVaultResults.IntMy { get; set; } = 0;

		double IVaultResults.SumMb { get; set; } = 0;
	}
}
