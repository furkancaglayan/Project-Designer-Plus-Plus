# Quick Start

Project Designer+ installs as a regular Unity asset and opens as a planning-first editor tool for pre-production boards, workflow presets, and asset-backed reference capture.

## Open The Planner

1. Import Project Designer+ into your Unity project.
2. Open Unity after the asset finishes importing.
3. Use `Tools/Project Designer/Onboarding` if you want a guided first-use flow or a starter template.
4. Use `Tools/Project Designer/Project Finder` if you want to reopen a planning board that already exists.
5. Use `Tools/Project Designer/Open Planner` when you already know which board you want to work on, or when you already have one selected.

## Understand The Three Surfaces

- `Onboarding`
  Learn how the tool works and start a board from a template.
- `Project Finder`
  Search current boards, pin favorites, and reopen recent work.
- `Planner`
  Edit one planning board at a time with cards, links, saved views, overview metrics, and details on demand.

## Use Project Finder

- Search by board name, summary, path, or board team snapshot.
- Pin the boards you revisit most.
- Reopen recent boards without hunting through the Project window.
- Open onboarding when you want a fresh board from a workflow preset.

## Use Onboarding To Pick A Starting Point

- `New Board`
  Light starting point with a pinned `Project Brief` card.
- `Project Designer+ Redo Board`
  Flagship showcase board for planner architecture, rollout work, docs, and launch prep.
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
  add cards, jump between saved views, and keep the shortcut reminder close at hand.
- Center board:
  pan, zoom, drag cards, use dotted alignment guides while moving or resizing, resize selected cards from the bottom-right handle, drag from `Link` on a card to create relationships, use `Arrange > Auto Layout Left To Right` when a board gets messy, and drop Unity assets onto the planner.
- Right inspector:
  edit the selected card or planning board details, set link anchors, and use the manual link fallback when you need more control.
- Bottom overview:
  watch planning metrics without adding a dashboard card to the board.

## Duration Estimates

Task estimates use duration text. The planner accepts mixed units and converts them through a work calendar.

| Unit | Meaning |
| --- | --- |
| `min` | minutes |
| `h` | hours |
| `d` | workdays, where `1d = 8h` |
| `w` | workweeks, where `1w = 5d` |
| `m` | work-months, where `1m = 4w` |

Examples: `1d`, `3w 1d`, `2.5h`, `30min`, and `5000min`. When the estimate parses correctly, the inspector shows the computed hours and workdays.

## Shortcuts

Alignment guides can be turned off in `Project Settings > Project Designer+ > Board Editing`.

- `F`: focus the selected card or selected cards.
- `Delete` / `Backspace`: delete selected cards.
- `Ctrl+D`: duplicate selected cards.
- `Ctrl+A`: select visible cards.
- `Esc`: cancel a link/resize/marquee action or clear the current selection.

## Built-In Card Types

- `Task`
Status, priority, duration estimate, assignee from the shared team roster, start date, due date, tags, and acceptance criteria.
- `Milestone`
  Roll-up checkpoint for linked tasks.
- `Note`
  Lightweight ideas, risks, and planning notes.
- `Reference`
  Unity assets, screenshots, image notes, text notes, and URLs.
- `Class`
  Secondary technical-design card for code structure discussions.
- `Project Brief`
Sticky context card for pitch, board team snapshot, and project knowledge.

## Asset Drag-Drop

- Drop a `TextAsset` to create a reference card.
- Drop a `Texture2D` or `Sprite` to create a visual reference card.
- Drop a `MonoScript` to create a class card.
- Open the included `Showcase Boards` sample under `Assets/Project Designer+/Samples` if you want a ready-made dense board for screenshots and team walkthroughs.
- Open the included `Status Report Extension` sample under `Assets/Project Designer+/Samples` to see how a custom importer adds a new card type.
