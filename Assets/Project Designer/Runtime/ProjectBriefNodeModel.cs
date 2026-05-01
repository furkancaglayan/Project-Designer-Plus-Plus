using System;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [Serializable]
    public sealed class ProjectBriefNodeModel : BoardNodeModel
    {
        [SerializeField]
        private string _overview;
        [SerializeField]
        private string _teamSnapshot;
        [SerializeField]
        private string _projectKnowledge;

        public override string Category
        {
            get { return BoardNodeCategories.Planning; }
        }

        public string Overview
        {
            get { return _overview; }
            set { _overview = value ?? string.Empty; }
        }

        public string TeamSnapshot
        {
            get { return _teamSnapshot; }
            set { _teamSnapshot = value ?? string.Empty; }
        }

        public string ProjectKnowledge
        {
            get { return _projectKnowledge; }
            set { _projectKnowledge = value ?? string.Empty; }
        }

        public ProjectBriefNodeModel()
            : base(BoardNodeTypeIds.ProjectBrief, ProjectDesignerProductInfo.ProjectBriefTitle, new Vector2(120f, 100f), new Vector2(360f, 280f))
        {
            _overview = "Use this card for the pitch, player promise, and current planning focus.";
            _teamSnapshot = "Capture the team roles and ownership here.";
            _projectKnowledge = "Track constraints, shared vocabulary, and important references.";
            IsPinned = true;
        }

        public override BoardNodeModel Clone()
        {
            var clone = new ProjectBriefNodeModel
            {
                Overview = Overview,
                TeamSnapshot = TeamSnapshot,
                ProjectKnowledge = ProjectKnowledge
            };
            CopyCommonTo(clone);
            return clone;
        }

        public override string GetSearchText()
        {
            return string.Join(" ", new[]
            {
                base.GetSearchText(),
                Overview,
                TeamSnapshot,
                ProjectKnowledge
            });
        }
    }
}
