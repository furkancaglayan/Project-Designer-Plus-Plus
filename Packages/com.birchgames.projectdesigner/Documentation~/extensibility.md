# Extensibility

Project Designer+ v2 exposes explicit registration APIs instead of reflection-heavy inheritance.

## Public Extension Points

- `ProjectDesignerRegistry`
  Registers card definitions, link definitions, inspectors, and importers.
- `IProjectDesignerNodeDefinition`
  Declares card metadata, accent color, default size, and preview text.
- `IProjectDesignerEdgeDefinition`
  Declares link rules and link styling.
- `IProjectDesignerInspector`
  Builds the right-side inspector UI for a card type.
- `IProjectDesignerAssetImporter`
  Turns dropped Unity assets into planner cards.

## Recommended Flow

1. Define a custom `BoardNodeModel`.
2. Create a matching `IProjectDesignerNodeDefinition`.
3. Create a matching `IProjectDesignerInspector`.
4. Optionally create one or more `IProjectDesignerAssetImporter` implementations.
5. Register everything from an editor-only bootstrap class.
6. Open a demo board that already uses the new card, so the extension is easy to inspect in context.

## Sample Extension

See:

- `Assets/ProjectDesigner+/Samples/StatusReportExtension/StatusReportExtension.cs`
- `Assets/ProjectDesigner+/Samples/StatusReportExtension/Status Report Demo Board.asset`
- `Assets/ProjectDesigner+/Samples/StatusReportExtension/README.md`

That sample adds:

- a custom `StatusReportNodeModel`
- a node definition with its own accent color and preview
- a custom inspector
- a `TextAsset` importer for assets with `status` in the name
- a demo planning board that already uses the custom card

In the source repository, that sample is maintained under `Packages/com.birchgames.projectdesigner/Samples~`, but the shipped Asset Store version includes it directly inside the project under `Assets/ProjectDesigner+/Samples`.

## What To Verify In The Sample

- the card appears in the planner beside built-in cards
- the card uses the custom inspector
- the custom importer creates the expected card when you drop matching assets
- the planner does not require any core-package edits to surface the extension

## Design Guidance

- Keep custom cards focused on one planning job.
- Use `AccentColor` as the visual identity hook for card buttons and card chrome.
- Prefer importers that create immediately useful planning data from dropped assets.
- Keep technical workflows secondary unless you are intentionally forking the package toward a more engineering-specific tool.
