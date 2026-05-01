namespace ProjectDesigner.V2.Data
{
    public sealed class BoardMilestoneHealthReport
    {
        public MilestoneNodeModel Milestone { get; set; }
        public BoardMilestoneHealthState State { get; set; }
        public float Completion { get; set; }
        public int LinkedTaskCount { get; set; }
        public int BlockedTaskCount { get; set; }
        public int OverdueTaskCount { get; set; }
        public int DueSoonTaskCount { get; set; }
    }
}
