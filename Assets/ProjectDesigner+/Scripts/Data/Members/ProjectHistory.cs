using ProjectDesigner.Core;
using ProjectDesigner.Helpers;
using System;
using System.Collections.ObjectModel;
using UnityEditor;

namespace ProjectDesigner.Data.Members
{
    /// <summary>
    /// <see cref="ProjectHistory"/> shows registered action logs in the <see cref="Project"/> object.
    /// </summary>
    public class ProjectHistory : MemberBase
    {
        private const int LogCountToShow = 3;

        public ProjectHistory(ProjectHistory other) : base()
        {
        }

        public ProjectHistory() : base()
        {
        }

        public override void OnAdded(NodeBase nodeBase)
        {
        }

        public override void Draw(IEditorContext context, NodeBase parent, float width)
        {
            ReadOnlyCollection<EditorLogHistory.Log> logs = context.GetLogs();
            CustomGUILayout.Label($"History", LabelStyle);

            for (int i = 0; i < Math.Min(LogCountToShow, logs.Count); i++)
            {
                EditorLogHistory.Log log = logs[logs.Count - 1 - i];
                CustomGUILayout.Label($"{log.Text}", LeftAlignedLabelStyle);
                CustomGUILayout.Label($"{log.Description}", EditorStyles.miniLabel);
            }
        }

        public override MemberBase Copy(NodeBase parent)
        {
            return new ProjectHistory(this);
        }
    }
}
