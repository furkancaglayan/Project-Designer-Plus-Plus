using System;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [Serializable]
    public sealed class MilestoneNodeModel : BoardNodeModel
    {
        [SerializeField]
        private string _summary;
        [SerializeField]
        private string _targetDateIso;

        public override string Category
        {
            get { return BoardNodeCategories.Planning; }
        }

        public string Summary
        {
            get { return _summary; }
            set { _summary = value ?? string.Empty; }
        }

        public string TargetDateIso
        {
            get { return _targetDateIso; }
            set { _targetDateIso = value ?? string.Empty; }
        }

        public MilestoneNodeModel()
            : base(BoardNodeTypeIds.Milestone, "Milestone", new Vector2(420f, 120f), new Vector2(320f, 200f))
        {
            _summary = "Capture the milestone goal and done definition.";
            _targetDateIso = string.Empty;
        }

        public override BoardNodeModel Clone()
        {
            var clone = new MilestoneNodeModel
            {
                Summary = Summary,
                TargetDateIso = TargetDateIso
            };
            CopyCommonTo(clone);
            return clone;
        }

        public override string GetSearchText()
        {
            return string.Join(" ", new[] { base.GetSearchText(), Summary, TargetDateIso });
        }
    }
}
