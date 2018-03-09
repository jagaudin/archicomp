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

    public interface IDiscretizable
    {
        List<PointLoad> ProjectedLoads<T>(T Structure) where T : IStructure;
    }

	public abstract class PointLoad : IDiscretizable
	{
        public PointLoad(Point3D loc, Vector3D force)
        {
            Loc = loc;
            Force = force;
        }

        public Point3D Loc { get; protected set; }
        public Vector3D Force { get; protected set; }

		public abstract List<PointLoad> ProjectedLoads<T>(T structure) where T : IStructure;
	}

    public abstract class DistributedLoad : IDiscretizable
    {

        public abstract List<PointLoad> ProjectedLoads<T>(T Structure) where T : IStructure;
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
                return VerticalLoads.Sum(
                    pload => pload.Force.DotProduct(UnitVector3D.ZAxis)
                );
            }
        }

        double IResults.TotalHorizontalLoad
        {
            get
            {
                return HorizontalLoads.Sum(
                    pload => pload.Force.DotProduct(UnitVector3D.XAxis)
                );       
            }
        }
    }
}
