using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Spatial.Euclidean;

using Constants = libarchicomp.utils.Constants;
using libarchicomp.structure;


namespace libarchicomp.loadcase
{
    public struct Boundaries
    {
        public Boundaries(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public double Min;
        public double Max;
    }

    public interface IResults
    {
        double TotalVerticalLoad { get; }
        double TotalHorizontalLoad { get; }
    }

    public interface IDiscretizableLoad<T> where T : IStructure
    {
        List<PointLoad> ToProjectedPointLoads(T Structure);
    }


    public abstract class PointLoad 
    {
        protected PointLoad(Point3D loc, Vector3D force)
        {
            Loc = loc;
            Force = force;
        }

        public Point3D Loc { get; protected set; }
        public Vector3D Force { get; protected set; }

        public double Prec => Constants.Prec;

        public override string ToString() => (string.Format("Loc: {0}, Force: {1}", Loc.ToString(), Force.ToString()));
    }


    public abstract class DistributedLoad
    {
        public Func<Point3D, Vector3D> Force;
        public Dictionary<UnitVector3D, Boundaries> Boundaries = 
            new Dictionary<UnitVector3D, Boundaries>(){
                {UnitVector3D.XAxis, default(Boundaries)},
                {UnitVector3D.YAxis, default(Boundaries)},
                {UnitVector3D.ZAxis, default(Boundaries)}
        };

        public double Prec => Constants.Prec;
    }


    public abstract class LoadCase<T> : IResults where T : IStructure
    {
        protected LoadCase(IStructure structure)
        {
            Structure = structure;
        }

        IStructure Structure;

        public List<PointLoad> VerticalLoads = new List<PointLoad>();
		public List<PointLoad> HorizontalLoads = new List<PointLoad>();

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
