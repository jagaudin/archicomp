using System;
using System.Collections.Generic;
using System.Linq;

using libarchicomp.utils;


namespace libarchicomp
{
    public interface IResults
    {
        double IntM { get; set; }
        double IntMx { get; set; }
        double IntMy { get; set; }
        double SumMb { get; set; }
        double TotalVerticalLoad { get; }
        double TotalHorizontalLoad { get; }
    }

    public interface ILoad
    {
        List<PointLoad> ToPointLoads(IStructure Structure);
    }

    public class PointLoad : ILoad
    {
        public PointLoad(double x, double force)
        {
            this.x = x;
            Force = force;
        }

        public double x { get; }
        public double Force { get; }

        public List<PointLoad> ToPointLoads(IStructure Structure)
        {
            List<double> XList = Structure.MidSegmentX;
            Func<int, double> key = i => Math.Abs(x - XList[i]);
            int index = Enumerable.Range(0, XList.Count).OrderBy(key).First();
            return new List<PointLoad>{ new PointLoad(XList[index], Force) };
        }
    }


    public class DistributedLoad : ILoad
    {
        public DistributedLoad()
        {

        }

        public List<PointLoad> ToPointLoads(IStructure Structure)
        {
            throw new NotImplementedException();
        }
    }

    public class LoadCase : IResults
    {
        public LoadCase(IStructure structure)
        {
            Structure = structure;
        }

        IStructure Structure;

        List<PointLoad> VerticalLoads = new List<PointLoad>();
        List<PointLoad> HorizontalLoads = new List<PointLoad>();

        public IResults Results
        {
            get
            {
                return this;
            }
        }

        double IResults.IntM { get; set; } = 0;

        double IResults.IntMx { get; set; } = 0;

        double IResults.IntMy { get; set; } = 0;

        double IResults.SumMb { get; set; } = 0;

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
