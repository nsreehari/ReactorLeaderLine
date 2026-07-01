using System;
using System.Collections.Generic;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Reactor.Community.LeaderLine.Geometry;
using Reactor.Community.LeaderLine.Internal;
using Windows.Foundation;
using Windows.UI;
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

    // Fallback stroke when the active theme's accent brush cannot be resolved
    // (for example before WinUI resources have loaded). Matches the previous
    // hardcoded default so behaviour degrades gracefully.
    private static readonly Color FallbackAccent = Color.FromArgb(255, 92, 107, 192);

    /// <summary>Builds the connector overlay and wires up automatic re-positioning.</summary>
    public override Element Render()
    {
        var lastGeometry = UseRef<ConnectorGeometry?>(null);
        var pointerPos = UseRef<GeoPoint?>(null);
        var (geometry, setGeometry) = UseState<ConnectorGeometry?>(null);

        // The overlay canvas is surfaced through a callback ref into state (not a plain
        // UseRef) so that assigning it triggers a re-render and re-runs the measurement
        // effect below. Reactor runs passive effects *before* the .Set ref callback fires,
        // so a plain ref would be null on the effect's only run and — because the effect's
        // dependencies never change — it would never recover and no line would ever draw.
        var (canvas, setCanvas) = UseState<Canvas?>(null);

        // Follow the Reactor theme + context: UseIsDarkTheme re-renders this component
        // whenever the effective colour scheme flips, and UseContext lets an ancestor
        // supply default connector styling for a whole subtree.
        bool isDark = UseIsDarkTheme();
        LeaderLineTheme themeContext = UseContext(LeaderLineContext.Theme);

        // Resolve any pointer-follow tracking elements during render so the effect can
        // re-subscribe when they change, and so RefreshToken can force a recompute.
        FrameworkElement? trackStart = (Props.Start as PointerAnchor)?.Track();
        FrameworkElement? trackEnd = (Props.End as PointerAnchor)?.Track();

        UseEffect(
            () =>
            {
                if (canvas is null)
                {
                    return () => { };
                }

                void Recompute()
                {
                    ConnectorGeometry? next = Props.Visible ? Compute(canvas, Props, pointerPos.Current) : null;
                    if (GeometryEquals(lastGeometry.Current, next))
                    {
                        return;
                    }

                    lastGeometry.Current = next;
                    setGeometry(next);
                }

                Recompute();

                EventHandler<object> layoutHandler = (_, _) => Recompute();
                canvas.LayoutUpdated += layoutHandler;

                // Bridge native pointer input into declarative state: on move we store the
                // cursor position and recompute; on exit we clear it so the endpoint (and
                // therefore the line) drops out until the pointer returns.
                var pointerTargets = new List<FrameworkElement>();
                if (trackStart is not null)
                {
                    pointerTargets.Add(trackStart);
                }

                if (trackEnd is not null && !ReferenceEquals(trackEnd, trackStart))
                {
                    pointerTargets.Add(trackEnd);
                }

                PointerEventHandler? movedHandler = null;
                PointerEventHandler? exitedHandler = null;
                if (pointerTargets.Count > 0)
                {
                    movedHandler = (_, e) =>
                    {
                        Point pt = e.GetCurrentPoint(canvas).Position;
                        pointerPos.Current = new GeoPoint(pt.X, pt.Y);
                        Recompute();
                    };
                    exitedHandler = (_, _) =>
                    {
                        pointerPos.Current = null;
                        Recompute();
                    };

                    foreach (FrameworkElement target in pointerTargets)
                    {
                        target.PointerMoved += movedHandler;
                        target.PointerExited += exitedHandler;
                    }
                }

                return () =>
                {
                    canvas.LayoutUpdated -= layoutHandler;
                    foreach (FrameworkElement target in pointerTargets)
                    {
                        if (movedHandler is not null)
                        {
                            target.PointerMoved -= movedHandler;
                        }

                        if (exitedHandler is not null)
                        {
                            target.PointerExited -= exitedHandler;
                        }
                    }
                };
            },
            canvas!,
            trackStart!,
            trackEnd!,
            Props.RefreshToken!);

        var children = new List<Element>();
        if (Props.Visible && geometry is not null)
        {
            LeaderLineResolvedStyle style = ResolveStyle(Props, themeContext, isDark);
            LeaderLineRenderer.BuildVisuals(children, geometry, Props, style);
        }

        return Canvas(children.ToArray())
            .Set(c =>
            {
                // Callback ref: publish the canvas into state the first time we see it so
                // the measurement effect can run against a live canvas. Guard against the
                // re-render loop by only updating state when the instance actually changes.
                if (!ReferenceEquals(canvas, c))
                {
                    setCanvas(c);
                }

                c.IsHitTestVisible = false;
                c.Background = null;
            });
    }

    // Resolves the effective connector colours. Precedence for each colour is:
    // explicit prop -> context default -> value derived from the active theme
    // (system accent for the stroke, page surface for the outline halo).
    private static LeaderLineResolvedStyle ResolveStyle(LeaderLineProps p, LeaderLineTheme context, bool isDark)
    {
        Color accent = ResolveThemeColor("AccentFillColorDefaultBrush", isDark, FallbackAccent);
        Color surface = ResolveThemeColor(
            "SolidBackgroundFillColorBaseBrush",
            isDark,
            isDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 255, 255, 255));

        Color stroke = p.Color ?? context.Color ?? accent;
        Color outline = p.OutlineColor ?? context.OutlineColor ?? surface;
        Color label = context.LabelColor ?? stroke;
        return new LeaderLineResolvedStyle(stroke, outline, label);
    }

    private static Color ResolveThemeColor(string resourceKey, bool isDark, Color fallback)
        => ThemeRef.Resolve(resourceKey, isDark) is SolidColorBrush brush ? brush.Color : fallback;

    private static ConnectorGeometry? Compute(Canvas root, LeaderLineProps p, GeoPoint? pointer)
    {
        if (!TryResolveReference(root, p.Start, pointer, out GeoRect? startBox, out GeoPoint startRef, out LeaderLineSocket startSocket)
            || !TryResolveReference(root, p.End, pointer, out GeoRect? endBox, out GeoPoint endRef, out LeaderLineSocket endSocket))
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
        GeoPoint? pointer,
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

            case PointerAnchor when pointer is { } cursor:
                box = null;
                reference = cursor;
                socket = LeaderLineSocket.Auto;
                return true;

            case PointerAnchor:
                // Pointer has not entered the tracked element yet; leave this endpoint
                // unresolved so the connector stays hidden until the cursor arrives.
                box = null;
                reference = default;
                socket = LeaderLineSocket.Auto;
                return false;

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
