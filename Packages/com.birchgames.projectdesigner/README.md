# Project Designer+

Project Designer+ is a node-based pre-production planner for indie Unity teams.

It ships as an editor package with a custom UI Toolkit workspace, typed board documents, built-in planning nodes, asset-driven reference capture, and explicit extension hooks for custom workflows.

## Highlights

- Plan milestones, tasks, notes, references, and technical design in one board.
- Drag Unity assets into the canvas to generate reference or class nodes.
- Use a command-stack-driven board model for undo, redo, testing, and persistence.
- Start from curated board presets: `Empty`, `Solo Indie`, `Small Team`, and `Technical Design`.
- Extend the package with your own nodes, edges, inspectors, and importers.

## Package Layout

- `Runtime`
  Board document model, node types, command stack, registry contracts, and package-facing APIs.
- `Editor`
  UI Toolkit workspace, built-in definitions, inspectors, importers, onboarding, and board asset tooling.
- `Samples~/StatusReportExtension`
  Example custom node, inspector, and asset importer built on the public registry.
- `Tests/Editor`
  Edit mode tests for document behavior, presets, commands, importers, and extension registration.
- `Documentation~`
  Versioned docs and Asset Store listing drafts.

## Minimum Version

`Unity 2022.3 LTS`

## Entry Points

- `Tools/Project Designer/Open Workspace`
- `Tools/Project Designer/New Board`
- `Tools/Project Designer/New Solo Indie Board`
- `Tools/Project Designer/New Small Team Board`
- `Tools/Project Designer/New Technical Design Board`

## Documentation

- [Quick Start](Documentation~/quick-start.md)
- [First 10 Minutes](Documentation~/first-10-minutes.md)
- [Extensibility](Documentation~/extensibility.md)
- [Migration From v1](Documentation~/migration-from-v1.md)
- [Sample Boards](Documentation~/sample-boards.md)
- [Asset Store Listing Draft](Documentation~/asset-store-listing.md)
