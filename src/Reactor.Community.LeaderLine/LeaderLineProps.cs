using Windows.UI;

namespace Reactor.Community.LeaderLine;

/// <summary>
/// The declarative contract for a <see cref="LeaderLine"/> connector. The two required
/// anchors describe where the line starts and ends; every other member styles or
/// decorates the line. All members are immutable — change the line by re-rendering with
/// new props, the Reactor way.
/// </summary>
/// <param name="Start">Where the connector begins.</param>
/// <param name="End">Where the connector ends.</param>
/// <param name="Path">How the connector routes between its endpoints.</param>
/// <param name="Color">
/// Stroke colour. When <c>null</c> a default connector colour is used. Ignored when
/// <paramref name="Gradient"/> is set.
/// </param>
/// <param name="Size">Stroke thickness, in pixels.</param>
/// <param name="StartPlug">Decoration at the start endpoint.</param>
/// <param name="EndPlug">Decoration at the end endpoint.</param>
/// <param name="PlugSize">Nominal plug size, in pixels.</param>
/// <param name="Dash">Optional dashed / animated line effect.</param>
/// <param name="Gradient">Optional two-stop gradient stroke; overrides <paramref name="Color"/>.</param>
/// <param name="DropShadow">Optional soft shadow beneath the line.</param>
/// <param name="Outline">Draws a contrasting halo around the stroke for legibility.</param>
/// <param name="OutlineColor">Halo colour when <paramref name="Outline"/> is set.</param>
/// <param name="StartLabel">Optional text near the start endpoint.</param>
/// <param name="MiddleLabel">Optional text near the mid-point.</param>
/// <param name="EndLabel">Optional text near the end endpoint.</param>
/// <param name="Visible">Whether the connector is drawn.</param>
/// <param name="Opacity">Overall opacity, 0–1.</param>
/// <param name="CornerRadius">
/// Corner radius, in pixels, for the right-angle elbows of <see cref="LeaderLinePath.Grid"/>
/// routing. When <c>0</c> (the default) elbows are sharp; larger values fillet them.
/// Ignored by non-orthogonal routing styles.
/// </param>
/// <param name="RefreshToken">
/// A declarative refresh signal. Changing this value (for example an incrementing counter
/// or a fresh object) forces the connector to re-measure and re-route — the Reactor-native
/// replacement for an imperative "refresh now" call. Useful after an off-layout change
/// (external scroll or resize) that does not itself raise a layout pass.
/// </param>
public sealed record LeaderLineProps(
    LeaderLineAnchor Start,
    LeaderLineAnchor End,
    LeaderLinePath Path = LeaderLinePath.Fluid,
    Color? Color = null,
    double Size = 2,
    LeaderLinePlug StartPlug = LeaderLinePlug.None,
    LeaderLinePlug EndPlug = LeaderLinePlug.Arrow,
    double PlugSize = 10,
    LeaderLineDash? Dash = null,
    LeaderLineGradient? Gradient = null,
    LeaderLineDropShadow? DropShadow = null,
    bool Outline = false,
    Color? OutlineColor = null,
    string? StartLabel = null,
    string? MiddleLabel = null,
    string? EndLabel = null,
    bool Visible = true,
    double Opacity = 1,
    double CornerRadius = 0,
    object? RefreshToken = null);
