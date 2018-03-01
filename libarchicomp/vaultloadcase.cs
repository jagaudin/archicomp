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
		public VaultPointLoad(double x, double force, Vector3D direction)
		{
			this.x = x;
			Force = force;
            Direction = direction;
		}

		public override List<PointLoad> ToPointLoads(IStructure structure)
		{
            double loc = ClosestValue(x, structure.MidSegmentX);
			return new List<PointLoad> { new VaultPointLoad(loc, Force, Direction) };
		}
	}


	public class DistributedLoad : ILoad
	{
		public DistributedLoad(Func<double, double> load, double start, double end)
		{
			Load = load;
			Start = start;
			End = end;
		}

		Func<double, double> Load { get; }
		double Start { get; }
		double End { get; }

		public List<PointLoad> ToPointLoads(IStructure Structure)
		{
			throw new NotImplementedException();
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
