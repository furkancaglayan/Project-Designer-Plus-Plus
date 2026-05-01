# Migration From v2

Project Designer+ V3 is a clean-break package rewrite.

## What Changed

- v3 was asset-folder based and centered on an IMGUI node system.
- v3 is packaged as `com.birchgames.projectdesigner`.
- v3 uses a new `ProjectBoardAsset` document model.
- v3 uses UI Toolkit for the planner shell and a custom canvas/edge layer.
- v3 replaces reflection-first customization with explicit registry-driven extension points.

## Compatibility

There is no automatic importer for legacy `Project` assets in this first v3 package revision.

Legacy content remains in the repo under `Assets/ProjectDesigner+` as reference while the package rewrite is validated.

## Recommended Migration Path

1. Open your old project reference material.
2. Create a new v3 board from the closest preset.
3. Recreate high-value tasks, milestones, and references manually.
4. Rebuild any old custom nodes as explicit registry extensions.
5. Keep the old asset folder only as reference until your workflow is stable.

## Why a Clean Break

The goal of v3 is to improve:

- package distribution
- editor maintainability
- UI Toolkit-first extensibility
- testability
- product clarity for indie team pre-production workflows
