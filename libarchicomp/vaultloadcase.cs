using System;
using System.Collections.Generic;
using System.Linq;

using MathNet.Spatial.Euclidean;

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

	public class VaultPointLoad : PointLoad
	{
		public VaultPointLoad(double x, Vector3D force): base(new Point3D(x, 0, 0), force)
		{
		}

        public VaultPointLoad(Point3D loc, Vector3D force) : base(loc, force)
        {
        }

        public override List<PointLoad> ProjectedLoads<T>(T structure)
        {
            if (structure is Vault)
                return ProjectedLoadsOnVault(structure as Vault);
            return new List<PointLoad>();
        }

        public List<PointLoad> ProjectedLoadsOnVault(Vault vault)
        { 
            double loc = ClosestValue(Loc.X, (vault as IStructure).MidSegmentX);
			return new List<PointLoad> {
                new VaultPointLoad(
                    new Point3D(loc, 0, vault.F(loc)), 
                    Force.ProjectOn(UnitVector3D.XAxis)
                ),
                new VaultPointLoad(
                    new Point3D(loc, 0, vault.F(loc)),
                    Force.ProjectOn(UnitVector3D.ZAxis)
                )
            };
		}
	}


	public class VaultDistributedLoad : IDiscretizable
	{
		public VaultDistributedLoad(Func<double, Vector3D> load, double start, double end)
        {
            Load = load;
			Start = start;
			End = end;
		}

		Func<double, Vector3D> Load { get; }
		double Start { get; }
		double End { get; }

        public List<PointLoad> ProjectedLoads<T>(T structure) where T : IStructure
        {
            if (structure is Vault)
                return ProjectedLoadsOnVault(structure as Vault);
            return new List<PointLoad>();
        }

        public List<PointLoad> ProjectedLoadsOnVault(Vault vault)
		{
            var res = new List<PointLoad>();

            return  res;
        }
	}

    public class VaultLoadCase : LoadCase, IVaultResults
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
