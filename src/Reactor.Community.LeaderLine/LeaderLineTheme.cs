using Microsoft.UI.Reactor.Core;
using Windows.UI;

namespace Reactor.Community.LeaderLine;

/// <summary>
/// Default connector styling supplied through Reactor context. Provide it with
/// <c>.Provide(LeaderLineContext.Theme, new LeaderLineTheme(...))</c> to set defaults for
/// every <see cref="LeaderLine"/> in a subtree without threading props through each one.
/// <para>
/// Every member is optional. Resolution precedence for each colour is: the explicit
/// <see cref="LeaderLineProps"/> value first, then this context value, then the value
/// derived from the active WinUI theme (system accent for the stroke, the page surface
/// for the outline halo). A <c>null</c> member simply falls through to the theme default.
/// </para>
/// </summary>
/// <param name="Color">Default stroke colour when a line does not set its own.</param>
/// <param name="OutlineColor">Default halo colour when a line enables <see cref="LeaderLineProps.Outline"/>.</param>
/// <param name="LabelColor">Default label colour; when <c>null</c> labels follow the stroke colour.</param>
public sealed record LeaderLineTheme(
    Color? Color = null,
    Color? OutlineColor = null,
    Color? LabelColor = null);

/// <summary>
/// Reactor context handles for <see cref="LeaderLine"/>. Use <see cref="Theme"/> to supply
/// default connector styling to a subtree.
/// </summary>
public static class LeaderLineContext
{
    /// <summary>
    /// Default connector styling for a subtree. Consumed by every <see cref="LeaderLine"/>
    /// beneath the nearest <c>.Provide(LeaderLineContext.Theme, ...)</c>; defaults to an
    /// empty theme (all colours fall through to the active WinUI theme).
    /// </summary>
    public static readonly Context<LeaderLineTheme> Theme = new(new LeaderLineTheme());
}
