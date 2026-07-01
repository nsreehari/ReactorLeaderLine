using System.Collections.Generic;
using Reactor.Community.LeaderLine;
using Reactor.Community.LeaderLine.Geometry;
using Xunit;

namespace Reactor.Community.LeaderLine.Tests;

public class PathBuilderTests
{
    private static readonly EndpointGeometry Start = new(new GeoPoint(0, 0), new GeoPoint(1, 0));
    private static readonly EndpointGeometry End = new(new GeoPoint(200, 0), new GeoPoint(-1, 0));

    [Fact]
    public void Straight_ProducesSingleLineSegmentToEnd()
    {
        ConnectorGeometry g = PathBuilder.Build(Start, End, LeaderLinePath.Straight);

        ConnectorSegment only = Assert.Single(g.Segments);
        LineSegmentTo line = Assert.IsType<LineSegmentTo>(only);
        Assert.Equal(new GeoPoint(0, 0), g.Start);
        Assert.Equal(new GeoPoint(200, 0), line.To);
    }

    [Fact]
    public void Fluid_ProducesCubicWithControlsAlongSocketDirections()
    {
        ConnectorGeometry g = PathBuilder.Build(Start, End, LeaderLinePath.Fluid);

        CubicSegmentTo cubic = Assert.IsType<CubicSegmentTo>(Assert.Single(g.Segments));
        // Start control point extends to the right of the start; end control point to the left of the end.
        Assert.True(cubic.Control1.X > 0);
        Assert.True(cubic.Control2.X < 200);
        Assert.Equal(new GeoPoint(200, 0), cubic.To);
    }

    [Fact]
    public void End_ReflectsLastSegmentTerminus()
    {
        ConnectorGeometry g = PathBuilder.Build(Start, End, LeaderLinePath.Arc);
        Assert.Equal(new GeoPoint(200, 0), g.End);
    }

    [Fact]
    public void Grid_HorizontalSockets_ProduceOrthogonalSegments()
    {
        var start = new EndpointGeometry(new GeoPoint(0, 0), new GeoPoint(1, 0));
        var end = new EndpointGeometry(new GeoPoint(200, 100), new GeoPoint(-1, 0));

        ConnectorGeometry g = PathBuilder.Build(start, end, LeaderLinePath.Grid);

        Assert.All(g.Segments, s => Assert.IsType<LineSegmentTo>(s));
        Assert.Equal(new GeoPoint(200, 100), g.End);

        // Every leg is axis-aligned (either x or y constant between consecutive points).
        GeoPoint prev = g.Start;
        foreach (ConnectorSegment segment in g.Segments)
        {
            GeoPoint to = ((LineSegmentTo)segment).To;
            bool axisAligned = IsClose(prev.X, to.X) || IsClose(prev.Y, to.Y);
            Assert.True(axisAligned, $"Segment {prev} -> {to} is not axis-aligned.");
            prev = to;
        }
    }

    [Fact]
    public void Distance_IsEuclidean()
    {
        Assert.Equal(5, PathBuilder.Distance(new GeoPoint(0, 0), new GeoPoint(3, 4)), 3);
    }

    private static bool IsClose(double a, double b) => System.Math.Abs(a - b) < 1e-6;
}
