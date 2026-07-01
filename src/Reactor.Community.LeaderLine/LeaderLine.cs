using System;
using System.Collections.Generic;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Reactor.Community.LeaderLine.Geometry;
using Reactor.Community.LeaderLine.Internal;
using Windows.Foundation;
using static Microsoft.UI.Reactor.Factories;

namespace Reactor.Community.LeaderLine;

/// <summary>
/// A declarative connector ("leader line") between two anchors, for Microsoft.UI.Reactor.
/// <para>
/// Render it as an overlay inside the same container as the elements you want to connect
/// (for example a <c>Grid</c> whose last child is the leader line). It renders a
/// transparent, hit-test-invisible <c>Canvas</c>, measures its <see cref="LeaderLineProps.Start"/>
/// and <see cref="LeaderLineProps.End"/> anchors relative to that canvas, and draws the
/// connector. The line re-positions automatically as anchored elements move or resize.
/// </para>
/// </summary>
public sealed class LeaderLine : Component<LeaderLineProps>
{
    private const double PositionTolerance = 0.5;

    /// <summary>Builds the connector overlay and wires up automatic re-positioning.</summary>
    public override Element Render()
    {
        var canvasRef = UseRef<Canvas?>(null);
        var lastGeometry = UseRef<ConnectorGeometry?>(null);
        var (geometry, setGeometry) = UseState<ConnectorGeometry?>(null);

        UseEffect(
            () =>
            {
                Canvas? canvas = canvasRef.Current;
                if (canvas is null)
                {
                    return () => { };
                }

                void Recompute()
                {
                    ConnectorGeometry? next = Props.Visible ? Compute(canvas, Props) : null;
                    if (GeometryEquals(lastGeometry.Current, next))
                    {
                        return;
                    }

                    lastGeometry.Current = next;
                    setGeometry(next);
                }

                Recompute();

                EventHandler<object> handler = (_, _) => Recompute();
                canvas.LayoutUpdated += handler;
                return () => canvas.LayoutUpdated -= handler;
            });

        var children = new List<Element>();
        if (Props.Visible && geometry is not null)
        {
            LeaderLineRenderer.BuildVisuals(children, geometry, Props);
        }

        return Canvas(children.ToArray())
            .Set(canvas =>
            {
                canvasRef.Current = canvas;
                canvas.IsHitTestVisible = false;
                canvas.Background = null;
            });
    }

    private static ConnectorGeometry? Compute(Canvas root, LeaderLineProps p)
    {
        if (!TryResolveReference(root, p.Start, out GeoRect? startBox, out GeoPoint startRef, out LeaderLineSocket startSocket)
            || !TryResolveReference(root, p.End, out GeoRect? endBox, out GeoPoint endRef, out LeaderLineSocket endSocket))
        {
            return null;
        }

        EndpointGeometry start = startBox is { } sb
            ? SocketResolver.ResolveRect(sb, startSocket, endRef)
            : SocketResolver.ResolvePoint(startRef, endRef);

        EndpointGeometry end = endBox is { } eb
            ? SocketResolver.ResolveRect(eb, endSocket, startRef)
            : SocketResolver.ResolvePoint(endRef, startRef);

        return PathBuilder.Build(start, end, p.Path, p.CornerRadius);
    }

    private static bool TryResolveReference(
        Canvas root,
        LeaderLineAnchor anchor,
        out GeoRect? box,
        out GeoPoint reference,
        out LeaderLineSocket socket)
    {
        switch (anchor)
        {
            case PointAnchor pt:
                box = null;
                reference = new GeoPoint(pt.X, pt.Y);
                socket = LeaderLineSocket.Auto;
                return true;

            case AreaAnchor area:
                var areaRect = new GeoRect(area.X, area.Y, area.Width, area.Height);
                box = areaRect;
                reference = areaRect.Center;
                socket = area.Socket;
                return true;

            case ElementAnchor element:
            {
                FrameworkElement? fe = element.Resolve();
                if (fe is null || fe.ActualWidth <= 0 || fe.ActualHeight <= 0)
                {
                    box = null;
                    reference = default;
                    socket = LeaderLineSocket.Auto;
                    return false;
                }

                try
                {
                    GeneralTransform transform = fe.TransformToVisual(root);
                    Point topLeft = transform.TransformPoint(new Point(0, 0));
                    var rect = new GeoRect(topLeft.X, topLeft.Y, fe.ActualWidth, fe.ActualHeight);
                    box = rect;
                    reference = rect.Center;
                    socket = element.Socket;
                    return true;
                }
                catch (ArgumentException)
                {
                    // Elements not in a shared visual tree cannot be transformed.
                    box = null;
                    reference = default;
                    socket = LeaderLineSocket.Auto;
                    return false;
                }
            }

            default:
                box = null;
                reference = default;
                socket = LeaderLineSocket.Auto;
                return false;
        }
    }

    private static bool GeometryEquals(ConnectorGeometry? a, ConnectorGeometry? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        if (a.Segments.Count != b.Segments.Count || !PointClose(a.Start, b.Start))
        {
            return false;
        }

        for (int i = 0; i < a.Segments.Count; i++)
        {
            if (!SegmentClose(a.Segments[i], b.Segments[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool SegmentClose(ConnectorSegment a, ConnectorSegment b)
        => (a, b) switch
        {
            (LineSegmentTo la, LineSegmentTo lb) => PointClose(la.To, lb.To),
            (CubicSegmentTo ca, CubicSegmentTo cb) =>
                PointClose(ca.Control1, cb.Control1)
                && PointClose(ca.Control2, cb.Control2)
                && PointClose(ca.To, cb.To),
            _ => false,
        };

    private static bool PointClose(GeoPoint a, GeoPoint b)
        => Math.Abs(a.X - b.X) <= PositionTolerance && Math.Abs(a.Y - b.Y) <= PositionTolerance;
}
