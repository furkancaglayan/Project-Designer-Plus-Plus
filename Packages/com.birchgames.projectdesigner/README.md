# Project Designer+

Project Designer+ is a node-based pre-production planner for indie Unity teams.

It ships as a UPM-friendly editor package with a custom UI Toolkit planner, typed planning-board documents, built-in workflow presets, asset-driven reference capture, and explicit extension hooks for custom cards.

## Highlights

- Plan tasks, milestones, notes, references, project context, and technical design in one board.
- Use onboarding to learn the tool and start from templates, then use `Project Finder` to search, pin, and reopen current boards quickly.
- Start from showcase boards and workflow presets, not just empty templates.
- Inspect the flagship `Project Designer+ Redo` board to see how the planner can carry a real package rewrite and launch-prep effort.
- Use board operations like multi-select, align/distribute, and edge-aware left-to-right auto layout to clean up busy boards quickly.
- Drag Unity assets into the planner to generate reference or class cards.
- Extend the package with your own cards, links, inspectors, and importers.

## Package Layout

- `Runtime`
  Board document model, card types, command stack, registry contracts, and package-facing APIs.
- `Editor`
  UI Toolkit planner, board browser, onboarding, built-in definitions, inspectors, importers, and board tooling.
- `Samples~/StatusReportExtension`
  Example custom card, inspector, importer, README, and demo board built on the public registry.
- `Samples~/ShowcaseBoards`
  Built-in demo content, including the `Project Designer+ Redo Demo Board`.
- `Tests/Editor`
  Edit mode tests for document behavior, presets, commands, importers, board discovery, and extension registration.
- `Documentation~`
  Versioned docs and Asset Store listing drafts.

## Minimum Version

`Unity 2022.3 LTS`

## Entry Points

- `Tools/Project Designer/Open Planner`
- `Tools/Project Designer/Project Finder`
- `Tools/Project Designer/Onboarding`
- `Tools/Project Designer/Documentation/Quick Start`
- `Tools/Project Designer/Project Settings`
- `Assets/Create/Project Designer/...` for direct board and roster asset creation

## Documentation

- [Quick Start](Documentation~/quick-start.md)
- [First 10 Minutes](Documentation~/first-10-minutes.md)
- [Extensibility](Documentation~/extensibility.md)
- [Migration From v1](Documentation~/migration-from-v1.md)
- [Sample Boards](Documentation~/sample-boards.md)
- [Asset Store Listing Draft](Documentation~/asset-store-listing.md)
