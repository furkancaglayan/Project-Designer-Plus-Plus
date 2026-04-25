using System;
using ProjectDesigner.V2.Data;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class ProjectDesignerWelcomeView : VisualElement
    {
        public ProjectDesignerWelcomeView(Action<string, string> createPreset, Action openSelectedBoard)
        {
            AddToClassList("pd-welcome");

            var card = new VisualElement();
            card.AddToClassList("pd-welcome-card");
            Add(card);

            var eyebrow = new Label("Project Designer+");
            eyebrow.AddToClassList("pd-welcome-eyebrow");
            card.Add(eyebrow);

            var title = new Label("Node-based pre-production planning for indie Unity teams");
            title.AddToClassList("pd-welcome-title");
            card.Add(title);

            var body = new Label("Start from a ready-made board, capture references from Unity assets, map milestones and tasks, and keep technical design as a secondary workflow instead of the entire product.");
            body.AddToClassList("pd-welcome-body");
            card.Add(body);

            card.Add(CreatePresetButton("New Empty Board", "Build your own board structure from scratch.", BoardPresetIds.Empty, "Project Board", createPreset));
            card.Add(CreatePresetButton("Solo Indie Board", "Vision, slice planning, and reference capture for a solo project.", BoardPresetIds.SoloIndie, "Solo Indie Board", createPreset));
            card.Add(CreatePresetButton("Small Team Board", "Feature planning and shared reference workflows for a small team.", BoardPresetIds.SmallTeam, "Small Team Board", createPreset));
            card.Add(CreatePresetButton("Technical Design Board", "Secondary workflow for class maps and implementation planning.", BoardPresetIds.TechnicalDesign, "Technical Design Board", createPreset));

            var openButton = new Button(openSelectedBoard) { text = "Open Selected Board" };
            openButton.AddToClassList("pd-secondary-button");
            card.Add(openButton);
        }

        private static VisualElement CreatePresetButton(string title, string description, string presetId, string boardName, Action<string, string> createPreset)
        {
            var row = new VisualElement();
            row.AddToClassList("pd-welcome-preset");

            var label = new Label(title);
            label.AddToClassList("pd-welcome-preset-title");
            row.Add(label);

            var descriptionLabel = new Label(description);
            descriptionLabel.AddToClassList("pd-welcome-preset-description");
            row.Add(descriptionLabel);

            var button = new Button(() => createPreset(presetId, boardName)) { text = title };
            button.AddToClassList("pd-primary-button");
            row.Add(button);
            return row;
        }
    }
}
