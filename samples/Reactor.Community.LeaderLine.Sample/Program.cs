// Reactor.Community.LeaderLine — sample app.
// Shows connectors between live elements, several path/plug styles, an animated
// dashed line, and automatic re-positioning when an anchored element moves.

using Microsoft.UI;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Reactor.Community.LeaderLine;
using Windows.UI;
using static Microsoft.UI.Reactor.Factories;

ReactorApp.Run<SampleApp>("Reactor.Community.LeaderLine", width: 900, height: 620);

internal sealed class SampleApp : Component
{
    private static readonly Color Indigo = Color.FromArgb(255, 92, 107, 192);
    private static readonly Color Teal = Color.FromArgb(255, 0, 150, 136);
    private static readonly Color Amber = Color.FromArgb(255, 255, 160, 0);

    public override Element Render()
    {
        var boxSource = UseRef<FrameworkElement?>(null);
        var boxTarget = UseRef<FrameworkElement?>(null);
        var boxNote = UseRef<FrameworkElement?>(null);
        var (targetY, setTargetY) = UseState(150.0);

        Element Box(string label, double x, double y, Ref<FrameworkElement?> box, Color color)
            => Border(TextBlock(label)
                    .FontSize(14)
                    .Set(tb => tb.Foreground = new SolidColorBrush(Colors.White)))
                .Width(150)
                .Height(64)
                .Padding(16, 20)
                .Background(new SolidColorBrush(color))
                .CornerRadius(10)
                .Canvas(x, y)
                .Set(control => box.Current = control);

        return Canvas(
            TextBlock("Reactor.Community.LeaderLine")
                .FontSize(18)
                .Canvas(40, 20),
            Button("Move target", () => setTargetY(targetY > 120 ? 320.0 : 150.0))
                .Canvas(40, 54),

            Box("Source", 140, 220, boxSource, Indigo),
            Box("Target", 560, targetY + 160, boxTarget, Teal),
            Box("Note", 140, 420, boxNote, Amber),

            Component<LeaderLine, LeaderLineProps>(new LeaderLineProps(
                Start: new ElementAnchor(() => boxSource.Current),
                End: new ElementAnchor(() => boxTarget.Current),
                Path: LeaderLinePath.Fluid,
                EndPlug: LeaderLinePlug.Arrow,
                Color: Indigo,
                Size: 2.4,
                MiddleLabel: "fluid")),

            Component<LeaderLine, LeaderLineProps>(new LeaderLineProps(
                Start: new ElementAnchor(() => boxSource.Current, LeaderLineSocket.Bottom),
                End: new ElementAnchor(() => boxNote.Current, LeaderLineSocket.Top),
                Path: LeaderLinePath.Grid,
                EndPlug: LeaderLinePlug.Disc,
                Dash: new LeaderLineDash(6, 4, Animate: true),
                Color: Amber,
                Size: 2,
                CornerRadius: 14,
                MiddleLabel: "grid • rounded")),

            Component<LeaderLine, LeaderLineProps>(new LeaderLineProps(
                Start: new ElementAnchor(() => boxNote.Current, LeaderLineSocket.Right),
                End: new ElementAnchor(() => boxTarget.Current, LeaderLineSocket.Bottom),
                Path: LeaderLinePath.Magnet,
                EndPlug: LeaderLinePlug.Arrow,
                Color: Teal,
                Size: 2,
                MiddleLabel: "magnet",
                Outline: true))
        );
    }
}
