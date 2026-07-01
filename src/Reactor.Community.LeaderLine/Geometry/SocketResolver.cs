using System;

namespace Reactor.Community.LeaderLine.Geometry;

/// <summary>
/// Resolves the concrete attachment point and outward direction for a connector
/// endpoint, given the endpoint's bounding rectangle, the configured socket, and the
/// opposite endpoint it is pointing toward.
/// </summary>
public static class SocketResolver
{
    /// <summary>
    /// Resolves an endpoint anchored to a rectangle.
    /// </summary>
    /// <param name="box">The endpoint's bounding rectangle.</param>
    /// <param name="socket">The requested socket, or <see cref="LeaderLineSocket.Auto"/>.</param>
    /// <param name="toward">A point the connector is heading toward (the other endpoint).</param>
    public static EndpointGeometry ResolveRect(GeoRect box, LeaderLineSocket socket, GeoPoint toward)
    {
        LeaderLineSocket side = socket == LeaderLineSocket.Auto
            ? ChooseAutoSide(box, toward)
            : socket;

        return side switch
        {
            LeaderLineSocket.Top => new EndpointGeometry(new GeoPoint(box.X + (box.Width / 2), box.Y), new GeoPoint(0, -1)),
            LeaderLineSocket.Bottom => new EndpointGeometry(new GeoPoint(box.X + (box.Width / 2), box.Y + box.Height), new GeoPoint(0, 1)),
            LeaderLineSocket.Left => new EndpointGeometry(new GeoPoint(box.X, box.Y + (box.Height / 2)), new GeoPoint(-1, 0)),
            LeaderLineSocket.Right => new EndpointGeometry(new GeoPoint(box.X + box.Width, box.Y + (box.Height / 2)), new GeoPoint(1, 0)),
            _ => new EndpointGeometry(box.Center, DirectionToward(box.Center, toward)),
        };
    }

    /// <summary>
    /// Resolves an endpoint anchored to a single point. The outward direction faces
    /// the opposite endpoint (used by curved routing).
    /// </summary>
    public static EndpointGeometry ResolvePoint(GeoPoint point, GeoPoint toward)
        => new(point, DirectionToward(point, toward));

    /// <summary>
    /// Picks the rectangle side whose outward normal best faces <paramref name="toward"/>.
    /// Horizontal separation wins ties on the dominant axis.
    /// </summary>
    public static LeaderLineSocket ChooseAutoSide(GeoRect box, GeoPoint toward)
    {
        GeoPoint c = box.Center;
        double dx = toward.X - c.X;
        double dy = toward.Y - c.Y;

        if (Math.Abs(dx) >= Math.Abs(dy))
        {
            return dx >= 0 ? LeaderLineSocket.Right : LeaderLineSocket.Left;
        }

        return dy >= 0 ? LeaderLineSocket.Bottom : LeaderLineSocket.Top;
    }

    private static GeoPoint DirectionToward(GeoPoint from, GeoPoint to)
    {
        double dx = to.X - from.X;
        double dy = to.Y - from.Y;
        double len = Math.Sqrt((dx * dx) + (dy * dy));
        return len < 1e-6 ? new GeoPoint(0, 0) : new GeoPoint(dx / len, dy / len);
    }
}
