namespace Reactor.Community.LeaderLine;

/// <summary>
/// How the connector routes between its two endpoints.
/// </summary>
public enum LeaderLinePath
{
    /// <summary>A single direct segment between the two endpoints.</summary>
    Straight,

    /// <summary>A symmetric curve that bows away from the straight baseline.</summary>
    Arc,

    /// <summary>
    /// A smooth cubic curve whose control points follow each endpoint's socket
    /// direction. The default routing, matching the classic leader-line "fluid" look.
    /// </summary>
    Fluid,

    /// <summary>
    /// A fluid curve that first leaves each endpoint straight along its socket before
    /// curving. Approximated as <see cref="Fluid"/> in v0.1.
    /// </summary>
    Magnet,

    /// <summary>Orthogonal (right-angle) routing composed of horizontal/vertical segments.</summary>
    Grid,
}

/// <summary>
/// Which side of an element a connector attaches to. <see cref="Auto"/> lets the
/// component choose the side facing the other endpoint.
/// </summary>
public enum LeaderLineSocket
{
    /// <summary>Choose the side that faces the other endpoint.</summary>
    Auto,

    /// <summary>Attach to the top edge.</summary>
    Top,

    /// <summary>Attach to the right edge.</summary>
    Right,

    /// <summary>Attach to the bottom edge.</summary>
    Bottom,

    /// <summary>Attach to the left edge.</summary>
    Left,
}

/// <summary>
/// The decoration drawn at an endpoint of the connector.
/// </summary>
public enum LeaderLinePlug
{
    /// <summary>No plug (a plain line end).</summary>
    None,

    /// <summary>A filled arrow head.</summary>
    Arrow,

    /// <summary>A filled circle.</summary>
    Disc,

    /// <summary>A filled square.</summary>
    Square,
}
