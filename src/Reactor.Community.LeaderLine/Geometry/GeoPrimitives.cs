using System.Collections.Generic;

namespace Reactor.Community.LeaderLine.Geometry;

/// <summary>A 2D point in the connector's coordinate space.</summary>
public readonly record struct GeoPoint(double X, double Y);

/// <summary>An axis-aligned rectangle in the connector's coordinate space.</summary>
public readonly record struct GeoRect(double X, double Y, double Width, double Height)
{
    /// <summary>The centre of the rectangle.</summary>
    public GeoPoint Center => new(X + (Width / 2), Y + (Height / 2));
}

/// <summary>
/// A resolved endpoint: the attachment <see cref="Point"/> plus the outward unit
/// <see cref="Direction"/> the connector leaves the endpoint along.
/// </summary>
public readonly record struct EndpointGeometry(GeoPoint Point, GeoPoint Direction);

/// <summary>Base type for a segment of a resolved connector path.</summary>
public abstract record ConnectorSegment;

/// <summary>A straight segment ending at <see cref="To"/>.</summary>
public sealed record LineSegmentTo(GeoPoint To) : ConnectorSegment;

/// <summary>A cubic Bézier segment with two control points ending at <see cref="To"/>.</summary>
public sealed record CubicSegmentTo(GeoPoint Control1, GeoPoint Control2, GeoPoint To) : ConnectorSegment;

/// <summary>
/// A fully resolved connector path: a <see cref="Start"/> point followed by one or
/// more <see cref="Segments"/>. Renderer-agnostic so it can be unit tested and then
/// projected onto a WinUI <c>PathGeometry</c>.
/// </summary>
public sealed record ConnectorGeometry(GeoPoint Start, IReadOnlyList<ConnectorSegment> Segments)
{
    /// <summary>The final point of the last segment (the connector's end point).</summary>
    public GeoPoint End => Segments.Count == 0
        ? Start
        : Segments[^1] switch
        {
            LineSegmentTo line => line.To,
            CubicSegmentTo cubic => cubic.To,
            _ => Start,
        };
}
