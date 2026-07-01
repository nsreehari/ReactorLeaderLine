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

    [Fact]
    public void Magnet_LeavesEachEndpointStraightBeforeCurving()
    {
        ConnectorGeometry g = PathBuilder.Build(Start, End, LeaderLinePath.Magnet);

        // A short straight lead, a cubic join, then a short straight lead into the end.
        Assert.Equal(3, g.Segments.Count);
        LineSegmentTo lead1 = Assert.IsType<LineSegmentTo>(g.Segments[0]);
        Assert.IsType<CubicSegmentTo>(g.Segments[1]);
        LineSegmentTo lead2 = Assert.IsType<LineSegmentTo>(g.Segments[2]);

        // First lead travels along the start's outward direction (+x) and stays on the baseline.
        Assert.True(lead1.To.X > g.Start.X);
        Assert.True(IsClose(lead1.To.Y, g.Start.Y));

        // Connector still terminates exactly at the end point.
        Assert.Equal(new GeoPoint(200, 0), lead2.To);
        Assert.Equal(new GeoPoint(200, 0), g.End);
    }

    [Fact]
    public void Grid_WithoutCornerRadius_HasSharpElbows()
    {
        var start = new EndpointGeometry(new GeoPoint(0, 0), new GeoPoint(1, 0));
        var end = new EndpointGeometry(new GeoPoint(200, 100), new GeoPoint(-1, 0));

        ConnectorGeometry g = PathBuilder.Build(start, end, LeaderLinePath.Grid, cornerRadius: 0);

        Assert.All(g.Segments, s => Assert.IsType<LineSegmentTo>(s));
    }

    [Fact]
    public void Grid_WithCornerRadius_FilletsElbowsWithCubics()
    {
        var start = new EndpointGeometry(new GeoPoint(0, 0), new GeoPoint(1, 0));
        var end = new EndpointGeometry(new GeoPoint(200, 100), new GeoPoint(-1, 0));

        ConnectorGeometry sharp = PathBuilder.Build(start, end, LeaderLinePath.Grid, cornerRadius: 0);
        ConnectorGeometry rounded = PathBuilder.Build(start, end, LeaderLinePath.Grid, cornerRadius: 16);

        // Rounding introduces cubic fillet segments that the sharp version does not have.
        Assert.Contains(rounded.Segments, s => s is CubicSegmentTo);
        Assert.DoesNotContain(sharp.Segments, s => s is CubicSegmentTo);

        // The routing still starts and ends at the same points.
        Assert.Equal(sharp.Start, rounded.Start);
        Assert.Equal(sharp.End, rounded.End);
    }

    [Fact]
    public void Grid_CornerRadius_IsClampedToHalfTheShorterLeg()
    {
        // Legs are only 20px long here; an oversized radius must not overshoot past the corner.
        var start = new EndpointGeometry(new GeoPoint(0, 0), new GeoPoint(1, 0));
        var end = new EndpointGeometry(new GeoPoint(20, 20), new GeoPoint(-1, 0));

        ConnectorGeometry rounded = PathBuilder.Build(start, end, LeaderLinePath.Grid, cornerRadius: 1000);

        Assert.Equal(new GeoPoint(0, 0), rounded.Start);
        Assert.Equal(new GeoPoint(20, 20), rounded.End);
        Assert.Contains(rounded.Segments, s => s is CubicSegmentTo);
    }

    private static bool IsClose(double a, double b) => System.Math.Abs(a - b) < 1e-6;
}
