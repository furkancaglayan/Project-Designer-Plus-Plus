# Quick Start

Project Designer+ opens as a planning-first Unity editor package for pre-production boards, workflow presets, and asset-backed reference capture.

## Open The Planner

1. Open Unity with `Packages/com.birchgames.projectdesigner` available.
2. Use `Tools/Project Designer/Open Planning Boards` if you want the board browser, or `Tools/Project Designer/Open Planner` if you already have a board selected.
3. Create a new planning board from `Tools/Project Designer` or `Assets/Create/Project Designer`.

## Pick A Starting Point

- `New Board`
  Light starting point with a pinned `Project Brief` card.
- `Solo Indie Board`
  Rich showcase board for solo slice planning and references.
- `Small Team Board`
  Denser cross-discipline planning example.
- `Technical Design Board`
  Secondary architecture-focused example.
- `Pitch & Vision Board`
  Workflow preset for player promise, audience, and pitch framing.
- `Milestone Roadmap Board`
  Workflow preset for checkpoints, sequencing, and scope.
- `Research & Reference Board`
  Workflow preset for clustering inspiration and extracting decisions.
- `Stakeholder Review Board`
  Workflow preset for demo prep, risks, and review asks.

## Use The Planner

- Left sidebar:
  add cards and jump between saved views.
- Center board:
  pan, zoom, drag cards, drag from `Link` on a card to create relationships, and drop Unity assets onto the planner.
- Right inspector:
  edit the selected card or planning board details and use the manual link fallback when you need more control.
- Bottom overview:
  watch planning metrics without adding a dashboard card to the board.

## Built-In Card Types

- `Task`
  Status, priority, estimate, assignee, due date, tags, and acceptance criteria.
- `Milestone`
  Roll-up checkpoint for linked tasks.
- `Note`
  Lightweight ideas, risks, and planning notes.
- `Reference`
  Unity assets, screenshots, image notes, text notes, and URLs.
- `Class`
  Secondary technical-design card for code structure discussions.
- `Project Brief`
  Sticky context card for pitch, team snapshot, and project knowledge.

## Asset Drag-Drop

- Drop a `TextAsset` to create a reference card.
- Drop a `Texture2D` or `Sprite` to create a visual reference card.
- Drop a `MonoScript` to create a class card.
- Import the `Status Report Extension` sample to see how a custom importer adds a new card type.
