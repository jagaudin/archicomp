﻿using System;
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

		public override List<PointLoad> ProjectedLoads(IStructure structure)
		{
            if (!(structure is Vault))
                return new List<PointLoad>();

            double loc = ClosestValue(Loc.X, structure.MidSegmentX);
			return new List<PointLoad> {
                new VaultPointLoad(
                    new Point3D(loc, 0, (structure as Vault).F(loc)), 
                    Force.ProjectOn(UnitVector3D.XAxis)
                ),
                new VaultPointLoad(
                    new Point3D(loc, 0, (structure as Vault).F(loc)),
                    Force.ProjectOn(UnitVector3D.ZAxis)
                )
            };
		}
	}


	public class DistributedLoad : ILoad
	{
		public DistributedLoad(Func<double, Vector3D> load, double start, double end)
        {
            Load = load;
			Start = start;
			End = end;
		}

		Func<double, Vector3D> Load { get; }
		double Start { get; }
		double End { get; }

		public List<PointLoad> ProjectedLoads(IStructure structure)
		{
            var res = new List<PointLoad>();

            if (!(structure is Vault))
                return res;

            

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
