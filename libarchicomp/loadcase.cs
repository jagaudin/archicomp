using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Spatial.Euclidean;

using libarchicomp.structure;


namespace libarchicomp.loadcase
{

    public interface IResults
    {
        double TotalVerticalLoad { get; }
        double TotalHorizontalLoad { get; }
    }

    public interface ILoad
    {
        List<PointLoad> ToPointLoads(IStructure Structure);
    }

	public abstract class PointLoad : ILoad
	{
        public double x { get; protected set; }
        public double y { get; protected set; }
        public double z { get; protected set; }

        public double Force { get; protected set; }
        public Vector3D Direction { get; protected set; }

		public abstract List<PointLoad> ToPointLoads(IStructure structure);
	}

    public abstract class LoadCase : IResults
    {
        public LoadCase(IStructure structure)
        {
            Structure = structure;
        }

        IStructure Structure;

        List<PointLoad> VerticalLoads = new List<PointLoad>();
		List<PointLoad> HorizontalLoads = new List<PointLoad>();

        double IResults.TotalVerticalLoad
        {
            get
            {
                return VerticalLoads.Sum(pload => pload.Force);
            }
        }

        double IResults.TotalHorizontalLoad
        {
            get
            {
                return HorizontalLoads.Sum(pload => pload.Force);
            }
        }
    }
}
