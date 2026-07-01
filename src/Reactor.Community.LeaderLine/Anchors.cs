using System;
using Microsoft.UI.Xaml;

namespace Reactor.Community.LeaderLine;

/// <summary>
/// Where a connector endpoint attaches. All coordinates are expressed in the
/// coordinate space of the <see cref="LeaderLine"/> overlay (the container it renders
/// into). Anchor a connector to a live element with <see cref="ElementAnchor"/>, or to
/// fixed geometry with <see cref="PointAnchor"/> / <see cref="AreaAnchor"/>.
/// </summary>
public abstract record LeaderLineAnchor;

/// <summary>
/// Anchors a connector endpoint to a live WinUI element. The element's bounds are
/// measured relative to the overlay each time the connector is positioned, so the line
/// tracks the element as it moves or resizes.
/// </summary>
/// <param name="Resolve">
/// Returns the current element, or <c>null</c> if it has not materialized yet. Pass a
/// closure over a Reactor ref, e.g. <c>() =&gt; myRef.Current</c>.
/// </param>
/// <param name="Socket">Which side of the element to attach to.</param>
public sealed record ElementAnchor(Func<FrameworkElement?> Resolve, LeaderLineSocket Socket = LeaderLineSocket.Auto)
    : LeaderLineAnchor
{
    /// <summary>Anchors directly to an already-materialized element.</summary>
    public ElementAnchor(FrameworkElement element, LeaderLineSocket socket = LeaderLineSocket.Auto)
        : this(() => element, socket)
    {
    }
}

/// <summary>Anchors a connector endpoint to a fixed point in the overlay's coordinate space.</summary>
public sealed record PointAnchor(double X, double Y) : LeaderLineAnchor;

/// <summary>
/// Anchors a connector endpoint to the live pointer position while the pointer is over
/// <paramref name="Track"/>. The endpoint follows the cursor; the connector is not drawn
/// until the pointer first moves over the tracked element, and hides again when the
/// pointer leaves it. Coordinates are taken relative to the connector overlay, so the
/// tracked element should share the overlay's coordinate space (typically the same
/// container). Give the tracked element a non-<c>null</c> background (e.g. transparent)
/// so it receives pointer input across its whole surface.
/// </summary>
/// <param name="Track">
/// The element whose pointer movement drives the endpoint. Pass a closure over a Reactor
/// ref: <c>() =&gt; rootRef.Current</c>.
/// </param>
public sealed record PointerAnchor(Func<FrameworkElement?> Track) : LeaderLineAnchor
{
    /// <summary>Tracks the pointer over an already-materialized element.</summary>
    public PointerAnchor(FrameworkElement element)
        : this(() => element)
    {
    }
}

/// <summary>
/// Anchors a connector endpoint to a fixed rectangle in the overlay's coordinate space.
/// The attachment side is chosen from <paramref name="Socket"/>.
/// </summary>
public sealed record AreaAnchor(
    double X,
    double Y,
    double Width,
    double Height,
    LeaderLineSocket Socket = LeaderLineSocket.Auto) : LeaderLineAnchor;
