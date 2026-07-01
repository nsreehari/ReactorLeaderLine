using System;
using System.Collections.Generic;

namespace Reactor.Community.LeaderLine.Geometry;

/// <summary>
/// Builds a renderer-agnostic <see cref="ConnectorGeometry"/> from two resolved
/// endpoints and a routing style. Pure math — no WinUI dependency.
/// </summary>
public static class PathBuilder
{
    private const double MinGravity = 48;
    private const double MaxGravity = 160;
    private const double GravityFactor = 0.4;

    /// <summary>
    /// Builds the connector geometry for the given endpoints and <paramref name="path"/> style.
    /// </summary>
    public static ConnectorGeometry Build(EndpointGeometry start, EndpointGeometry end, LeaderLinePath path)
        => path switch
        {
            LeaderLinePath.Straight => BuildStraight(start, end),
            LeaderLinePath.Arc => BuildArc(start, end),
            LeaderLinePath.Grid => BuildGrid(start, end),
            _ => BuildFluid(start, end),
        };

    /// <summary>Distance between two points.</summary>
    public static double Distance(GeoPoint a, GeoPoint b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        return Math.Sqrt((dx * dx) + (dy * dy));
    }

    private static double Gravity(GeoPoint a, GeoPoint b)
        => Math.Clamp(Distance(a, b) * GravityFactor, MinGravity, MaxGravity);

    private static ConnectorGeometry BuildStraight(EndpointGeometry start, EndpointGeometry end)
        => new(start.Point, new ConnectorSegment[] { new LineSegmentTo(end.Point) });

    private static ConnectorGeometry BuildFluid(EndpointGeometry start, EndpointGeometry end)
    {
        double g = Gravity(start.Point, end.Point);
        var c1 = new GeoPoint(start.Point.X + (start.Direction.X * g), start.Point.Y + (start.Direction.Y * g));
        var c2 = new GeoPoint(end.Point.X + (end.Direction.X * g), end.Point.Y + (end.Direction.Y * g));
        return new ConnectorGeometry(start.Point, new ConnectorSegment[] { new CubicSegmentTo(c1, c2, end.Point) });
    }

    private static ConnectorGeometry BuildArc(EndpointGeometry start, EndpointGeometry end)
    {
        // Bow the curve perpendicular to the baseline by a fraction of its length.
        GeoPoint a = start.Point;
        GeoPoint b = end.Point;
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        double len = Math.Sqrt((dx * dx) + (dy * dy));
        double bow = Math.Clamp(len * 0.25, 24, 140);

        // Perpendicular unit vector (rotate baseline by -90°).
        double px = len < 1e-6 ? 0 : -dy / len;
        double py = len < 1e-6 ? 0 : dx / len;

        var c1 = new GeoPoint(a.X + (dx / 3) + (px * bow), a.Y + (dy / 3) + (py * bow));
        var c2 = new GeoPoint(a.X + (2 * dx / 3) + (px * bow), a.Y + (2 * dy / 3) + (py * bow));
        return new ConnectorGeometry(a, new ConnectorSegment[] { new CubicSegmentTo(c1, c2, b) });
    }

    private static ConnectorGeometry BuildGrid(EndpointGeometry start, EndpointGeometry end)
    {
        GeoPoint a = start.Point;
        GeoPoint b = end.Point;
        var segments = new List<ConnectorSegment>();

        bool startHorizontal = Math.Abs(start.Direction.X) >= Math.Abs(start.Direction.Y);
        bool endHorizontal = Math.Abs(end.Direction.X) >= Math.Abs(end.Direction.Y);

        if (startHorizontal && endHorizontal)
        {
            double midX = (a.X + b.X) / 2;
            segments.Add(new LineSegmentTo(new GeoPoint(midX, a.Y)));
            segments.Add(new LineSegmentTo(new GeoPoint(midX, b.Y)));
            segments.Add(new LineSegmentTo(b));
        }
        else if (!startHorizontal && !endHorizontal)
        {
            double midY = (a.Y + b.Y) / 2;
            segments.Add(new LineSegmentTo(new GeoPoint(a.X, midY)));
            segments.Add(new LineSegmentTo(new GeoPoint(b.X, midY)));
            segments.Add(new LineSegmentTo(b));
        }
        else if (startHorizontal)
        {
            segments.Add(new LineSegmentTo(new GeoPoint(b.X, a.Y)));
            segments.Add(new LineSegmentTo(b));
        }
        else
        {
            segments.Add(new LineSegmentTo(new GeoPoint(a.X, b.Y)));
            segments.Add(new LineSegmentTo(b));
        }

        return new ConnectorGeometry(a, segments);
    }
}
