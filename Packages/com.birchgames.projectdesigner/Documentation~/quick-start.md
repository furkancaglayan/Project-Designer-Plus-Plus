# Quick Start

Project Designer+ opens as a node-based workspace for planning game pre-production directly inside Unity.

## Create Your First Board

1. Open Unity with the package available in `Packages/com.birchgames.projectdesigner`.
2. Use `Tools/Project Designer/New Board` or one of the starter presets.
3. Double-click the created `ProjectBoardAsset` to open the workspace.

## Use the Workspace

- Left sidebar:
  add built-in node types and apply saved filters.
- Center canvas:
  pan, zoom, drag nodes, and drop Unity assets onto the board.
- Right inspector:
  edit the selected node or board details and create links between nodes.
- Bottom overview:
  monitor task, milestone, reference, and technical node counts.

## Built-In Node Types

- `Task`
  Backlog item with status, priority, estimate, assignee, due date, tags, and acceptance notes.
- `Milestone`
  Roll-up checkpoint that tracks linked task completion.
- `Note`
  Lightweight idea capture and pre-production notes.
- `Reference`
  Linked Unity assets, text snippets, image references, and URLs.
- `Class`
  Secondary technical-design node for class and ownership planning.

## Asset Drag-Drop

- Drop a `TextAsset` onto the board to create a reference node.
- Drop a `Texture2D` or `Sprite` to create a visual reference node.
- Drop a `MonoScript` to create a class node from script metadata.

## Suggested Starting Preset

Use `Solo Indie Board` if you are testing the package alone, or `Small Team Board` if you want to evaluate the planning flow with explicit team roles.
