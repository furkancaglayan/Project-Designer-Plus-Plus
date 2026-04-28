using System;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.V2.Editor
{
    [FilePath("ProjectSettings/ProjectDesignerSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class ProjectDesignerSettings : ScriptableSingleton<ProjectDesignerSettings>
    {
        [SerializeField]
        private bool _showOnboardingOnStartup = true;

        [SerializeField]
        private bool _showOnboardingForPackageUpdates = true;

        [SerializeField]
        private bool _autoOpenWorkspaceAfterBoardCreation = true;

        [SerializeField]
        private string _defaultBoardFolder = "Assets/Project Designer";

        [SerializeField]
        private string _lastSeenOnboardingVersion = string.Empty;

        [SerializeField]
        private ProjectDesignerThemeMode _editorTheme = ProjectDesignerThemeConfig.DefaultTheme;

        public bool ShowOnboardingOnStartup
        {
            get { return _showOnboardingOnStartup; }
        }

        public bool ShowOnboardingForPackageUpdates
        {
            get { return _showOnboardingForPackageUpdates; }
        }

        public bool AutoOpenWorkspaceAfterBoardCreation
        {
            get { return _autoOpenWorkspaceAfterBoardCreation; }
        }

        public string DefaultBoardFolder
        {
            get { return NormalizeAssetFolder(_defaultBoardFolder); }
        }

        public string LastSeenOnboardingVersion
        {
            get { return _lastSeenOnboardingVersion ?? string.Empty; }
        }

        public ProjectDesignerThemeMode EditorTheme
        {
            get { return ProjectDesignerThemeConfig.ResolveTheme(_editorTheme); }
        }

        public bool ShouldShowOnboarding(string currentVersion)
        {
            if (!_showOnboardingOnStartup)
            {
                return false;
            }

            if (string.IsNullOrEmpty(_lastSeenOnboardingVersion))
            {
                return true;
            }

            if (string.Equals(_lastSeenOnboardingVersion, currentVersion, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return _showOnboardingForPackageUpdates;
        }

        public void MarkOnboardingSeen(string currentVersion)
        {
            _lastSeenOnboardingVersion = currentVersion ?? string.Empty;
            Save(true);
        }

        public void SetShowOnboardingOnStartup(bool value)
        {
            _showOnboardingOnStartup = value;
            Save(true);
        }

        public void SetEditorTheme(ProjectDesignerThemeMode value)
        {
            _editorTheme = ProjectDesignerThemeConfig.ResolveTheme(value);
            Save(true);
        }

        public void SaveSettings()
        {
            Save(true);
        }

        public static string NormalizeAssetFolder(string path)
        {
            string normalized = string.IsNullOrWhiteSpace(path) ? "Assets" : path.Trim().Replace("\\", "/");
            if (!normalized.StartsWith("Assets", StringComparison.Ordinal))
            {
                return "Assets";
            }

            while (normalized.Contains("//"))
            {
                normalized = normalized.Replace("//", "/");
            }

            normalized = normalized.TrimEnd('/');
            return normalized;
        }
    }
}
