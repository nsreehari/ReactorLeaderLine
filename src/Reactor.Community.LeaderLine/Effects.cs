using Windows.UI;

namespace Reactor.Community.LeaderLine;

/// <summary>
/// An animated dashed-line effect. When <see cref="Animate"/> is set the dash pattern
/// marches along the connector to suggest flow.
/// </summary>
/// <param name="Length">Length of each dash, in pixels.</param>
/// <param name="Gap">Length of the gap between dashes, in pixels.</param>
/// <param name="Animate">Whether the dash pattern animates along the line.</param>
public sealed record LeaderLineDash(double Length = 4, double Gap = 3, bool Animate = false);

/// <summary>
/// A two-stop gradient stroke running from the start plug colour to the end plug colour.
/// </summary>
/// <param name="Start">Colour at the start endpoint.</param>
/// <param name="End">Colour at the end endpoint.</param>
public sealed record LeaderLineGradient(Color Start, Color End);

/// <summary>
/// A soft drop shadow rendered beneath the connector.
/// </summary>
/// <param name="Blur">Blur radius, in pixels.</param>
/// <param name="OffsetX">Horizontal shadow offset, in pixels.</param>
/// <param name="OffsetY">Vertical shadow offset, in pixels.</param>
/// <param name="Opacity">Shadow opacity, 0–1.</param>
public sealed record LeaderLineDropShadow(double Blur = 3, double OffsetX = 2, double OffsetY = 2, double Opacity = 0.35);
