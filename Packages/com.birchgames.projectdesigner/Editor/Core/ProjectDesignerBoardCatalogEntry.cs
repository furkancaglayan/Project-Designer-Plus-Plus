using ProjectDesigner.V2.Data;

namespace ProjectDesigner.V2.Editor
{
    internal sealed class ProjectDesignerBoardCatalogEntry
    {
        public ProjectBoardAsset Board { get; set; }
        public string Guid { get; set; }
        public string AssetPath { get; set; }
        public string BoardName { get; set; }
        public string Summary { get; set; }
        public string TeamSnapshot { get; set; }
        public int NodeCount { get; set; }
        public int EdgeCount { get; set; }
        public int InProgressCount { get; set; }
        public bool IsPinned { get; set; }
        public int RecentIndex { get; set; }

        public bool IsRecent
        {
            get { return RecentIndex >= 0; }
        }
    }
}
