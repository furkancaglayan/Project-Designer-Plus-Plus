# Project Designer+

Project Designer+ is being rebuilt as a UPM-first Unity editor package for node-based pre-production planning.

The active package lives at [Packages/com.birchgames.projectdesigner](Packages/com.birchgames.projectdesigner) and targets `Unity 2022.3 LTS+`.

## Current Direction

- planning-first boards for indie teams
- workflow presets and richer showcase examples
- asset-backed reference capture inside Unity
- a custom UI Toolkit planner with a dedicated board browser
- public extension hooks for custom cards, inspectors, links, and importers
- technical design as a secondary workflow instead of the whole product pitch

## Repo Layout

- `Packages/com.birchgames.projectdesigner`
  The active package, docs, samples, tests, and editor tooling.
- `Assets/ProjectDesigner+`
  Legacy v1 content kept in the repo as reference during the transition.

## Getting Started

1. Open the project in Unity.
2. Use `Tools/Project Designer/Onboarding` if you want to learn the tool or start from a template.
3. Use `Tools/Project Designer/Project Finder` to reopen current boards.
4. Import the `Status Report Extension` sample if you want to evaluate custom-card extensibility immediately.

## Documentation

- [Package README](Packages/com.birchgames.projectdesigner/README.md)
- [Quick Start](Packages/com.birchgames.projectdesigner/Documentation~/quick-start.md)
- [First 10 Minutes](Packages/com.birchgames.projectdesigner/Documentation~/first-10-minutes.md)
- [Extensibility](Packages/com.birchgames.projectdesigner/Documentation~/extensibility.md)
- [Migration From v1](Packages/com.birchgames.projectdesigner/Documentation~/migration-from-v1.md)
- [Sample Boards](Packages/com.birchgames.projectdesigner/Documentation~/sample-boards.md)
- [Asset Store Listing Draft](Packages/com.birchgames.projectdesigner/Documentation~/asset-store-listing.md)
- [Maintainer Guide](Assets/Project%20Designer%20Docs/Project%20Designer%2B%20Maintainer%20Guide.md)

## Current Status

The package now contains the v2 planner shell, workflow presets, showcase boards, a planning-board browser, onboarding, a sample extension with a demo board, and edit mode tests around the new architecture.
