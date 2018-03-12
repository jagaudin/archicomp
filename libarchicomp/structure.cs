using System.Collections.Generic;

using MathNet.Spatial.Euclidean;

namespace libarchicomp.structure
{
	public interface IStructure2DDiscretCoord
	{
		List<double> SegmentX { get; }
		List<double> MidSegmentX { get; }
	}

	public interface IStructure2DDiscretePoints
	{
		Point3D Start { get; }
		Point3D End { get; }
		List<Point3D> MidSegment { get; }
		List<Point3D> Segment { get; }
	}

	public interface IStructure
	{
	}

    public interface IDiscretizableStructure2D: IStructure, IStructure2DDiscretCoord, IStructure2DDiscretePoints
    {
        
    }
}
