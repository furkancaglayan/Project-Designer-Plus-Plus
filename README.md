# Project Designer+

Project Designer+ is now being rebuilt as a UPM-first Unity editor package for node-based pre-production planning.

The active package lives at [Packages/com.birchgames.projectdesigner](Packages/com.birchgames.projectdesigner) and targets `Unity 2022.3 LTS+`. It focuses on:

- milestone and task planning for indie teams
- reference capture from Unity assets
- a UI Toolkit workspace with a custom board canvas
- public extension hooks for custom nodes, inspectors, edges, and asset importers
- technical design as a secondary workflow instead of the entire product pitch

## Repo Layout

- `Packages/com.birchgames.projectdesigner`
  The new package, docs, samples, tests, and editor tooling for the v2 rewrite.
- `Assets/ProjectDesigner+`
  Legacy v1 asset content kept in the repo as reference during the transition.

## Getting Started

1. Open the project in Unity.
2. Use `Tools/Project Designer/Open Workspace` or create a board from the `Tools/Project Designer` menu.
3. Work from one of the packaged starter boards:
   `Empty`, `Solo Indie`, `Small Team`, or `Technical Design`.

## Documentation

- [Package README](Packages/com.birchgames.projectdesigner/README.md)
- [Quick Start](Packages/com.birchgames.projectdesigner/Documentation~/quick-start.md)
- [First 10 Minutes](Packages/com.birchgames.projectdesigner/Documentation~/first-10-minutes.md)
- [Extensibility](Packages/com.birchgames.projectdesigner/Documentation~/extensibility.md)
- [Migration From v1](Packages/com.birchgames.projectdesigner/Documentation~/migration-from-v1.md)
- [Sample Boards](Packages/com.birchgames.projectdesigner/Documentation~/sample-boards.md)
- [Asset Store Listing Draft](Packages/com.birchgames.projectdesigner/Documentation~/asset-store-listing.md)

## Current Status

This repository now contains the v2 package foundation, built-in planning definitions, preset board generation, the UI Toolkit workspace shell, a sample extension, and edit mode tests.

The repo still includes the older asset implementation under `Assets/ProjectDesigner+` while the package rewrite is completed and validated inside Unity.
