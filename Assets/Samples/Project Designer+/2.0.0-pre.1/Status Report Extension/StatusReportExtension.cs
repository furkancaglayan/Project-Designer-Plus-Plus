using System;
using System.Collections.Generic;
using ProjectDesigner.V2.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.SampleExtension
{
    public enum StatusReportHealth
    {
        Green,
        Yellow,
        Red
    }

    [Serializable]
    public sealed class StatusReportNodeModel : BoardNodeModel
    {
        [SerializeField]
        private string _updateSummary;
        [SerializeField]
        private string _risks;
        [SerializeField]
        private StatusReportHealth _health;

        public override string Category
        {
            get { return BoardNodeCategories.Planning; }
        }

        public string UpdateSummary
        {
            get { return _updateSummary; }
            set { _updateSummary = value ?? string.Empty; }
        }

        public string Risks
        {
            get { return _risks; }
            set { _risks = value ?? string.Empty; }
        }

        public StatusReportHealth Health
        {
            get { return _health; }
            set { _health = value; }
        }

        public StatusReportNodeModel()
            : base(BoardNodeTypeIds.StatusReport, "Status Report", new Vector2(180f, 180f), new Vector2(320f, 220f))
        {
            _updateSummary = "Weekly delivery summary.";
            _risks = "List blockers, unknowns, and asks.";
            _health = StatusReportHealth.Green;
        }

        public override BoardNodeModel Clone()
        {
            var clone = new StatusReportNodeModel
            {
                UpdateSummary = UpdateSummary,
                Risks = Risks,
                Health = Health
            };
            CopyCommonTo(clone);
            return clone;
        }

        public override string GetSearchText()
        {
            return string.Join(" ", new[] { base.GetSearchText(), UpdateSummary, Risks, Health.ToString() });
        }
    }

    internal sealed class StatusReportNodeDefinition : IProjectDesignerNodeDefinition
    {
        public string TypeId { get { return BoardNodeTypeIds.StatusReport; } }
        public string DisplayName { get { return "Status Report"; } }
        public string Description { get { return "Sample extension node for weekly delivery reporting."; } }
        public string Category { get { return BoardNodeCategories.Planning; } }
        public string AccentColor { get { return "#00BFA5"; } }
        public Vector2 DefaultSize { get { return new Vector2(320f, 220f); } }

        public BoardNodeModel CreateDefaultNode(Vector2 position)
        {
            var node = new StatusReportNodeModel();
            node.Position = position;
            return node;
        }

        public string GetPreview(BoardNodeModel node, BoardDocument document)
        {
            StatusReportNodeModel report = node as StatusReportNodeModel;
            return report == null ? string.Empty : report.Health + " | " + report.UpdateSummary;
        }
    }

    internal sealed class StatusReportInspector : IProjectDesignerInspector
    {
        public string NodeTypeId { get { return BoardNodeTypeIds.StatusReport; } }
        public int Priority { get { return 100; } }

        public VisualElement BuildInspector(ProjectBoardAsset board, BoardNodeModel node, IBoardCommandDispatcher dispatcher, Action repaint)
        {
            var report = node as StatusReportNodeModel;
            var root = new VisualElement();
            if (report == null)
            {
                return root;
            }

            var title = new TextField("Title");
            title.value = report.Title;
            title.isDelayed = true;
            title.RegisterValueChangedCallback(evt =>
            {
                StatusReportNodeModel updated = (StatusReportNodeModel)report.Clone();
                updated.Title = evt.newValue;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });
            root.Add(title);

            var health = new EnumField("Health", report.Health);
            health.RegisterValueChangedCallback(evt =>
            {
                StatusReportNodeModel updated = (StatusReportNodeModel)report.Clone();
                updated.Health = (StatusReportHealth)evt.newValue;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });
            root.Add(health);

            var summary = new TextField("Summary");
            summary.value = report.UpdateSummary;
            summary.isDelayed = true;
            summary.multiline = true;
            summary.RegisterValueChangedCallback(evt =>
            {
                StatusReportNodeModel updated = (StatusReportNodeModel)report.Clone();
                updated.UpdateSummary = evt.newValue;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });
            root.Add(summary);

            var risks = new TextField("Risks");
            risks.value = report.Risks;
            risks.isDelayed = true;
            risks.multiline = true;
            risks.RegisterValueChangedCallback(evt =>
            {
                StatusReportNodeModel updated = (StatusReportNodeModel)report.Clone();
                updated.Risks = evt.newValue;
                dispatcher.Execute(new UpdateNodeCommand(board, updated));
                repaint();
            });
            root.Add(risks);

            return root;
        }
    }

    internal sealed class StatusReportTextImporter : IProjectDesignerAssetImporter
    {
        public int Priority { get { return 75; } }

        public bool CanImport(UnityEngine.Object asset)
        {
            TextAsset textAsset = asset as TextAsset;
            return textAsset != null && textAsset.name.IndexOf("status", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public IEnumerable<BoardNodeModel> Import(UnityEngine.Object asset, Vector2 position)
        {
            TextAsset textAsset = asset as TextAsset;
            if (textAsset == null)
            {
                yield break;
            }

            var report = new StatusReportNodeModel
            {
                Title = textAsset.name,
                UpdateSummary = textAsset.text,
                Position = position
            };

            yield return report;
        }
    }

    [InitializeOnLoad]
    public static class StatusReportExtensionRegistration
    {
        static StatusReportExtensionRegistration()
        {
            Register();
        }

        public static void Register()
        {
            ProjectDesignerRegistry.RegisterNodeDefinition(new StatusReportNodeDefinition());
            ProjectDesignerRegistry.RegisterInspector(new StatusReportInspector());
            ProjectDesignerRegistry.RegisterAssetImporter(new StatusReportTextImporter());
        }
    }
}
