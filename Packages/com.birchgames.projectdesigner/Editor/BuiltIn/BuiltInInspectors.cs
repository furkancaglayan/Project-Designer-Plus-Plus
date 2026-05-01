using System;
using System.Collections.Generic;
using System.Linq;
using ProjectDesigner.V2.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.BuiltIn
{
    internal static class BuiltInInspectorUtility
    {
        private const string FoldoutSessionKeyPrefix = "ProjectDesigner.V2.BuiltInFoldout.";

        public static void AddDelayedTextField(VisualElement parent, string label, string value, Action<string> onCommit, bool multiline = false)
        {
            var field = new TextField(label);
            field.value = value ?? string.Empty;
            field.isDelayed = true;
            field.multiline = multiline;
            if (multiline)
            {
                field.style.minHeight = 72f;
            }

            field.RegisterValueChangedCallback(evt => onCommit(evt.newValue));
            parent.Add(field);
        }

        public static void AddIntegerField(VisualElement parent, string label, int value, Action<int> onCommit)
        {
            var field = new IntegerField(label);
            field.value = value;
            field.isDelayed = true;
            field.RegisterValueChangedCallback(evt => onCommit(evt.newValue));
            parent.Add(field);
        }

        public static void AddEnumField<TEnum>(VisualElement parent, string label, TEnum value, Action<TEnum> onCommit) where TEnum : Enum
        {
            var field = new EnumField(label, value);
            field.RegisterValueChangedCallback(evt => onCommit((TEnum)evt.newValue));
            parent.Add(field);
        }

        public static void AddTagsField(VisualElement parent, BoardNodeModel node, ProjectBoardAsset board, IBoardCommandDispatcher dispatcher, Action repaint)
        {
            AddDelayedTextField(parent, "Tags", node.GetTagsCsv(), value =>
            {
                BoardNodeModel updated = node.Clone();
                updated.SetTagsFromCsv(value);
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });
        }

        public static void AddSectionTitle(VisualElement parent, string title)
        {
            var label = new Label(title);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginTop = 10f;
            label.style.marginBottom = 4f;
            parent.Add(label);
        }

        public static Foldout CreateFoldout(string title, bool expanded = false)
        {
            var foldout = new Foldout
            {
                text = title,
                value = expanded
            };
            foldout.AddToClassList("pd-foldout");
            return foldout;
        }

        public static Foldout CreatePersistentFoldout(string stateKey, string title, bool expanded = false)
        {
            string sessionKey = FoldoutSessionKeyPrefix + (string.IsNullOrWhiteSpace(stateKey) ? title : stateKey);
            Foldout foldout = CreateFoldout(title, SessionState.GetBool(sessionKey, expanded));
            foldout.RegisterValueChangedCallback(evt => SessionState.SetBool(sessionKey, evt.newValue));
            return foldout;
        }
    }

    internal readonly struct AssigneeChoice
    {
        public string Id { get; }
        public string Label { get; }

        public AssigneeChoice(string id, string label)
        {
            Id = id ?? string.Empty;
            Label = string.IsNullOrWhiteSpace(label) ? "Unassigned" : label.Trim();
        }
    }

    internal sealed class TaskNodeInspector : IProjectDesignerInspector
    {
        public string NodeTypeId { get { return BoardNodeTypeIds.Task; } }
        public int Priority { get { return 100; } }

        public VisualElement BuildInspector(ProjectBoardAsset board, BoardNodeModel node, IBoardCommandDispatcher dispatcher, Action repaint)
        {
            var task = node as TaskNodeModel;
            var root = new VisualElement();
            if (task == null)
            {
                return root;
            }

            BuiltInInspectorUtility.AddDelayedTextField(root, "Title", task.Title, value =>
            {
                TaskNodeModel updated = (TaskNodeModel)task.Clone();
                updated.Title = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });

            BuiltInInspectorUtility.AddDelayedTextField(root, "Description", task.Description, value =>
            {
                TaskNodeModel updated = (TaskNodeModel)task.Clone();
                updated.Description = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            }, true);

            ProjectDesignerTeamRosterAsset roster = ProjectDesignerTeamRosterContext.CurrentRoster;
            if (roster == null)
            {
                root.Add(new HelpBox("Assign a default team roster in Project Settings to pick assignees from a shared project-wide list.", HelpBoxMessageType.Info));

                var openSettingsButton = new Button(() => SettingsService.OpenProjectSettings(ProjectDesignerProductInfo.SettingsPath))
                {
                    text = "Open Project Settings"
                };
                openSettingsButton.AddToClassList("pd-secondary-button");
                root.Add(openSettingsButton);
            }
            else
            {
                roster.EnsureDefaults();
                ProjectDesignerTeamMemberData resolvedMember = ProjectDesignerTeamRosterResolver.ResolveMember(roster, task.AssigneeId);
                string selectedAssigneeId = resolvedMember == null ? (task.AssigneeId ?? string.Empty) : resolvedMember.Id;
                bool isUnmappedAssignee = !string.IsNullOrWhiteSpace(task.AssigneeId) && resolvedMember == null;
                bool hasInactiveSelectedAssignee = resolvedMember != null && !resolvedMember.IsActive;

                List<AssigneeChoice> choices = BuildAssigneeChoices(roster, selectedAssigneeId, resolvedMember, isUnmappedAssignee, hasInactiveSelectedAssignee);
                int selectedIndex = Mathf.Max(0, choices.FindIndex(choice => string.Equals(choice.Id, selectedAssigneeId, StringComparison.OrdinalIgnoreCase)));
                var assigneeField = new PopupField<string>("Assignee", choices.Select(choice => choice.Label).ToList(), selectedIndex);
                assigneeField.RegisterValueChangedCallback(evt =>
                {
                    int choiceIndex = choices.FindIndex(choice => choice.Label == evt.newValue);
                    if (choiceIndex < 0)
                    {
                        return;
                    }

                    TaskNodeModel updated = (TaskNodeModel)task.Clone();
                    updated.AssigneeId = choices[choiceIndex].Id;
                    dispatcher.Execute(new UpdateNodeCommand(board, updated));
                    repaint();
                });
                root.Add(assigneeField);

                if (isUnmappedAssignee)
                {
                    root.Add(new HelpBox("This task points to a missing roster member id. Pick a team member from the dropdown or clear the assignment.", HelpBoxMessageType.Warning));
                }
            }

            BuiltInInspectorUtility.AddEnumField(root, "Status", task.Status, value =>
            {
                TaskNodeModel updated = (TaskNodeModel)task.Clone();
                updated.Status = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });

            Foldout advancedFoldout = BuiltInInspectorUtility.CreatePersistentFoldout(task.Id + ".Advanced", "Advanced", false);
            var advancedNote = new Label("Use this section for scheduling, sizing, and acceptance details.");
            advancedNote.AddToClassList("pd-muted-body");
            advancedFoldout.Add(advancedNote);

            BuiltInInspectorUtility.AddEnumField(advancedFoldout, "Priority", task.Priority, value =>
            {
                TaskNodeModel updated = (TaskNodeModel)task.Clone();
                updated.Priority = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });

            BuiltInInspectorUtility.AddIntegerField(advancedFoldout, "Estimate", task.EstimatePoints, value =>
            {
                TaskNodeModel updated = (TaskNodeModel)task.Clone();
                updated.EstimatePoints = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });

            BuiltInInspectorUtility.AddDelayedTextField(advancedFoldout, "Due Date (YYYY-MM-DD)", task.DueDateIso, value =>
            {
                TaskNodeModel updated = (TaskNodeModel)task.Clone();
                updated.DueDateIso = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });
            if (!string.IsNullOrWhiteSpace(task.DueDateIso) && !BoardInsights.TryParseDate(task.DueDateIso, out _))
            {
                advancedFoldout.Add(new HelpBox("Use a valid date like 2026-05-15 so timeline and milestone health signals stay accurate.", HelpBoxMessageType.Warning));
            }

            BuiltInInspectorUtility.AddDelayedTextField(advancedFoldout, "Acceptance", task.AcceptanceCriteria, value =>
            {
                TaskNodeModel updated = (TaskNodeModel)task.Clone();
                updated.AcceptanceCriteria = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            }, true);

            BuiltInInspectorUtility.AddTagsField(advancedFoldout, task, board, dispatcher, repaint);
            root.Add(advancedFoldout);
            return root;
        }

        private static List<AssigneeChoice> BuildAssigneeChoices(ProjectDesignerTeamRosterAsset roster, string selectedAssigneeId, ProjectDesignerTeamMemberData selectedMember, bool includeUnmappedChoice, bool includeInactiveSelectedChoice)
        {
            var choices = new List<AssigneeChoice>
            {
                new AssigneeChoice(string.Empty, "Unassigned")
            };

            if (includeUnmappedChoice)
            {
                choices.Add(new AssigneeChoice(selectedAssigneeId, "Missing roster member"));
            }

            if (includeInactiveSelectedChoice && selectedMember != null)
            {
                choices.Add(new AssigneeChoice(selectedMember.Id, selectedMember.DisplayName + " - inactive"));
            }

            List<ProjectDesignerTeamMemberData> members = roster.Members
                .Where(member => member != null && member.IsActive)
                .OrderBy(member => member.DisplayName)
                .ToList();
            Dictionary<string, int> labelCounts = members
                .GroupBy(member => BuildMemberLabel(member), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

            foreach (ProjectDesignerTeamMemberData member in members)
            {
                string label = BuildMemberLabel(member);
                if (labelCounts[label] > 1)
                {
                    label += " - " + member.Id;
                }

                choices.Add(new AssigneeChoice(member.Id, label));
            }

            return choices;
        }

        private static string BuildMemberLabel(ProjectDesignerTeamMemberData member)
        {
            string label = member == null ? "Team Member" : member.DisplayName;
            if (member == null)
            {
                return label;
            }

            if (!string.IsNullOrWhiteSpace(member.Role))
            {
                return label + " - " + member.Role.Trim();
            }

            if (!string.IsNullOrWhiteSpace(member.Discipline))
            {
                return label + " - " + member.Discipline.Trim();
            }

            return label;
        }
    }

    internal sealed class ProjectBriefNodeInspector : IProjectDesignerInspector
    {
        public string NodeTypeId { get { return BoardNodeTypeIds.ProjectBrief; } }
        public int Priority { get { return 100; } }

        public VisualElement BuildInspector(ProjectBoardAsset board, BoardNodeModel node, IBoardCommandDispatcher dispatcher, Action repaint)
        {
            var brief = node as ProjectBriefNodeModel;
            var root = new VisualElement();
            if (brief == null)
            {
                return root;
            }

            BuiltInInspectorUtility.AddDelayedTextField(root, "Title", brief.Title, value =>
            {
                ProjectBriefNodeModel updated = (ProjectBriefNodeModel)brief.Clone();
                updated.Title = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });

            BuiltInInspectorUtility.AddDelayedTextField(root, "Overview", brief.Overview, value =>
            {
                ProjectBriefNodeModel updated = (ProjectBriefNodeModel)brief.Clone();
                updated.Overview = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            }, true);

            Foldout contextFoldout = BuiltInInspectorUtility.CreatePersistentFoldout(brief.Id + ".BoardContext", "Board Context", false);

            BuiltInInspectorUtility.AddDelayedTextField(contextFoldout, "Board Team Snapshot", brief.TeamSnapshot, value =>
            {
                ProjectBriefNodeModel updated = (ProjectBriefNodeModel)brief.Clone();
                updated.TeamSnapshot = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            }, true);

            BuiltInInspectorUtility.AddDelayedTextField(contextFoldout, "Project Knowledge", brief.ProjectKnowledge, value =>
            {
                ProjectBriefNodeModel updated = (ProjectBriefNodeModel)brief.Clone();
                updated.ProjectKnowledge = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            }, true);

            var syncButton = new Button(() =>
            {
                ProjectBriefNodeModel updated = (ProjectBriefNodeModel)brief.Clone();
                updated.Overview = board.Document.Summary;
                updated.TeamSnapshot = string.Join(", ", board.Document.TeamMembers);
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            })
            {
                text = "Sync From Board Context"
            };
            syncButton.AddToClassList("pd-secondary-button");
            contextFoldout.Add(syncButton);

            BuiltInInspectorUtility.AddTagsField(contextFoldout, brief, board, dispatcher, repaint);
            root.Add(contextFoldout);
            return root;
        }
    }

    internal sealed class MilestoneNodeInspector : IProjectDesignerInspector
    {
        public string NodeTypeId { get { return BoardNodeTypeIds.Milestone; } }
        public int Priority { get { return 100; } }

        public VisualElement BuildInspector(ProjectBoardAsset board, BoardNodeModel node, IBoardCommandDispatcher dispatcher, Action repaint)
        {
            var milestone = node as MilestoneNodeModel;
            var root = new VisualElement();
            if (milestone == null)
            {
                return root;
            }

            BuiltInInspectorUtility.AddDelayedTextField(root, "Title", milestone.Title, value =>
            {
                MilestoneNodeModel updated = (MilestoneNodeModel)milestone.Clone();
                updated.Title = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });

            BuiltInInspectorUtility.AddDelayedTextField(root, "Summary", milestone.Summary, value =>
            {
                MilestoneNodeModel updated = (MilestoneNodeModel)milestone.Clone();
                updated.Summary = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            }, true);

            BuiltInInspectorUtility.AddDelayedTextField(root, "Target Date", milestone.TargetDateIso, value =>
            {
                MilestoneNodeModel updated = (MilestoneNodeModel)milestone.Clone();
                updated.TargetDateIso = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });

            Foldout detailsFoldout = BuiltInInspectorUtility.CreatePersistentFoldout(milestone.Id + ".Advanced", "Advanced", false);
            BuiltInInspectorUtility.AddTagsField(detailsFoldout, milestone, board, dispatcher, repaint);
            root.Add(detailsFoldout);
            return root;
        }
    }

    internal sealed class NoteNodeInspector : IProjectDesignerInspector
    {
        public string NodeTypeId { get { return BoardNodeTypeIds.Note; } }
        public int Priority { get { return 100; } }

        public VisualElement BuildInspector(ProjectBoardAsset board, BoardNodeModel node, IBoardCommandDispatcher dispatcher, Action repaint)
        {
            var note = node as NoteNodeModel;
            var root = new VisualElement();
            if (note == null)
            {
                return root;
            }

            BuiltInInspectorUtility.AddDelayedTextField(root, "Title", note.Title, value =>
            {
                NoteNodeModel updated = (NoteNodeModel)note.Clone();
                updated.Title = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });

            BuiltInInspectorUtility.AddDelayedTextField(root, "Body", note.Body, value =>
            {
                NoteNodeModel updated = (NoteNodeModel)note.Clone();
                updated.Body = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            }, true);

            Foldout advancedFoldout = BuiltInInspectorUtility.CreatePersistentFoldout(note.Id + ".Advanced", "Advanced", false);
            BuiltInInspectorUtility.AddDelayedTextField(advancedFoldout, "Accent Color", note.AccentHex, value =>
            {
                NoteNodeModel updated = (NoteNodeModel)note.Clone();
                updated.AccentHex = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });

            BuiltInInspectorUtility.AddTagsField(advancedFoldout, note, board, dispatcher, repaint);
            root.Add(advancedFoldout);
            return root;
        }
    }

    internal sealed class ReferenceNodeInspector : IProjectDesignerInspector
    {
        public string NodeTypeId { get { return BoardNodeTypeIds.Reference; } }
        public int Priority { get { return 100; } }

        public VisualElement BuildInspector(ProjectBoardAsset board, BoardNodeModel node, IBoardCommandDispatcher dispatcher, Action repaint)
        {
            var reference = node as ReferenceNodeModel;
            var root = new VisualElement();
            if (reference == null)
            {
                return root;
            }

            BuiltInInspectorUtility.AddDelayedTextField(root, "Title", reference.Title, value =>
            {
                ReferenceNodeModel updated = (ReferenceNodeModel)reference.Clone();
                updated.Title = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });

            BuiltInInspectorUtility.AddDelayedTextField(root, "Summary", reference.Summary, value =>
            {
                ReferenceNodeModel updated = (ReferenceNodeModel)reference.Clone();
                updated.Summary = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            }, true);

            var assetField = new ObjectField("Unity Asset");
            assetField.objectType = typeof(UnityEngine.Object);
            if (!string.IsNullOrEmpty(reference.AssetPath))
            {
                assetField.value = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(reference.AssetPath);
            }

            assetField.RegisterValueChangedCallback(evt =>
            {
                ReferenceNodeModel updated = (ReferenceNodeModel)reference.Clone();
                updated.AssetPath = evt.newValue == null ? string.Empty : AssetDatabase.GetAssetPath(evt.newValue);
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });
            root.Add(assetField);

            BuiltInInspectorUtility.AddDelayedTextField(root, "External URL", reference.ExternalUrl, value =>
            {
                ReferenceNodeModel updated = (ReferenceNodeModel)reference.Clone();
                updated.ExternalUrl = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });

            Foldout detailsFoldout = BuiltInInspectorUtility.CreatePersistentFoldout(reference.Id + ".ReferenceDetails", "Reference Details", false);
            BuiltInInspectorUtility.AddDelayedTextField(detailsFoldout, "Excerpt", reference.TextReference, value =>
            {
                ReferenceNodeModel updated = (ReferenceNodeModel)reference.Clone();
                updated.TextReference = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            }, true);

            BuiltInInspectorUtility.AddTagsField(detailsFoldout, reference, board, dispatcher, repaint);
            root.Add(detailsFoldout);
            return root;
        }
    }

    internal sealed class ClassNodeInspector : IProjectDesignerInspector
    {
        public string NodeTypeId { get { return BoardNodeTypeIds.Class; } }
        public int Priority { get { return 100; } }

        public VisualElement BuildInspector(ProjectBoardAsset board, BoardNodeModel node, IBoardCommandDispatcher dispatcher, Action repaint)
        {
            var classNode = node as ClassNodeModel;
            var root = new VisualElement();
            if (classNode == null)
            {
                return root;
            }

            BuiltInInspectorUtility.AddDelayedTextField(root, "Title", classNode.Title, value =>
            {
                ClassNodeModel updated = (ClassNodeModel)classNode.Clone();
                updated.Title = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });

            BuiltInInspectorUtility.AddDelayedTextField(root, "Namespace", classNode.NamespaceName, value =>
            {
                ClassNodeModel updated = (ClassNodeModel)classNode.Clone();
                updated.NamespaceName = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });

            BuiltInInspectorUtility.AddDelayedTextField(root, "Summary", classNode.Summary, value =>
            {
                ClassNodeModel updated = (ClassNodeModel)classNode.Clone();
                updated.Summary = value;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            }, true);

            Foldout fieldsFoldout = BuiltInInspectorUtility.CreatePersistentFoldout(classNode.Id + ".Fields", "Fields", false);
            BuiltInInspectorUtility.AddDelayedTextField(fieldsFoldout, "Field List", string.Join("\n", classNode.Fields.Select(item => item.Visibility + " " + item.Signature)), value =>
            {
                ClassNodeModel updated = (ClassNodeModel)classNode.Clone();
                updated.Fields.Clear();
                foreach (string line in value.Split('\n'))
                {
                    string trimmed = line.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        updated.Fields.Add(new BoardClassMemberData(trimmed, "private"));
                    }
                }

                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            }, true);
            root.Add(fieldsFoldout);

            Foldout methodsFoldout = BuiltInInspectorUtility.CreatePersistentFoldout(classNode.Id + ".Methods", "Methods", false);
            BuiltInInspectorUtility.AddDelayedTextField(methodsFoldout, "Method List", string.Join("\n", classNode.Methods.Select(item => item.Visibility + " " + item.Signature)), value =>
            {
                ClassNodeModel updated = (ClassNodeModel)classNode.Clone();
                updated.Methods.Clear();
                foreach (string line in value.Split('\n'))
                {
                    string trimmed = line.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        updated.Methods.Add(new BoardClassMemberData(trimmed, "public"));
                    }
                }

                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            }, true);
            root.Add(methodsFoldout);

            Foldout advancedFoldout = BuiltInInspectorUtility.CreatePersistentFoldout(classNode.Id + ".Advanced", "Advanced", false);
            BuiltInInspectorUtility.AddTagsField(advancedFoldout, classNode, board, dispatcher, repaint);
            root.Add(advancedFoldout);
            return root;
        }
    }
}
