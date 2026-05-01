# Migration From v1

Project Designer+ `v3.0.0` is a clean-break rewrite.

## What Changed

- earlier versions used the older IMGUI-based editor architecture
- `v3.0.0` uses a new `ProjectBoardAsset` document model
- `v3.0.0` uses UI Toolkit for the planner shell and a custom canvas/edge layer
- `v3.0.0` replaces reflection-first customization with explicit registry-driven extension points
- `v3.0.0` is installed as a regular Unity asset under `Assets/Project Designer+`

## Compatibility

There is no automatic importer for legacy `Project` assets in this first `v3.0.0` release.

## Recommended Migration Path

1. Open your old project reference material.
2. Create a new `v3.0.0` board from the closest preset.
3. Recreate high-value tasks, milestones, and references manually.
4. Rebuild any old custom nodes as explicit registry extensions.
5. Keep the old version open as reference until your new workflow is stable.

## Why a Clean Break

The goal of v3 is to improve:

- asset-store delivery clarity
- editor maintainability
- UI Toolkit-first extensibility
- testability
- product clarity for indie team pre-production workflows
