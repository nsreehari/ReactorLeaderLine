using System;
using System.Collections.Generic;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using Reactor.Community.LeaderLine.Geometry;
using Windows.Foundation;
using Windows.UI;
using static Microsoft.UI.Reactor.Factories;
using Path = Microsoft.UI.Xaml.Shapes.Path;
using LineSegment = Microsoft.UI.Xaml.Media.LineSegment;

namespace Reactor.Community.LeaderLine.Internal;

/// <summary>
/// Theme-resolved connector colours passed from <see cref="LeaderLine"/> (which owns the
/// hooks) to the pure renderer. Each colour has already had the precedence
/// prop -&gt; context -&gt; theme applied.
/// </summary>
internal readonly record struct LeaderLineResolvedStyle(Color Stroke, Color Outline, Color Label);

/// <summary>
/// Projects a resolved <see cref="ConnectorGeometry"/> and its styling onto Reactor
/// elements: the stroke path, optional outline/shadow underlays, endpoint plugs, and
/// labels. Pure element construction — no hooks, no measurement.
/// </summary>
internal static class LeaderLineRenderer
{
    public static void BuildVisuals(List<Element> children, ConnectorGeometry g, LeaderLineProps p, LeaderLineResolvedStyle style)
    {
        Color strokeColor = style.Stroke;
        Brush StrokeBrush() => p.Gradient is { } grad
            ? MakeGradient(grad, g.Start, g.End)
            : new SolidColorBrush(strokeColor);

        int key = 0;

        // Drop shadow underlay (offset, translucent — an approximation without blur).
        if (p.DropShadow is { } shadow)
        {
            var shadowColor = Color.FromArgb((byte)(255 * Clamp01(shadow.Opacity)), 0, 0, 0);
            children.Add(StrokePath(g)
                .Stroke(new SolidColorBrush(shadowColor))
                .StrokeThickness(p.Size)
                .Opacity(p.Opacity)
                .Set(path =>
                {
                    path.RenderTransform = new TranslateTransform { X = shadow.OffsetX, Y = shadow.OffsetY };
                })
                .WithKey($"ll-shadow-{key++}"));
        }

        // Outline halo underlay.
        if (p.Outline)
        {
            children.Add(StrokePath(g)
                .Stroke(new SolidColorBrush(style.Outline))
                .StrokeThickness(p.Size + 4)
                .Opacity(p.Opacity)
                .WithKey($"ll-outline-{key++}"));
        }

        // Main stroke. The dash pattern and round dash cap use declarative modifiers;
        // only the (imperative) marching-ants animation falls back to .Set.
        PathElement stroke = StrokePath(g)
            .Stroke(StrokeBrush())
            .StrokeThickness(p.Size)
            .Opacity(p.Opacity);

        if (p.Dash is { } dash)
        {
            double unit = Math.Max(p.Size, 0.1);
            stroke = stroke
                .StrokeDashArray(dash.Length / unit, dash.Gap / unit)
                .StrokeDashCap(PenLineCap.Round);

            if (dash.Animate)
            {
                stroke = stroke.Set(path => AnimateDash(path, dash, unit));
            }
        }

        children.Add(stroke.WithKey($"ll-stroke-{key++}"));

        // Plugs.
        if (p.EndPlug != LeaderLinePlug.None)
        {
            AddPlug(children, p.EndPlug, g.End, EndTangent(g), strokeColor, p, ref key, "end");
        }

        if (p.StartPlug != LeaderLinePlug.None)
        {
            GeoPoint outStart = Negate(StartTangent(g));
            AddPlug(children, p.StartPlug, g.Start, outStart, strokeColor, p, ref key, "start");
        }

        // Labels.
        AddLabel(children, p.StartLabel, g.Start, style.Label, p, ref key, "s");
        AddLabel(children, p.MiddleLabel, Midpoint(g.Start, g.End), style.Label, p, ref key, "m");
        AddLabel(children, p.EndLabel, g.End, style.Label, p, ref key, "e");
    }

    private static PathElement StrokePath(ConnectorGeometry g)
        => Path2D().Set(path => path.Data = ToPathGeometry(g));

    private static void AnimateDash(Path path, LeaderLineDash dash, double unit)
    {
        var anim = new DoubleAnimation
        {
            From = 0,
            To = -((dash.Length + dash.Gap) / unit),
            Duration = new Duration(TimeSpan.FromSeconds(0.8)),
            RepeatBehavior = RepeatBehavior.Forever,
        };
        Storyboard.SetTarget(anim, path);
        Storyboard.SetTargetProperty(anim, "(Shape.StrokeDashOffset)");
        var storyboard = new Storyboard();
        storyboard.Children.Add(anim);
        storyboard.Begin();
    }

