using System.Collections.Generic;

using MathNet.Spatial.Euclidean;

namespace libarchicomp.structure
{
	public interface ICoord
	{
		List<double> SegmentX { get; }
		List<double> MidSegmentX { get; }
	}

	public interface IPoints
	{
		Point3D Start { get; }
		Point3D End { get; }
		List<Point3D> MidSegment { get; }
		List<Point3D> Curve { get; }
	}

	public interface IStructure: ICoord, IPoints
	{
	}
}
