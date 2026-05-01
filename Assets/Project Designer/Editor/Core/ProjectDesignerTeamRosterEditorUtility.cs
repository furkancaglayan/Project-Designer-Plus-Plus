using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProjectDesigner.V2.Data;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.V2.Editor
{
    internal static class ProjectDesignerTeamRosterEditorUtility
    {
        public static ProjectDesignerTeamRosterAsset CreateAndAssignDefaultRoster()
        {
            string targetFolder = ProjectDesignerBoardUtility.EnsureAssetFolderExists(ProjectDesignerSettings.instance.DefaultBoardFolder);
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(targetFolder + "/" + ProjectDesignerProductInfo.TeamRosterAssetName + ".asset");

            ProjectDesignerTeamRosterAsset roster = ScriptableObject.CreateInstance<ProjectDesignerTeamRosterAsset>();
            roster.name = Path.GetFileNameWithoutExtension(assetPath);

            foreach (string memberName in GetSuggestedMemberNames())
            {
                roster.Members.Add(new ProjectDesignerTeamMemberData
                {
                    DisplayName = memberName
                });
            }

            roster.EnsureDefaults();
            AssetDatabase.CreateAsset(roster, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ProjectDesignerSettings.instance.SetDefaultTeamRoster(roster);
            ProjectDesignerThemeConfig.RefreshOpenWindows();
            Selection.activeObject = roster;
            EditorGUIUtility.PingObject(roster);
            return roster;
        }

        public static void SelectRoster(ProjectDesignerTeamRosterAsset roster)
        {
            if (roster == null)
            {
                return;
            }

            Selection.activeObject = roster;
            EditorGUIUtility.PingObject(roster);
        }

        private static IEnumerable<string> GetSuggestedMemberNames()
        {
            List<string> memberNames = ProjectDesignerBoardCatalog.GetBoards()
                .SelectMany(board => board == null ? Enumerable.Empty<string>() : board.Document.TeamMembers)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList();

            if (memberNames.Count > 0)
            {
                return memberNames;
            }

            return new[] { "Producer", "Designer", "Programmer", "Artist" };
        }
    }
}
