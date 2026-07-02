# Changelog

All notable changes to `Reactor.Community.LeaderLine` are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.4.0-preview.2] - 2026-07-02

Tracks Reactor `0.1.0-preview.11`.

### Fixed

- Connectors never rendered because the overlay canvas was captured via `UseRef`,
  so the measurement effect saw a null canvas on first render and never re-ran once
  the canvas was attached. The overlay canvas is now surfaced via `UseState`, which
  triggers the measurement effect to run once the canvas is attached. Connectors,
  arrows, dashes and labels now render as expected.

## [0.4.0-preview.1] - 2026-07-01

Tracks Reactor `0.1.0-preview.11`.

### Added

- Theme-aware default colours. When a connector does not set an explicit `Color` /
  `OutlineColor`, the stroke now follows the active WinUI **system accent** and the
  outline halo follows the **page surface**, resolved for the current light/dark
  scheme. The component calls `UseIsDarkTheme()` so it re-renders and re-resolves
  when the theme flips. Explicit props still win and existing usage is unchanged.
- `LeaderLineTheme` + `LeaderLineContext.Theme` — a Reactor context for supplying
  default connector styling (`Color`, `OutlineColor`, `LabelColor`) to a whole
  subtree via `.Provide(LeaderLineContext.Theme, new LeaderLineTheme(...))`.
  Resolution precedence per colour is: explicit prop → context → theme default.

## [0.3.0-preview.1] - 2026-07-01

Tracks Reactor `0.1.0-preview.11`.

### Added

- `PointerAnchor` — a mouse-follow endpoint that tracks the live cursor over a given
  element. The connector is drawn while the pointer is over the tracked element and
  hides when it leaves. Pointer input is bridged into declarative state inside the
  component's effect (with full subscription cleanup).
- `LeaderLineProps.RefreshToken` — a declarative "refresh now" signal. Changing the
  value forces the connector to re-measure and re-route, replacing the need for an
  imperative `RefreshPosition()` method (the Reactor-native way is to change a prop).

### Changed

- The dashed-stroke pattern now uses the declarative `StrokeDashArray` modifier;
  only the marching-ants animation and dash cap remain imperative `.Set` seams.

### Deferred

- Polygon area anchors remain deferred to a later preview.

## [0.2.0-preview.1] - 2026-07-01

Tracks Reactor `0.1.0-preview.11`.

### Added

- `LeaderLineProps.CornerRadius` — fillets the right-angle elbows of `Grid`
  (orthogonal) routing. Defaults to `0` (sharp corners), so existing usage is
  unchanged.

### Changed

- `Magnet` path routing is now distinct from `Fluid`: the connector leaves each
  endpoint straight along its socket for a short lead before curving, keeping the
  plug "docked" to its side.

### Deferred

- Mouse-follow / hover anchor, polygon area anchors, and an imperative
  `RefreshPosition` API remain deferred to a later preview.

## [0.1.0-preview.1] - 2026-07-01

Initial preview, tracking Reactor `0.1.0-preview.11`.

### Added

- `LeaderLine` component (`Component<LeaderLine, LeaderLineProps>`) that draws a
  connector between two anchors and re-measures automatically on layout changes.
- Anchors: `ElementAnchor` (element + socket), `PointAnchor` (fixed coordinate),
  and `AreaAnchor` (arbitrary rectangle).
- Path styles: `Straight`, `Arc`, `Fluid` (default), and `Grid` (orthogonal).
  `Magnet` is accepted and currently routes as `Fluid`.
- Sockets: `Auto`, `Top`, `Right`, `Bottom`, `Left`, with dominant-axis auto side
  selection.
- Plugs: `None`, `Arrow`, `Disc`, `Square` for both ends.
- Stroke styling: solid color, `Size`, `Outline` halo, `LeaderLineDash`
  (with animated marching-ants), `LeaderLineGradient`, and `LeaderLineDropShadow`.
- Labels: `StartLabel`, `MiddleLabel`, `EndLabel`.
- `Visible` and `Opacity` props.
- Sample app under `samples/` and geometry unit tests under `tests/`.

### Known limitations / deferred to a later preview

- `Magnet` path routing is not yet distinct from `Fluid`.
- No mouse-follow / hover anchor.
- Area anchors are rectangular only (no polygon anchors).
- No imperative `RefreshPosition` API — repositioning is driven by layout updates.
