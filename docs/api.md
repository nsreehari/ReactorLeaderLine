# API reference

`Reactor.Community.LeaderLine` exposes one component and a small set of records and
enums. Everything lives in the `Reactor.Community.LeaderLine` namespace.

## Component

### `LeaderLine : Component<LeaderLineProps>`

Host it like any Reactor class component:

```csharp
Component<LeaderLine, LeaderLineProps>(new LeaderLineProps(Start: a, End: b))
```

The component renders a transparent, hit-test-invisible `Canvas` overlay and draws
the connector into it. It subscribes to `LayoutUpdated` and recomputes geometry only
when the resolved endpoints actually move (within a 0.5px tolerance), so it will not
spin in a layout loop.

Place a `LeaderLine` as a sibling of the elements it connects, inside a shared
`Canvas`, so their coordinate spaces line up.

## `LeaderLineProps`

`sealed record LeaderLineProps`

| Prop | Type | Default | Description |
| --- | --- | --- | --- |
| `Start` | `LeaderLineAnchor` | — (required) | Where the line begins. |
| `End` | `LeaderLineAnchor` | — (required) | Where the line ends. |
| `Path` | `LeaderLinePath` | `Fluid` | Routing style. |
| `Color` | `Color?` | `null` (themed accent) | Stroke color. When `null`, follows the active theme's system accent, then any `LeaderLineContext.Theme` value. |
| `Size` | `double` | `2` | Stroke thickness. |
| `StartPlug` | `LeaderLinePlug` | `None` | Marker at the start. |
| `EndPlug` | `LeaderLinePlug` | `Arrow` | Marker at the end. |
| `PlugSize` | `double` | `10` | Plug marker size. |
| `Dash` | `LeaderLineDash?` | `null` | Dashed / animated stroke. |
| `Gradient` | `LeaderLineGradient?` | `null` | Two-stop gradient stroke. |
| `DropShadow` | `LeaderLineDropShadow?` | `null` | Soft offset underlay. |
| `Outline` | `bool` | `false` | Draw a contrasting halo behind the stroke. |
| `OutlineColor` | `Color?` | `null` | Halo color when `Outline` is set. When `null`, follows the active theme's page surface, then any `LeaderLineContext.Theme` value. |
| `StartLabel` | `string?` | `null` | Text near the start. |
| `MiddleLabel` | `string?` | `null` | Text near the midpoint. |
| `EndLabel` | `string?` | `null` | Text near the end. |
| `Visible` | `bool` | `true` | Hide without unmounting. |
| `Opacity` | `double` | `1` | Overall opacity. |
| `CornerRadius` | `double` | `0` | Fillets the right-angle elbows of `Grid` routing. Ignored by other styles. |
| `RefreshToken` | `object?` | `null` | Change this value to force a re-measure/re-route (declarative "refresh now"). |

## Theming and context

When a connector does not set an explicit `Color` / `OutlineColor`, colors are derived
from the active WinUI theme via `UseIsDarkTheme()` (the component re-renders when the
scheme flips). Precedence per color is: explicit prop → `LeaderLineContext.Theme` →
theme default.

### `LeaderLineTheme`

`sealed record LeaderLineTheme(Color? Color = null, Color? OutlineColor = null, Color? LabelColor = null)`

Default connector styling for a subtree. Every member is optional; a `null` member
falls through to the theme-derived default. `LabelColor` defaults to the resolved
stroke color.

### `LeaderLineContext.Theme`

`static readonly Context<LeaderLineTheme>` — provide it to set defaults for all
connectors beneath a point in the tree:

```csharp
content.Provide(LeaderLineContext.Theme, new LeaderLineTheme(Color: brandColor));
```

| Theme default | Source resource | Fallback |
| --- | --- | --- |
| Stroke | `AccentFillColorDefaultBrush` (system accent) | indigo |
| Outline halo | `SolidBackgroundFillColorBaseBrush` (page surface) | white / near-black by scheme |

## Anchors

`abstract record LeaderLineAnchor`

### `ElementAnchor`

```csharp
new ElementAnchor(Func<FrameworkElement?> Resolve, LeaderLineSocket Socket = Auto)
new ElementAnchor(FrameworkElement element, LeaderLineSocket socket = Auto)
```

Resolves a live element each recompute. If the resolver returns `null`, or the
element has not been measured yet (`ActualWidth`/`ActualHeight` ≤ 0), that endpoint is
skipped until the next layout pass.

### `PointAnchor`

```csharp
new PointAnchor(double X, double Y)
```

A fixed coordinate in the shared canvas space.

### `AreaAnchor`

```csharp
new AreaAnchor(double X, double Y, double Width, double Height, LeaderLineSocket Socket = Auto)
```

An arbitrary rectangle; the socket picks which side the line attaches to.

### `PointerAnchor`

```csharp
new PointerAnchor(Func<FrameworkElement?> Track)
new PointerAnchor(FrameworkElement element)
```

A mouse-follow endpoint: it tracks the live cursor over `Track`. The connector is
drawn while the pointer is over the tracked element and hides when it leaves. Give the
tracked element a non-`null` background (e.g. transparent) so it receives pointer moves
across its whole surface, and use an element that shares the overlay's coordinate space.

## Enums

### `LeaderLinePath`

`Straight`, `Arc`, `Fluid`, `Magnet`, `Grid`.
`Fluid` is a smooth S-curve; `Grid` produces orthogonal right-angle legs (fillet
them with `CornerRadius`).
`Magnet` leaves each endpoint straight along its socket for a short lead, then
curves — keeping the plug docked to its side.

### `LeaderLineSocket`

`Auto`, `Top`, `Right`, `Bottom`, `Left`.
`Auto` chooses the side facing the other anchor by dominant axis.

### `LeaderLinePlug`

`None`, `Arrow`, `Disc`, `Square`.

## Effect records

```csharp
record LeaderLineDash(double Length = 4, double Gap = 3, bool Animate = false);
record LeaderLineGradient(Color Start, Color End);
record LeaderLineDropShadow(double Blur = 3, double OffsetX = 2, double OffsetY = 2, double Opacity = 0.35);
```

`Colors` come from `Windows.UI.Color`.
