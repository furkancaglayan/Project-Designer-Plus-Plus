using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectDesigner.V2.Editor
{
    internal static class ProjectDesignerThemeConfig
    {
        private static readonly List<string> KnownStylePaths = new List<string>
        {
            ProjectDesignerPackageInfo.BaseStyleSheetPath,
            ProjectDesignerPackageInfo.DarkStyleSheetPath
        };

        public const ProjectDesignerThemeMode DefaultTheme = ProjectDesignerThemeMode.Light;
        public const bool SupportsDarkTheme = true;

        public static ProjectDesignerThemeMode ResolveTheme(ProjectDesignerThemeMode requestedTheme)
        {
            if (!SupportsDarkTheme && requestedTheme == ProjectDesignerThemeMode.Dark)
            {
                return DefaultTheme;
            }

            return requestedTheme;
        }

        public static void ApplyTheme(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            foreach (string stylePath in KnownStylePaths)
            {
                StyleSheet existingStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylePath);
                if (existingStyleSheet != null)
                {
                    root.styleSheets.Remove(existingStyleSheet);
                }
            }

            AddStyleSheet(root, ProjectDesignerPackageInfo.BaseStyleSheetPath);

            ProjectDesignerThemeMode theme = ResolveTheme(ProjectDesignerSettings.instance.EditorTheme);
            if (theme == ProjectDesignerThemeMode.Dark)
            {
                AddStyleSheet(root, ProjectDesignerPackageInfo.DarkStyleSheetPath);
            }
        }

        public static void RefreshOpenWindows()
        {
            ProjectDesignerV2Window[] plannerWindows = Resources.FindObjectsOfTypeAll<ProjectDesignerV2Window>();
            foreach (ProjectDesignerV2Window window in plannerWindows)
            {
                window.RefreshTheme();
            }

            ProjectDesignerOnboardingWindow[] onboardingWindows = Resources.FindObjectsOfTypeAll<ProjectDesignerOnboardingWindow>();
            foreach (ProjectDesignerOnboardingWindow window in onboardingWindows)
            {
                window.RefreshTheme();
            }

            ProjectDesignerBoardBrowserWindow[] browserWindows = Resources.FindObjectsOfTypeAll<ProjectDesignerBoardBrowserWindow>();
            foreach (ProjectDesignerBoardBrowserWindow window in browserWindows)
            {
                window.RefreshTheme();
            }
        }

        private static void AddStyleSheet(VisualElement root, string assetPath)
        {
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(assetPath);
            if (styleSheet != null)
            {
                root.styleSheets.Remove(styleSheet);
                root.styleSheets.Add(styleSheet);
            }
        }
    }
}
