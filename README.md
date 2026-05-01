# Project Designer+

Project Designer+ is a Unity editor tool for node-based pre-production planning.

The shipped Asset Store version is intended to install as a regular Unity package inside `Assets/ProjectDesigner+` and targets `Unity 2022.3 LTS+`.

## Current Direction

- planning-first boards for indie teams
- workflow presets and richer showcase examples
- asset-backed reference capture inside Unity
- a custom UI Toolkit planner with a dedicated board browser
- public extension hooks for custom cards, inspectors, links, and importers
- technical design as a secondary workflow instead of the whole product pitch

## Repo Layout

- `Packages/com.birchgames.projectdesigner`
  The source package, docs, samples, tests, and editor tooling used to build the shipped asset.
- `Assets/ProjectDesigner+`
  Legacy v1 content kept in the repo as reference during the transition.

## Getting Started

1. Open the project in Unity.
2. Use `Tools/Project Designer/Onboarding` if you want to learn the tool or start from a template.
3. Use `Tools/Project Designer/Project Finder` to reopen current boards.
4. Open the included sample content under `Assets/ProjectDesigner+/Samples` if you want to evaluate showcase boards or custom-card extensibility immediately.

## Documentation

- [Package README](Packages/com.birchgames.projectdesigner/README.md)
- [Quick Start](Packages/com.birchgames.projectdesigner/Documentation~/quick-start.md)
- [First 10 Minutes](Packages/com.birchgames.projectdesigner/Documentation~/first-10-minutes.md)
- [Extensibility](Packages/com.birchgames.projectdesigner/Documentation~/extensibility.md)
- [Migration From v1](Packages/com.birchgames.projectdesigner/Documentation~/migration-from-v1.md)
- [Sample Boards](Packages/com.birchgames.projectdesigner/Documentation~/sample-boards.md)
- [Maintainer Guide](Assets/Project%20Designer%20Docs/Project%20Designer%2B%20Maintainer%20Guide.md)

## Current Status

The package now contains the v2 planner shell, workflow presets, showcase boards, a planning-board browser, onboarding, a sample extension with a demo board, and edit mode tests around the new architecture.
