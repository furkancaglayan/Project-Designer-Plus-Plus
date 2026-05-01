# Project Designer+

Project Designer+ is a Unity editor tool for node-based pre-production planning.

The shipped Asset Store version is intended to install as a regular Unity asset inside `Assets/Project Designer` and targets `Unity 2022.3 LTS+`.

## Current Direction

- planning-first boards for indie teams
- workflow presets and richer showcase examples
- asset-backed reference capture inside Unity
- a custom UI Toolkit planner with a dedicated board browser
- public extension hooks for custom cards, inspectors, links, and importers
- technical design as a secondary workflow instead of the whole product pitch

## Repo Layout

- `Assets/Project Designer`
  The classic Unity-package layout being prepared for the shipped Asset Store asset.
- `Packages/com.birchgames.projectdesigner`
  Source-package copy and tests retained during the export branch transition.

## Getting Started

1. Open the project in Unity.
2. Use `Tools/Project Designer/Onboarding` if you want to learn the tool or start from a template.
3. Use `Tools/Project Designer/Project Finder` to reopen current boards.
4. Open the included sample content under `Assets/Project Designer/Samples` if you want to evaluate showcase boards or custom-card extensibility immediately.

## Documentation

- [Package README](Assets/Project%20Designer/README.md)
- [Quick Start](Assets/Project%20Designer/Documentation/quick-start.md)
- [First 10 Minutes](Assets/Project%20Designer/Documentation/first-10-minutes.md)
- [Extensibility](Assets/Project%20Designer/Documentation/extensibility.md)
- [Migration From v1](Assets/Project%20Designer/Documentation/migration-from-v1.md)
- [Sample Boards](Assets/Project%20Designer/Documentation/sample-boards.md)
- [Maintainer Guide](Assets/Project%20Designer%20Docs/Project%20Designer%2B%20Maintainer%20Guide.md)

## Current Status

The package now contains the v2 planner shell, workflow presets, showcase boards, a planning-board browser, onboarding, a sample extension with a demo board, and edit mode tests around the new architecture.
