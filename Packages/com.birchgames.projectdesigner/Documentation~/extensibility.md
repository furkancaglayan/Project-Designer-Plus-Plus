# Extensibility

Project Designer+ v2 exposes explicit registration APIs instead of relying on reflection-heavy inheritance.

## Public Extension Points

- `ProjectDesignerRegistry`
  Central registration point for definitions, inspectors, and importers.
- `IProjectDesignerNodeDefinition`
  Declares a node type, default creation, display metadata, and preview text.
- `IProjectDesignerEdgeDefinition`
  Declares a link type and its connection rules.
- `IProjectDesignerInspector`
  Builds the right-side inspector UI for a node type.
- `IProjectDesignerAssetImporter`
  Maps dropped Unity assets into board nodes.

## Extension Flow

1. Define a custom `BoardNodeModel`.
2. Create a matching `IProjectDesignerNodeDefinition`.
3. Create a matching `IProjectDesignerInspector`.
4. Optionally create one or more `IProjectDesignerAssetImporter` implementations.
5. Register everything from an editor-only bootstrap class.

## Sample Extension

See:

- [StatusReportExtension.cs](../Samples~/StatusReportExtension/StatusReportExtension.cs)

That sample adds:

- a custom `StatusReportNodeModel`
- a node definition
- a custom inspector
- a `TextAsset` importer for assets with `status` in the name

## Design Guidance

- Keep custom nodes focused on one planning job.
- Prefer importers that create immediately useful data from dropped assets.
- Use the built-in planning model as the primary product surface and push project-specific workflows into extensions.
- Treat technical design as a secondary board mode unless your package fork is specifically targeting engineering workflows.
