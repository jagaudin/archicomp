﻿using System;
using System.Collections.Generic;
using System.Linq;

using libarchicomp.structure;


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
			double loc = Structure.MidSegmentX.Aggregate(
				(u, v) => Math.Abs(u - x) < Math.Abs(v - x) ? u : v
			);
			return new List<PointLoad>{ new PointLoad(loc, Force) };
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