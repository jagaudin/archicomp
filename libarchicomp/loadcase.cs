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
        public Point3D Loc { get; protected set; }
        public Vector3D Force { get; protected set; }

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
                return VerticalLoads.Sum(pload => pload.Force.DotProduct(new Vector3D(0, 1, 0)));
            }
        }

        double IResults.TotalHorizontalLoad
        {
            get
            {
                return HorizontalLoads.Sum(pload => pload.Force.DotProduct(new Vector3D(1, 0, 0)));
            }
        }
    }
}
