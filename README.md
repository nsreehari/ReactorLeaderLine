# Reactor.Community.LeaderLine

Declarative connector lines (leader lines) for [Microsoft.UI.Reactor](https://microsoft.github.io/microsoft-ui-reactor/).

Draw an arrow, curve, or orthogonal path between two live WinUI elements — or between
raw points and areas — using a single Reactor component. The line re-measures and
re-draws itself automatically when its anchored elements move, resize, or re-layout.

> **Preview.** This is a `0.1.0-preview` release tracking Reactor `0.1.0-preview.11`.
> The public API may change before `1.0`. See [CHANGELOG.md](CHANGELOG.md).

## Install

```powershell
dotnet add package Reactor.Community.LeaderLine --prerelease
```

The package targets `net10.0-windows10.0.26100.0` and depends on
`Microsoft.UI.Reactor` and `Microsoft.WindowsAppSDK`.

## Quick start

Connect two elements. Capture each element with a `Ref` via `.Set`, then point a
`LeaderLine` at them with `ElementAnchor`:

```csharp
using Microsoft.UI.Reactor.Core;
using Reactor.Community.LeaderLine;
using static Microsoft.UI.Reactor.Factories;

public sealed class Diagram : Component
{
    public override Element Render()
    {
        var source = UseRef<FrameworkElement?>(null);
        var target = UseRef<FrameworkElement?>(null);

        return Canvas(
            Box("A").Canvas(80, 80).Set(c => source.Current = c),
            Box("B").Canvas(360, 240).Set(c => target.Current = c),

            Component<LeaderLine, LeaderLineProps>(new LeaderLineProps(
                Start: new ElementAnchor(() => source.Current),
                End:   new ElementAnchor(() => target.Current)))
        );
    }
}
```

Because the anchors resolve through a `Func<FrameworkElement?>`, you never wire up
size or position by hand — the line follows the elements.

## Anchors

| Anchor | Use it for |
| --- | --- |
| `ElementAnchor(() => refEl, socket)` | Connect to a live element; `socket` picks the side (`Auto` by default). |
| `PointAnchor(x, y)` | Connect to a fixed canvas coordinate. |
| `AreaAnchor(x, y, width, height, socket)` | Connect to an arbitrary rectangle. |
| `PointerAnchor(() => trackEl)` | Follow the mouse cursor over `trackEl` (mouse-follow endpoint). |

```csharp
new LeaderLineProps(
    Start: new PointAnchor(40, 40),
    End:   new ElementAnchor(() => target.Current, LeaderLineSocket.Left));
```

## Path styles

`LeaderLinePath` selects the routing:

```csharp
new LeaderLineProps(Start: a, End: b, Path: LeaderLinePath.Fluid);    // smooth S-curve (default)
new LeaderLineProps(Start: a, End: b, Path: LeaderLinePath.Straight); // direct line
new LeaderLineProps(Start: a, End: b, Path: LeaderLinePath.Arc);      // single curved arc
new LeaderLineProps(Start: a, End: b, Path: LeaderLinePath.Magnet);   // straight lead off each socket, then curve
new LeaderLineProps(Start: a, End: b, Path: LeaderLinePath.Grid, CornerRadius: 12); // orthogonal legs, rounded elbows
```

## Plugs, dashes, and labels

```csharp
new LeaderLineProps(
    Start: a,
    End: b,
    Color: Color.FromArgb(255, 255, 160, 0),
    Size: 2.4,
    StartPlug: LeaderLinePlug.Disc,
    EndPlug: LeaderLinePlug.Arrow,
    Dash: new LeaderLineDash(Length: 6, Gap: 4, Animate: true),
    Outline: true,
    StartLabel: "from",
    MiddleLabel: "flows to",
    EndLabel: "to");
```

- `LeaderLinePlug`: `None`, `Arrow`, `Disc`, `Square` (set `StartPlug` / `EndPlug`).
- `LeaderLineDash`: dash `Length`, `Gap`, and `Animate` for a marching-ants effect.
- `LeaderLineGradient(start, end)` and `LeaderLineDropShadow(...)` add stroke styling.
- `StartLabel` / `MiddleLabel` / `EndLabel` render text along the connector.

See [samples/](samples/Reactor.Community.LeaderLine.Sample) for a runnable app that
demonstrates every path style, animated dashes, labels, and live re-positioning.

## Documentation

- [docs/api.md](docs/api.md) — full prop and type reference.
- [docs/leader-line-mapping.md](docs/leader-line-mapping.md) — how this maps to the
  web [`leader-line`](https://github.com/anseki/leader-line) library, and where it
  intentionally differs.

## License

MIT © 2026 Reactor Community. See [LICENSE](LICENSE).

This is a community package and is not an official Microsoft product.
