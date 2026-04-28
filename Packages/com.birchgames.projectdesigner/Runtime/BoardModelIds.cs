namespace ProjectDesigner.V2.Data
{
    public static class BoardNodeCategories
    {
        public const string All = "All";
        public const string Planning = "Planning";
        public const string Reference = "Reference";
        public const string TechnicalDesign = "Technical Design";
    }

    public static class BoardNodeTypeIds
    {
        public const string ProjectBrief = "project-designer.project-brief";
        public const string Task = "project-designer.task";
        public const string Milestone = "project-designer.milestone";
        public const string Note = "project-designer.note";
        public const string Reference = "project-designer.reference";
        public const string Class = "project-designer.class";
        public const string StatusReport = "project-designer.sample.status-report";
    }

    public static class BoardEdgeTypeIds
    {
        public const string Dependency = "project-designer.edge.dependency";
        public const string Milestone = "project-designer.edge.milestone";
        public const string Reference = "project-designer.edge.reference";
        public const string TechnicalRelation = "project-designer.edge.technical";
    }

    public static class BoardPresetIds
    {
        public const string Empty = "project-designer.preset.empty";
        public const string SoloIndie = "project-designer.preset.solo";
        public const string SmallTeam = "project-designer.preset.small-team";
        public const string TechnicalDesign = "project-designer.preset.technical";
        public const string PitchVision = "project-designer.preset.pitch-vision";
        public const string MilestoneRoadmap = "project-designer.preset.milestone-roadmap";
        public const string ResearchReference = "project-designer.preset.research-reference";
        public const string StakeholderReview = "project-designer.preset.stakeholder-review";
    }
}
