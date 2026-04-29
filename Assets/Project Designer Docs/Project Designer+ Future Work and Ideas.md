# Project Designer+ Future Work and Ideas

This document is the maintainer-side backlog and idea bank for Project Designer+.

It is intentionally separate from the shipped package docs. Use it to capture things that may be worth doing later without letting them quietly turn into release scope.

## Current Position

Project Designer+ now has the core shape of a usable pre-production planner:

- onboarding for learning and template starts
- Project Finder for reopening boards
- Planner for actual board editing
- built-in planning cards, saved views, filters, workload/risk/dependency signals, and sample boards

That means future work should be judged against a higher bar than before.

### Default rule

Prefer:

- release quality
- clarity
- strong examples
- small usability wins

Avoid adding broad new systems unless they clearly improve the product more than docs, polish, or store readiness would.

## Prioritization Rules

When choosing future work, prefer ideas that are:

1. high impact for solo and small-team planning
2. low risk to the current data model
3. low risk to the current extension API
4. easy to explain in onboarding and store media

Be cautious with changes that:

- add more always-visible chrome
- complicate the board model
- split product responsibilities again
- require schema migration
- break `IProjectDesignerNodeDefinition` or `IProjectDesignerEdgeDefinition`

## Near-Term Usability Backlog

These are the strongest low-cost candidates after release work.

### Inspector and shell polish

- Add a real color-picker UX for editable user data such as team-roster accent colors and note accent colors, while still storing hex under the hood.
- Consider an overlay details rail if the current manual panel still feels too layout-heavy when opened.
- Keep tightening field alignment and spacing in the right panel where long labels or dense link rows still feel cramped.
- Make active filters and quick filters even more obvious when they are applied, especially when multiple filter sources stack.
- Consider a shorter or dismissible canvas hint once users are familiar with the tool.

### Board scanning and clarity

- Add clearer “why am I only seeing these cards?” feedback when search, saved view, category, or quick filter is active.
- Improve empty states for:
  - no saved views
  - no roster assigned
  - no board selected
  - no valid links available
- Revisit whether a global compact presentation mode is useful, without doing true per-card collapse yet.

### Theme and visual consistency

- Audit dark/light parity carefully after all release docs and screenshots are final.
- Replace remaining plain string-based editable accent inputs with safer UI where users are expected to author colors directly.

## Medium-Term Product Improvements

These are useful, but should wait until after release polish and store work.

### Sharing and review

- Read-only stakeholder review mode
- Export board snapshot
- Export summary/report for milestone, risks, and ownership
- Better “review packet” flow from a board

### Board operations and layout

- Smarter auto-layout follow-up that understands clusters, major milestones, and cleaner lane spacing
- Optional grouping, sections, or swimlanes if real planning boards begin to need stronger structure
- Better edge filtering or focus modes for dense boards

### Project Finder follow-up

- Revisit live search if Unity UI Toolkit makes it stable enough later
- More explicit recent/pinned/recommended sections if board counts grow
- Better metadata display when many boards exist in one project

## Larger Ideas To Revisit Carefully

These are valid ideas, but they should be treated as separate product decisions, not casual follow-ups.

### Collaboration and team workflows

- comments
- review requests
- approvals
- shared or synchronized boards
- cloud or multi-user workflows

These could be valuable, but they move the tool beyond its current local-editor-first product shape.

### Reporting and planning depth

- richer scheduling views
- cross-board dashboards
- burndown or progress reports
- ownership heatmaps
- stronger milestone forecasting

These should only be added if they remain understandable for small teams and do not overload the Planner surface.

### Technical design expansion

- deeper UML support
- richer class/member editing
- more technical relation types
- script/code analysis tie-ins

Keep in mind that technical design is intentionally secondary in the current product story.

## Ideas To Avoid Unless A Strong Need Appears

- New card types just because they are possible
- Splitting planning and technical design into separate tools too early
- More toolbar chrome
- Complex per-card collapse without a larger rendering/layout rethink
- Breaking the extension interfaces for cosmetic reasons alone
- Reintroducing mixed responsibilities between onboarding, finder, and planner

## Release-After Checklist

After the current docs/store pass is done, reassess future work with real usage in mind:

1. Which actions still feel awkward during a full planning session?
2. Which questions can the board not answer quickly enough?
3. Which parts of onboarding or Project Finder are still misunderstood?
4. Which features are users actually asking for versus what merely sounds nice?

Only then should new backlog items move from this doc into actual implementation plans.

## Good First Candidates After Release

If there is room for one or two post-release improvements, the safest bets are:

- color picker UX for editable accent values
- clearer active-filter visibility near search/view controls
- overlay details rail only if the current manual rail still annoys in practice
- review/export surfaces if they help the product sell and demo better
