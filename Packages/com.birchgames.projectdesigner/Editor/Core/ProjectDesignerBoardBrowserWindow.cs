using System;
using System.Collections.Generic;
using System.Linq;
using ProjectDesigner.V2.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class ProjectDesignerBoardBrowserWindow : EditorWindow
    {
        private string _searchQuery = string.Empty;
        private ProjectDesignerBoardFinderSortMode _sortMode;

        public static void RefreshOpenBrowsers()
        {
            ProjectDesignerBoardBrowserWindow[] windows = Resources.FindObjectsOfTypeAll<ProjectDesignerBoardBrowserWindow>();
            foreach (ProjectDesignerBoardBrowserWindow window in windows)
            {
                if (window != null)
                {
                    window.Rebuild();
                }
            }
        }

        public static void Open()
        {
            ProjectDesignerBoardBrowserWindow window = GetWindow<ProjectDesignerBoardBrowserWindow>();
            window.titleContent = new GUIContent(ProjectDesignerProductInfo.ProjectFinderWindowTitle);
            window.minSize = new Vector2(980f, 740f);
            window.Show();
            window.Rebuild();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent(ProjectDesignerProductInfo.ProjectFinderWindowTitle);
            _sortMode = ProjectDesignerSettings.instance.ProjectFinderSortMode;
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

            IReadOnlyList<ProjectDesignerBoardCatalogEntry> entries = GetFilteredEntries();

            var scrollView = new ScrollView();
            scrollView.AddToClassList("pd-onboarding-window");
            rootVisualElement.Add(scrollView);

            var container = new VisualElement();
            container.AddToClassList("pd-onboarding-content");
            scrollView.Add(container);

            container.Add(CreateHeaderCard());
            container.Add(CreateFinderControlsCard(entries.Count));
            container.Add(CreateQuickAccessCard(entries));
            container.Add(CreateBoardListCard(entries));
        }

        private IReadOnlyList<ProjectDesignerBoardCatalogEntry> GetFilteredEntries()
        {
            return ProjectDesignerBoardCatalog.SearchAndSort(
                ProjectDesignerBoardCatalog.GetBoardEntries(),
                _searchQuery,
                _sortMode);
        }

        private VisualElement CreateHeaderCard()
        {
            var card = new VisualElement();
            card.AddToClassList("pd-welcome-card");

            var eyebrow = new Label(ProjectDesignerProductInfo.ProjectFinderName);
            eyebrow.AddToClassList("pd-welcome-eyebrow");
            card.Add(eyebrow);

            var title = new Label("Open the right planning board fast");
            title.AddToClassList("pd-welcome-title");
            card.Add(title);

            var body = new Label("Search across boards, pin the ones you revisit most, jump back into recent work, or start a fresh planning board from a workflow preset.");
            body.AddToClassList("pd-welcome-body");
            card.Add(body);

            card.Add(CreateSectionLabel("Quick Create"));
            card.Add(CreateMutedBodyLabel("Start with a light board or jump directly into one of the stronger workflow presets."));

            VisualElement firstRow = CreateActionRow();
            firstRow.Add(CreatePresetButton("New Board", BoardPresetIds.Empty, ProjectDesignerProductInfo.DefaultBoardName, true));
            firstRow.Add(CreatePresetButton("Solo Indie", BoardPresetIds.SoloIndie, ProjectDesignerProductInfo.SoloBoardName, false));
            firstRow.Add(CreatePresetButton("Small Team", BoardPresetIds.SmallTeam, ProjectDesignerProductInfo.SmallTeamBoardName, false));
            firstRow.Add(CreatePresetButton("Technical Design", BoardPresetIds.TechnicalDesign, ProjectDesignerProductInfo.TechnicalBoardName, false));
            card.Add(firstRow);

            VisualElement secondRow = CreateActionRow();
            secondRow.Add(CreatePresetButton("Pitch & Vision", BoardPresetIds.PitchVision, ProjectDesignerProductInfo.PitchVisionBoardName, false));
            secondRow.Add(CreatePresetButton("Milestone Roadmap", BoardPresetIds.MilestoneRoadmap, ProjectDesignerProductInfo.MilestoneRoadmapBoardName, false));
            secondRow.Add(CreatePresetButton("Research & Reference", BoardPresetIds.ResearchReference, ProjectDesignerProductInfo.ResearchReferenceBoardName, false));
            secondRow.Add(CreatePresetButton("Stakeholder Review", BoardPresetIds.StakeholderReview, ProjectDesignerProductInfo.StakeholderReviewBoardName, false));
            card.Add(secondRow);

            card.Add(CreateSectionLabel("Team Roster"));
            card.Add(CreateRosterPreview());

            VisualElement actions = CreateActionRow();
            actions.Add(CreateActionButton("Quick Start", ProjectDesignerV2Menus.OpenQuickStartGuide, false));
            actions.Add(CreateActionButton("Project Settings", ProjectDesignerV2Menus.OpenProjectSettings, false));
            actions.Add(CreateActionButton("Open Team Roster", ProjectDesignerV2Menus.OpenTeamRoster, false));
            card.Add(actions);

            return card;
        }

        private VisualElement CreateFinderControlsCard(int resultCount)
        {
            var card = new VisualElement();
            card.AddToClassList("pd-welcome-card");

            var title = new Label("Find Boards");
            title.AddToClassList("pd-section-title");
            card.Add(title);

            var body = new Label("Search by board name, summary, path, or board team snapshot. Sorting preference is remembered for this Unity project.");
            body.AddToClassList("pd-muted-body");
            card.Add(body);

            var controlsRow = new VisualElement();
            controlsRow.AddToClassList("pd-browser-toolbar");
            card.Add(controlsRow);

            var searchField = new ToolbarSearchField();
            searchField.value = _searchQuery;
            searchField.AddToClassList("pd-browser-search");
            searchField.RegisterValueChangedCallback(evt =>
            {
                _searchQuery = evt.newValue ?? string.Empty;
                Rebuild();
            });
            controlsRow.Add(searchField);

            List<string> sortChoices = GetSortLabels();
            var sortField = new PopupField<string>(sortChoices, GetSortLabel(_sortMode));
            sortField.AddToClassList("pd-browser-sort");
            sortField.RegisterValueChangedCallback(evt =>
            {
                _sortMode = GetSortMode(evt.newValue);
                ProjectDesignerSettings.instance.SetProjectFinderSortMode(_sortMode);
                Rebuild();
            });
            controlsRow.Add(sortField);

            controlsRow.Add(CreateActionButton("Clear Search", () =>
            {
                _searchQuery = string.Empty;
                Rebuild();
            }, false));

            controlsRow.Add(CreateActionButton("Refresh", Rebuild, false));

            card.Add(CreateMutedBodyLabel(resultCount + " board" + (resultCount == 1 ? string.Empty : "s") + " match the current finder view."));
            return card;
        }

        private VisualElement CreateQuickAccessCard(IReadOnlyList<ProjectDesignerBoardCatalogEntry> entries)
        {
            var card = new VisualElement();
            card.AddToClassList("pd-welcome-card");

            var title = new Label("Quick Access");
            title.AddToClassList("pd-section-title");
            card.Add(title);

            List<ProjectDesignerBoardCatalogEntry> pinnedBoards = entries
                .Where(entry => entry.IsPinned)
                .ToList();
            List<ProjectDesignerBoardCatalogEntry> recentBoards = entries
                .Where(entry => entry.IsRecent && !entry.IsPinned)
                .OrderBy(entry => entry.RecentIndex)
                .Take(6)
                .ToList();

            if (pinnedBoards.Count == 0 && recentBoards.Count == 0)
            {
                card.Add(CreateMutedBodyLabel("Pin a few planning boards or open boards from this finder to build a quick-access layer."));
                return card;
            }

            if (pinnedBoards.Count > 0)
            {
                card.Add(CreateSubsectionLabel("Pinned Boards"));
                foreach (ProjectDesignerBoardCatalogEntry entry in pinnedBoards)
                {
                    card.Add(CreateBoardRow(entry));
                }
            }

            if (recentBoards.Count > 0)
            {
                card.Add(CreateSubsectionLabel("Recently Opened"));
                foreach (ProjectDesignerBoardCatalogEntry entry in recentBoards)
                {
                    card.Add(CreateBoardRow(entry));
                }
            }

            return card;
        }

        private VisualElement CreateBoardListCard(IReadOnlyList<ProjectDesignerBoardCatalogEntry> entries)
        {
            var card = new VisualElement();
            card.AddToClassList("pd-welcome-card");

            var title = new Label("All Matching Boards");
            title.AddToClassList("pd-section-title");
            card.Add(title);

            card.Add(CreateMutedBodyLabel("Boards are discovered through AssetDatabase and sorted by your current finder preference."));

            if (entries.Count == 0)
            {
                card.Add(new Label("No planning boards match the current finder filters yet."));
                return card;
            }

            foreach (ProjectDesignerBoardCatalogEntry entry in entries)
            {
                card.Add(CreateBoardRow(entry));
            }

            return card;
        }

        private VisualElement CreateBoardRow(ProjectDesignerBoardCatalogEntry entry)
        {
            var row = new VisualElement();
            row.AddToClassList("pd-board-browser-card");

            var headerRow = new VisualElement();
            headerRow.AddToClassList("pd-board-browser-header");
            row.Add(headerRow);

            var title = new Label(entry.BoardName);
            title.AddToClassList("pd-board-browser-title");
            headerRow.Add(title);

            var badges = new VisualElement();
            badges.AddToClassList("pd-board-browser-badges");
            headerRow.Add(badges);

            if (entry.IsPinned)
            {
                badges.Add(CreateBadge("Pinned", "pd-board-browser-badge-pinned"));
            }

            if (entry.IsRecent)
            {
                badges.Add(CreateBadge("Recent", "pd-board-browser-badge-recent"));
            }

            var path = new Label(entry.AssetPath);
            path.AddToClassList("pd-board-browser-path");
            row.Add(path);

            if (!string.IsNullOrWhiteSpace(entry.Summary))
            {
                var summary = new Label(entry.Summary);
                summary.AddToClassList("pd-board-browser-summary");
                row.Add(summary);
            }

            if (!string.IsNullOrWhiteSpace(entry.TeamSnapshot))
            {
                row.Add(CreateMutedBodyLabel("Board team snapshot: " + entry.TeamSnapshot));
            }

            var stats = new Label(
                entry.NodeCount + " cards | " +
                entry.EdgeCount + " links | " +
                entry.InProgressCount + " in progress");
            stats.AddToClassList("pd-board-browser-stats");
            row.Add(stats);

            var actions = new VisualElement();
            actions.AddToClassList("pd-board-browser-actions");
            row.Add(actions);

            string pinText = entry.IsPinned ? "Unpin" : "Pin";
            actions.Add(CreateActionButton(pinText, () =>
            {
                ProjectDesignerSettings.instance.TogglePinnedBoard(entry.Guid);
                Rebuild();
            }, false));

            actions.Add(CreateActionButton("Select", () =>
            {
                Selection.activeObject = entry.Board;
                EditorGUIUtility.PingObject(entry.Board);
            }, false));

            actions.Add(CreateActionButton("Open Planner", () =>
            {
                ProjectDesignerV2Window.Open(entry.Board);
                Rebuild();
            }, true));

            return row;
        }

        private VisualElement CreateRosterPreview()
        {
            ProjectDesignerTeamRosterAsset roster = ProjectDesignerSettings.instance.DefaultTeamRoster;
            if (roster == null)
            {
                return CreateMutedBodyLabel("No project-wide team roster is assigned yet. Create one in Project Settings to make task assignment, workload views, and filters more consistent.");
            }

            roster.EnsureDefaults();
            List<ProjectDesignerTeamMemberData> members = roster.Members
                .Where(member => member != null)
                .OrderBy(member => member.DisplayName)
                .ToList();

            var container = new VisualElement();
            container.Add(CreateMutedBodyLabel(roster.name + " | " + members.Count + " member" + (members.Count == 1 ? string.Empty : "s")));

            if (members.Count == 0)
            {
                container.Add(CreateMutedBodyLabel("The roster exists but does not have any members yet."));
                return container;
            }

            string preview = string.Join(", ", members.Take(5).Select(member => member.DisplayName).ToArray());
            if (members.Count > 5)
            {
                preview += " +" + (members.Count - 5) + " more";
            }

            container.Add(CreateMutedBodyLabel(preview));
            return container;
        }

        private Button CreatePresetButton(string label, string presetId, string boardName, bool primary)
        {
            return CreateActionButton(label, () =>
            {
                ProjectDesignerV2Menus.CreateBoard(presetId, boardName);
                Rebuild();
            }, primary);
        }

        private static Button CreateActionButton(string text, Action onClick, bool primary)
        {
            var button = new Button(() =>
            {
                if (onClick != null)
                {
                    onClick.Invoke();
                }
            })
            {
                text = text
            };
            button.AddToClassList(primary ? "pd-primary-button" : "pd-secondary-button");
            return button;
        }

        private static Label CreateBadge(string text, string extraClass)
        {
            var badge = new Label(text);
            badge.AddToClassList("pd-board-browser-badge");
            if (!string.IsNullOrEmpty(extraClass))
            {
                badge.AddToClassList(extraClass);
            }

            return badge;
        }

        private static VisualElement CreateActionRow()
        {
            var row = new VisualElement();
            row.AddToClassList("pd-action-row");
            return row;
        }

        private static Label CreateSectionLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("pd-section-title");
            return label;
        }

        private static Label CreateSubsectionLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("pd-subsection-title");
            return label;
        }

        private static Label CreateMutedBodyLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("pd-muted-body");
            return label;
        }

        private static List<string> GetSortLabels()
        {
            return new List<string>
            {
                GetSortLabel(ProjectDesignerBoardFinderSortMode.RecentlyOpened),
                GetSortLabel(ProjectDesignerBoardFinderSortMode.BoardName),
                GetSortLabel(ProjectDesignerBoardFinderSortMode.MostCards),
                GetSortLabel(ProjectDesignerBoardFinderSortMode.MostActiveWork)
            };
        }

        private static string GetSortLabel(ProjectDesignerBoardFinderSortMode sortMode)
        {
            switch (sortMode)
            {
                case ProjectDesignerBoardFinderSortMode.BoardName:
                    return "Sort: Name";
                case ProjectDesignerBoardFinderSortMode.MostCards:
                    return "Sort: Most Cards";
                case ProjectDesignerBoardFinderSortMode.MostActiveWork:
                    return "Sort: Most Active Work";
                default:
                    return "Sort: Recently Opened";
            }
        }

        private static ProjectDesignerBoardFinderSortMode GetSortMode(string label)
        {
            if (string.Equals(label, GetSortLabel(ProjectDesignerBoardFinderSortMode.BoardName), StringComparison.Ordinal))
            {
                return ProjectDesignerBoardFinderSortMode.BoardName;
            }

            if (string.Equals(label, GetSortLabel(ProjectDesignerBoardFinderSortMode.MostCards), StringComparison.Ordinal))
            {
                return ProjectDesignerBoardFinderSortMode.MostCards;
            }

            if (string.Equals(label, GetSortLabel(ProjectDesignerBoardFinderSortMode.MostActiveWork), StringComparison.Ordinal))
            {
                return ProjectDesignerBoardFinderSortMode.MostActiveWork;
            }

            return ProjectDesignerBoardFinderSortMode.RecentlyOpened;
        }
    }
}
