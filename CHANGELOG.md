# Changelog

All notable changes to `Reactor.Community.LeaderLine` are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
