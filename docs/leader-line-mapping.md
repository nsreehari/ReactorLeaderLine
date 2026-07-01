# Mapping from the web `leader-line` library

This package is inspired by the popular web library
[`leader-line`](https://github.com/anseki/leader-line) by anseki, but it is a
Reactor-native reimagining rather than a port. The web library is imperative
(`new LeaderLine(startEl, endEl, options)` plus `line.position()`); this package is
declarative — you describe the connector as props and Reactor keeps it in sync.

## Concept mapping

| web `leader-line` | `Reactor.Community.LeaderLine` |
| --- | --- |
| `new LeaderLine(start, end, opts)` | `Component<LeaderLine, LeaderLineProps>(new LeaderLineProps(...))` |
| `LeaderLine.pointAnchor(el, {x, y})` | `PointAnchor(x, y)` |
| `LeaderLine.areaAnchor(el, {...})` | `AreaAnchor(x, y, width, height)` |
| `LeaderLine.mouseHoverAnchor(el)` | `PointerAnchor(() => el)` (mouse-follow) |
| `line.position()` (manual refresh) | automatic — driven by `LayoutUpdated`; force one by changing `RefreshToken` |
| `path: 'straight' \| 'arc' \| 'fluid' \| 'magnet' \| 'grid'` | `LeaderLinePath` enum |
| `socketGravity` / `socket` | `LeaderLineSocket` + internal gravity heuristics |
| `startPlug` / `endPlug` (`arrow`, `disc`, `square`, `behind`) | `LeaderLinePlug` (`Arrow`, `Disc`, `Square`, `None`) |
| `dash: { len, gap, animation }` | `LeaderLineDash(Length, Gap, Animate)` |
| `gradient: { startColor, endColor }` | `LeaderLineGradient(Start, End)` |
| `dropShadow` | `LeaderLineDropShadow(...)` |
| `startLabel` / `middleLabel` / `endLabel` | `StartLabel` / `MiddleLabel` / `EndLabel` |
| `hide()` / `show()` | `Visible` prop |
| `outline` | `Outline` + `OutlineColor` |

## Intentional differences

- **Declarative, not imperative.** There is no `position()`, `show()`, or `remove()`
  to call. You change props (or the elements move) and the line updates. When an
  off-layout change needs a nudge, change the `RefreshToken` prop instead of calling an
  imperative refresh. This matches Reactor's model and removes the class of bugs where a
  line drifts because a manual refresh was missed.
- **Anchors resolve lazily.** `ElementAnchor` takes a `Func<FrameworkElement?>` so a
  connector can be declared before its endpoints exist, and safely no-ops until they
  are measured.
- **Coordinate space is a shared `Canvas`.** The web library positions against the
  document `body`; here the connector overlay lives in the same `Canvas` as its
  anchors, so `PointAnchor`/`AreaAnchor` are in that canvas's coordinates.
- **A subset of plugs/paths in preview.** The plug set is
  `None`/`Arrow`/`Disc`/`Square`. See the
  [CHANGELOG](../CHANGELOG.md) for deferred items.

## Not (yet) implemented

- Polygon area anchors.
