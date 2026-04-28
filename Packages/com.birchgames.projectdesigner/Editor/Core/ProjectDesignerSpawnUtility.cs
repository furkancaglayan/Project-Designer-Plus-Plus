using ProjectDesigner.V2.Data;
using UnityEngine;

namespace ProjectDesigner.V2.Editor
{
    internal static class ProjectDesignerSpawnUtility
    {
        private static readonly Vector2 SelectedOffset = new Vector2(46f, 38f);
        private static readonly Vector2 CascadeStep = new Vector2(34f, 28f);
        private const int CascadeLength = 6;

        public static Vector2 GetCreatePosition(Vector2 viewportCenter, BoardNodeModel selectedNode, int createIndex)
        {
            if (selectedNode != null)
            {
                return selectedNode.Position + SelectedOffset;
            }

            int stepIndex = Mathf.Max(0, createIndex) % CascadeLength;
            return viewportCenter + CascadeStep * stepIndex;
        }
    }
}
