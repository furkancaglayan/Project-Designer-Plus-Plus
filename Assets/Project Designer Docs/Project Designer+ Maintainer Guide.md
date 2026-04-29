# Project Designer+ Maintainer Guide

This guide is for working on the package inside this Unity project. It is not part of the shipped package docs.

## Product Model

Project Designer+ is split into three user-facing surfaces:

- `Onboarding`
  Learn the tool and start from templates.
- `Project Finder`
  Reopen current boards, search, and pin boards.
- `Planner`
  Edit a single planning board.

That separation is intentional. Avoid letting one surface do all three jobs again.

## Package Layout

- `Packages/com.birchgames.projectdesigner/Runtime`
  Data model, board document, command stack, built-in node models, board insights, layout helpers, and public extension interfaces.
- `Packages/com.birchgames.projectdesigner/Editor/Core`
  Planner shell, onboarding, Project Finder, settings provider, menus, theme config, and shared editor utilities.
- `Packages/com.birchgames.projectdesigner/Editor/BuiltIn`
  Built-in card definitions, inspectors, templates, presentation helpers, and importers.
- `Packages/com.birchgames.projectdesigner/Samples~`
  Public sample content, including showcase boards and the custom status-report extension sample.
- `Packages/com.birchgames.projectdesigner/Tests/Editor`
  Edit mode tests for document behavior, board insights, presets, shell helpers, and extension registration.

## High-Level Data Flow

### Opening windows

- `ProjectDesignerV2Window`
  Main planner editor window.
- `ProjectDesignerOnboardingWindow`
  Guided first-use and template-picking surface.
- `ProjectDesignerBoardBrowserWindow`
  Project Finder for current boards.

When no board is open, `ProjectDesignerV2Window` now shows a lightweight planner-start surface instead of template selection.

### Working with a board

- `ProjectBoardAsset`
  Root ScriptableObject for a planning board.
- `BoardDocument`
  Holds nodes, edges, saved filters, templates, and view state.
- `BoardViewState`
  Stores pan, zoom, selection, search, category, quick filter, and editor interaction state that belongs to the board.
- `ProjectDesignerCommandStack`
  All meaningful board mutations should flow through commands for undo/redo consistency.

### Planner shell

- `ProjectDesignerWorkspaceView`
  Main 3-pane planner shell.
- `BoardCanvasView`
  Grid, pan/zoom, selection, drag, links, drag-drop, and board context menu behavior.
- `BoardNodeView`
  Single card rendering and card-level interaction state.
- `ProjectDesignerOverviewView`
  Bottom insights strip and quick-filter actions.

## Current UX Direction

The planner is intentionally moving toward:

- less always-visible chrome
- more contextual actions
- more stable layout behavior
- clearer surface responsibilities

Current shell rules worth preserving:

- left library rail is manual-only
- right details rail is manual-only
- collapsed rails use stable labels
- context menus and shortcuts carry high-frequency actions
- Project Finder is for existing boards, not new-board templates
- onboarding is for templates and first-use guidance

## Built-In Card System

Built-in card definitions and inspectors live under `Editor/BuiltIn`.

Important ideas:

- built-in models live in `Runtime`
- built-in rendering and editing live in `Editor/BuiltIn`
- card preview text should stay calm and scan-friendly
- technical design remains supported, but secondary to planning

Key built-in planning cards:

- `Task`
- `Milestone`
- `Note`
- `Reference`
- `Project Brief`
- `Class` as optional technical-design support

## Extension Model

Public extension hooks live in the runtime/editor package surface and are registry-based, not reflection-heavy inheritance.

Important interfaces:

- `IProjectDesignerNodeDefinition`
- `IProjectDesignerEdgeDefinition`
- `IProjectDesignerInspector`
- `IProjectDesignerAssetImporter`

Registry entry point:

- `ProjectDesignerRegistry`

Reference sample:

- `Samples~/StatusReportExtension`

Use that sample when checking whether API changes would break third-party customization.

## Board Discovery And Settings

- `ProjectDesignerSettings`
  Project-level editor settings, finder preferences, onboarding behavior, theme, and default team roster reference.
- `ProjectDesignerSettingsProvider`
  Project Settings UI.
- `ProjectDesignerBoardCatalog`
  AssetDatabase-backed discovery for current board assets.

Settings should stay configuration-focused. Board browsing belongs in Project Finder.

## Tests

The edit mode suite is in:

- `Packages/com.birchgames.projectdesigner/Tests/Editor/ProjectDesignerPackageTests.cs`

The tests are strongest around:

- document cloning and serialization
- commands and undo/redo
- board insights and quick filters
- preset generation
- registry and sample-extension behavior
- editor helper logic that is deterministic

Avoid brittle UI tests for purely visual shell behavior unless the logic is extracted into testable helpers first.

## Docs Strategy

There are now two doc audiences:

- package docs in `Documentation~`
  user-facing guidance
- this guide in `Assets`
  maintainer-facing architecture and workflow notes

If you change the product shape, update both sides when needed:

- user docs for visible workflow changes
- maintainer docs for structural/architectural changes

## Good Next Changes

If the planner still feels heavy, the strongest next usability moves are likely:

- shorter/dismissible canvas hinting
- stronger active-filter visibility
- more intentional empty states
- overlay details rail only if manual layout shift still feels annoying

Avoid broad new feature work unless it clearly beats usability/stability work for impact.
