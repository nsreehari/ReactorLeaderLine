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
    private const double MagnetLeadMin = 16;
    private const double MagnetLeadMax = 40;
    private const double MagnetLeadFactor = 0.2;

    /// <summary>
    /// Builds the connector geometry for the given endpoints and <paramref name="path"/> style.
    /// </summary>
    /// <param name="start">The resolved start endpoint.</param>
    /// <param name="end">The resolved end endpoint.</param>
    /// <param name="path">The routing style.</param>
    /// <param name="cornerRadius">
    /// Corner radius for orthogonal (<see cref="LeaderLinePath.Grid"/>) routing. When
    /// greater than zero the right-angle elbows are filleted; ignored by other styles.
    /// </param>
    public static ConnectorGeometry Build(EndpointGeometry start, EndpointGeometry end, LeaderLinePath path, double cornerRadius = 0)
        => path switch
        {
            LeaderLinePath.Straight => BuildStraight(start, end),
            LeaderLinePath.Arc => BuildArc(start, end),
            LeaderLinePath.Grid => BuildGrid(start, end, cornerRadius),
            LeaderLinePath.Magnet => BuildMagnet(start, end),
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

    /// <summary>
    /// Magnet routing: leave each endpoint travelling straight along its socket for a
    /// short lead, then join the two leads with a smooth cubic. This keeps the plug
    /// firmly "docked" to its side before the curve begins.
    /// </summary>
    private static ConnectorGeometry BuildMagnet(EndpointGeometry start, EndpointGeometry end)
    {
        double lead = Math.Clamp(Distance(start.Point, end.Point) * MagnetLeadFactor, MagnetLeadMin, MagnetLeadMax);

        var l1 = new GeoPoint(start.Point.X + (start.Direction.X * lead), start.Point.Y + (start.Direction.Y * lead));
        var l2 = new GeoPoint(end.Point.X + (end.Direction.X * lead), end.Point.Y + (end.Direction.Y * lead));

        double g = Gravity(l1, l2);
        var c1 = new GeoPoint(l1.X + (start.Direction.X * g), l1.Y + (start.Direction.Y * g));
        var c2 = new GeoPoint(l2.X + (end.Direction.X * g), l2.Y + (end.Direction.Y * g));

        return new ConnectorGeometry(start.Point, new ConnectorSegment[]
        {
            new LineSegmentTo(l1),
            new CubicSegmentTo(c1, c2, l2),
            new LineSegmentTo(end.Point),
        });
    }

    private static ConnectorGeometry BuildGrid(EndpointGeometry start, EndpointGeometry end, double cornerRadius)
    {
        IReadOnlyList<GeoPoint> points = GridPoints(start, end);
        return cornerRadius > 0
            ? RoundedPolyline(points, cornerRadius)
            : Polyline(points);
    }

    /// <summary>Builds the orthogonal polyline vertices (start, elbows..., end) for Grid routing.</summary>
    private static List<GeoPoint> GridPoints(EndpointGeometry start, EndpointGeometry end)
    {
        GeoPoint a = start.Point;
        GeoPoint b = end.Point;
        var points = new List<GeoPoint> { a };

        bool startHorizontal = Math.Abs(start.Direction.X) >= Math.Abs(start.Direction.Y);
        bool endHorizontal = Math.Abs(end.Direction.X) >= Math.Abs(end.Direction.Y);

        if (startHorizontal && endHorizontal)
        {
            double midX = (a.X + b.X) / 2;
            points.Add(new GeoPoint(midX, a.Y));
            points.Add(new GeoPoint(midX, b.Y));
        }
        else if (!startHorizontal && !endHorizontal)
        {
            double midY = (a.Y + b.Y) / 2;
            points.Add(new GeoPoint(a.X, midY));
            points.Add(new GeoPoint(b.X, midY));
        }
        else if (startHorizontal)
        {
            points.Add(new GeoPoint(b.X, a.Y));
        }
        else
        {
            points.Add(new GeoPoint(a.X, b.Y));
        }

        points.Add(b);
        return points;
    }

    /// <summary>Converts a list of vertices into a straight-segment connector geometry.</summary>
    private static ConnectorGeometry Polyline(IReadOnlyList<GeoPoint> points)
    {
        var segments = new List<ConnectorSegment>(points.Count - 1);
        for (int i = 1; i < points.Count; i++)
        {
            segments.Add(new LineSegmentTo(points[i]));
        }

        return new ConnectorGeometry(points[0], segments);
    }

    /// <summary>
    /// Converts a polyline into a connector geometry whose interior corners are filleted
    /// with cubic arcs of at most <paramref name="radius"/> pixels.
    /// </summary>
    private static ConnectorGeometry RoundedPolyline(IReadOnlyList<GeoPoint> points, double radius)
    {
        var segments = new List<ConnectorSegment>();

        for (int i = 1; i < points.Count - 1; i++)
        {
            GeoPoint prev = points[i - 1];
            GeoPoint corner = points[i];
            GeoPoint next = points[i + 1];

            double inLen = Distance(prev, corner);
            double outLen = Distance(corner, next);
            if (inLen < 1e-6 || outLen < 1e-6)
            {
                segments.Add(new LineSegmentTo(corner));
                continue;
            }

            double r = Math.Min(radius, Math.Min(inLen, outLen) / 2);
            GeoPoint before = Lerp(corner, prev, r / inLen);
            GeoPoint after = Lerp(corner, next, r / outLen);

            segments.Add(new LineSegmentTo(before));
            // Control points at the corner pull the cubic into a smooth quarter-round fillet.
            segments.Add(new CubicSegmentTo(corner, corner, after));
        }

        segments.Add(new LineSegmentTo(points[^1]));
        return new ConnectorGeometry(points[0], segments);
    }

    private static GeoPoint Lerp(GeoPoint from, GeoPoint to, double t)
        => new(from.X + ((to.X - from.X) * t), from.Y + ((to.Y - from.Y) * t));
}
