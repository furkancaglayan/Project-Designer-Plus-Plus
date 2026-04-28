using System.Collections.Generic;
using ProjectDesigner.V2.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class ProjectDesignerBoardBrowserWindow : EditorWindow
    {
        public static void Open()
        {
            ProjectDesignerBoardBrowserWindow window = GetWindow<ProjectDesignerBoardBrowserWindow>();
            window.titleContent = new GUIContent(ProjectDesignerProductInfo.PlanningBoardsWindowTitle);
            window.minSize = new Vector2(920f, 700f);
            window.Show();
            window.Rebuild();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent(ProjectDesignerProductInfo.PlanningBoardsWindowTitle);
            Rebuild();
        }

        private void OnProjectChange()
        {
            Rebuild();
        }

        public void RefreshTheme()
        {
            Rebuild();
        }

        private void Rebuild()
        {
            if (rootVisualElement == null)
            {
                return;
            }

            ProjectDesignerThemeConfig.ApplyTheme(rootVisualElement);
            rootVisualElement.Clear();

            var scrollView = new ScrollView();
            scrollView.AddToClassList("pd-onboarding-window");
            rootVisualElement.Add(scrollView);

            var container = new VisualElement();
            container.AddToClassList("pd-onboarding-content");
            scrollView.Add(container);

            container.Add(CreateHeaderCard());
            container.Add(CreateBoardListCard());
        }

        private VisualElement CreateHeaderCard()
        {
            var card = new VisualElement();
            card.AddToClassList("pd-welcome-card");

            var eyebrow = new Label(ProjectDesignerProductInfo.PlanningBoardsName);
            eyebrow.AddToClassList("pd-welcome-eyebrow");
            card.Add(eyebrow);

            var title = new Label("Open a planning board or start from a workflow preset");
            title.AddToClassList("pd-welcome-title");
            card.Add(title);

            var body = new Label("Use this browser to jump between boards quickly, create a fresh workflow preset, or open one of the richer showcase boards.");
            body.AddToClassList("pd-welcome-body");
            card.Add(body);

            card.Add(CreatePresetButton("New Board", "Light starting point with a pinned project brief card.", BoardPresetIds.Empty, ProjectDesignerProductInfo.DefaultBoardName));
            card.Add(CreatePresetButton("Solo Indie Board", "Showcase board for solo planning, slice risks, and reference capture.", BoardPresetIds.SoloIndie, ProjectDesignerProductInfo.SoloBoardName));
            card.Add(CreatePresetButton("Small Team Board", "Cross-discipline example for design, production, engineering, and art.", BoardPresetIds.SmallTeam, ProjectDesignerProductInfo.SmallTeamBoardName));
            card.Add(CreatePresetButton("Technical Design Board", "Architecture-focused example that keeps technical design secondary to planning.", BoardPresetIds.TechnicalDesign, ProjectDesignerProductInfo.TechnicalBoardName));
            card.Add(CreatePresetButton("Pitch & Vision Board", "Workflow preset for player promise, target audience, and pitch framing.", BoardPresetIds.PitchVision, ProjectDesignerProductInfo.PitchVisionBoardName));
            card.Add(CreatePresetButton("Milestone Roadmap Board", "Workflow preset for checkpoint planning and release-shaping discussions.", BoardPresetIds.MilestoneRoadmap, ProjectDesignerProductInfo.MilestoneRoadmapBoardName));
            card.Add(CreatePresetButton("Research & Reference Board", "Workflow preset for inspiration gathering, synthesis, and research tasks.", BoardPresetIds.ResearchReference, ProjectDesignerProductInfo.ResearchReferenceBoardName));
            card.Add(CreatePresetButton("Stakeholder Review Board", "Workflow preset for demo prep, risks, and review readiness.", BoardPresetIds.StakeholderReview, ProjectDesignerProductInfo.StakeholderReviewBoardName));

            var actions = new VisualElement();
            actions.AddToClassList("pd-action-row");
            card.Add(actions);

            var quickStartButton = new Button(ProjectDesignerV2Menus.OpenQuickStartGuide) { text = "Quick Start" };
            quickStartButton.AddToClassList("pd-secondary-button");
            actions.Add(quickStartButton);

            var settingsButton = new Button(ProjectDesignerV2Menus.OpenProjectSettings) { text = "Project Settings" };
            settingsButton.AddToClassList("pd-secondary-button");
            actions.Add(settingsButton);

            return card;
        }

        private VisualElement CreateBoardListCard()
        {
            var card = new VisualElement();
            card.AddToClassList("pd-welcome-card");

            var title = new Label("Current Planning Boards");
            title.AddToClassList("pd-section-title");
            card.Add(title);

            var body = new Label("All planning boards found through AssetDatabase are listed here.");
            body.AddToClassList("pd-muted-body");
            card.Add(body);

            IReadOnlyList<ProjectBoardAsset> boards = ProjectDesignerBoardCatalog.GetBoards();
            foreach (ProjectBoardAsset board in boards)
            {
                card.Add(CreateBoardRow(board));
            }

            if (boards.Count == 0)
            {
                card.Add(new Label("No planning boards were found in this project yet."));
            }

            return card;
        }

        private VisualElement CreateBoardRow(ProjectBoardAsset board)
        {
            var row = new VisualElement();
            row.AddToClassList("pd-board-browser-card");

            var title = new Label(board.Document.BoardName);
            title.AddToClassList("pd-board-browser-title");
            row.Add(title);

            var path = new Label(AssetDatabase.GetAssetPath(board));
            path.AddToClassList("pd-board-browser-path");
            row.Add(path);

            var summary = new Label(board.Document.Summary);
            summary.AddToClassList("pd-board-browser-summary");
            row.Add(summary);

            var stats = new Label(
                board.Document.Nodes.Count + " cards | " +
                board.Document.Edges.Count + " links | " +
                BoardInsights.CountTasksByStatus(board.Document, TaskNodeStatus.InProgress) + " in progress");
            stats.AddToClassList("pd-board-browser-stats");
            row.Add(stats);

            var actions = new VisualElement();
            actions.AddToClassList("pd-board-browser-actions");
            row.Add(actions);

            var selectButton = new Button(() =>
            {
                Selection.activeObject = board;
                EditorGUIUtility.PingObject(board);
            })
            {
                text = "Select"
            };
            selectButton.AddToClassList("pd-secondary-button");
            actions.Add(selectButton);

            var openButton = new Button(() => ProjectDesignerV2Window.Open(board))
            {
                text = "Open Planner"
            };
            openButton.AddToClassList("pd-primary-button");
            actions.Add(openButton);

            return row;
        }

        private static VisualElement CreatePresetButton(string title, string description, string presetId, string boardName)
        {
            var row = new VisualElement();
            row.AddToClassList("pd-welcome-preset");

            var label = new Label(title);
            label.AddToClassList("pd-welcome-preset-title");
            row.Add(label);

            var descriptionLabel = new Label(description);
            descriptionLabel.AddToClassList("pd-welcome-preset-description");
            row.Add(descriptionLabel);

            var button = new Button(() => ProjectDesignerV2Menus.CreateBoard(presetId, boardName)) { text = title };
            button.AddToClassList("pd-primary-button");
            row.Add(button);

            return row;
        }
    }
}
