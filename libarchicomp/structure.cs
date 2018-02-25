using System.Collections.Generic;
using libarchicomp.utils;


namespace libarchicomp.structure
{
	public interface ICoord
	{
		List<double> SegmentX { get; }
		List<double> MidSegmentX { get; }
	}

	public interface IPoints
	{
		Point Start { get; }
		Point End { get; }
		List<Point> MidSegment { get; }
		List<Point> Curve { get; }
	}

	public interface IStructure: ICoord, IPoints
	{
	}
}