    private static void AddPlug(
        List<Element> children,
        LeaderLinePlug plug,
        GeoPoint tip,
        GeoPoint dir,
        Color color,
        LeaderLineProps p,
        ref int key,
        string tag)
    {
        var brush = new SolidColorBrush(color);
        double s = p.PlugSize;

        switch (plug)
        {
            case LeaderLinePlug.Arrow:
            {
                GeoPoint perp = new(-dir.Y, dir.X);
                double half = s * 0.5;
                var back = new GeoPoint(tip.X - (dir.X * s), tip.Y - (dir.Y * s));
                var c1 = new GeoPoint(back.X + (perp.X * half), back.Y + (perp.Y * half));
                var c2 = new GeoPoint(back.X - (perp.X * half), back.Y - (perp.Y * half));
                var geo = new PathGeometry
                {
                    Figures =
                    {
                        new PathFigure
                        {
                            StartPoint = new Point(tip.X, tip.Y),
                            IsClosed = true,
                            IsFilled = true,
                            Segments =
                            {
                                new LineSegment { Point = new Point(c1.X, c1.Y) },
                                new LineSegment { Point = new Point(c2.X, c2.Y) },
                            },
                        },
                    },
                };
                children.Add(Path2D()
                    .Set(path => path.Data = geo)
                    .Fill(brush)
                    .Opacity(p.Opacity)
                    .WithKey($"ll-plug-{tag}-{key++}"));
                break;
            }

            case LeaderLinePlug.Disc:
                children.Add(Ellipse()
                    .Width(s)
                    .Height(s)
                    .Fill(brush)
                    .Opacity(p.Opacity)
                    .Canvas(tip.X - (s / 2), tip.Y - (s / 2))
                    .WithKey($"ll-plug-{tag}-{key++}"));
                break;

            case LeaderLinePlug.Square:
                children.Add(Rectangle()
                    .Width(s)
                    .Height(s)
                    .Fill(brush)
                    .Opacity(p.Opacity)
                    .Canvas(tip.X - (s / 2), tip.Y - (s / 2))
                    .WithKey($"ll-plug-{tag}-{key++}"));
                break;
        }
    }

    private static void AddLabel(
        List<Element> children,
        string? text,
        GeoPoint at,
        Color color,
        LeaderLineProps p,
        ref int key,
        string tag)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var brush = new SolidColorBrush(color);
        children.Add(TextBlock(text!)
            .FontSize(11)
            .Opacity(p.Opacity)
            .Set(tb => tb.Foreground = brush)
            .Canvas(at.X + 6, at.Y - 16)
            .WithKey($"ll-label-{tag}-{key++}"));
    }

    private static LinearGradientBrush MakeGradient(LeaderLineGradient grad, GeoPoint start, GeoPoint end)
        => new()
        {
            MappingMode = BrushMappingMode.Absolute,
            StartPoint = new Point(start.X, start.Y),
            EndPoint = new Point(end.X, end.Y),
            GradientStops =
            {
                new GradientStop { Color = grad.Start, Offset = 0 },
                new GradientStop { Color = grad.End, Offset = 1 },
            },
        };

    internal static PathGeometry ToPathGeometry(ConnectorGeometry g)
    {
        var figure = new PathFigure { StartPoint = new Point(g.Start.X, g.Start.Y) };
        foreach (ConnectorSegment segment in g.Segments)
        {
            switch (segment)
            {
                case LineSegmentTo line:
                    figure.Segments.Add(new LineSegment { Point = new Point(line.To.X, line.To.Y) });
                    break;
                case CubicSegmentTo cubic:
                    figure.Segments.Add(new BezierSegment
                    {
                        Point1 = new Point(cubic.Control1.X, cubic.Control1.Y),
                        Point2 = new Point(cubic.Control2.X, cubic.Control2.Y),
                        Point3 = new Point(cubic.To.X, cubic.To.Y),
                    });
                    break;
            }
        }

        return new PathGeometry { Figures = { figure } };
    }

    private static GeoPoint EndTangent(ConnectorGeometry g)
    {
        if (g.Segments.Count == 0)
        {
            return new GeoPoint(1, 0);
        }

        GeoPoint prev = g.Segments.Count == 1
            ? g.Start
            : EndOf(g.Segments[^2]);

        return g.Segments[^1] switch
        {
            CubicSegmentTo cubic => Normalize(cubic.To, cubic.Control2),
            LineSegmentTo line => Normalize(line.To, prev),
            _ => new GeoPoint(1, 0),
        };
    }

    private static GeoPoint StartTangent(ConnectorGeometry g)
        => g.Segments.Count == 0
            ? new GeoPoint(1, 0)
            : g.Segments[0] switch
            {
                CubicSegmentTo cubic => Normalize(cubic.Control1, g.Start),
                LineSegmentTo line => Normalize(line.To, g.Start),
                _ => new GeoPoint(1, 0),
            };

    private static GeoPoint EndOf(ConnectorSegment s) => s switch
    {
        LineSegmentTo line => line.To,
        CubicSegmentTo cubic => cubic.To,
        _ => new GeoPoint(0, 0),
    };

    private static GeoPoint Normalize(GeoPoint to, GeoPoint from)
    {
        double dx = to.X - from.X;
        double dy = to.Y - from.Y;
        double len = Math.Sqrt((dx * dx) + (dy * dy));
        return len < 1e-6 ? new GeoPoint(1, 0) : new GeoPoint(dx / len, dy / len);
    }

    private static GeoPoint Negate(GeoPoint v) => new(-v.X, -v.Y);

    private static GeoPoint Midpoint(GeoPoint a, GeoPoint b) => new((a.X + b.X) / 2, (a.Y + b.Y) / 2);

    private static double Clamp01(double v) => Math.Clamp(v, 0, 1);
}
