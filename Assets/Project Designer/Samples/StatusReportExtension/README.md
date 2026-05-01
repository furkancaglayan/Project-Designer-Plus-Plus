# Status Report Extension Sample

This sample shows the full custom-node path for Project Designer+:

- a custom `StatusReportNodeModel`
- a matching node definition
- a custom inspector
- a text-asset importer
- a demo board that already uses the custom card on canvas

## What To Open

Open `Status Report Demo Board.asset`.

Use that board to inspect:

- how the custom `Status Report` card appears beside built-in planning cards
- how the custom inspector edits health, summary, and risks
- how links between the sample card and built-in cards render in the planner
- how the sample can live beside built-in presets without modifying core package code

## Try It Yourself

1. Open the board and select the `Status Report` card.
2. Edit the health state and summary in the right inspector.
3. Drag in a `TextAsset` with `status` in the name to trigger the sample importer.
4. Review [StatusReportExtension.cs](StatusReportExtension.cs) to see how the model, definition, inspector, importer, and registration fit together.
