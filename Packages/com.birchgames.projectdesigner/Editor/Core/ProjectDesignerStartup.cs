using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.V2.Editor
{
    [InitializeOnLoad]
    internal static class ProjectDesignerStartup
    {
        private const string StartupSessionKey = "ProjectDesigner.V2.OnboardingStartupChecked";

        static ProjectDesignerStartup()
        {
            EditorApplication.delayCall += ShowOnboardingIfNeeded;
        }

        private static void ShowOnboardingIfNeeded()
        {
            if (SessionState.GetBool(StartupSessionKey, false))
            {
                return;
            }

            if (Application.isBatchMode)
            {
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += ShowOnboardingIfNeeded;
                return;
            }

            SessionState.SetBool(StartupSessionKey, true);

            string version = ProjectDesignerPackageInfo.GetInstalledVersion();
            ProjectDesignerSettings settings = ProjectDesignerSettings.instance;
            if (!settings.ShouldShowOnboarding(version))
            {
                return;
            }

            settings.MarkOnboardingSeen(version);
            if (Resources.FindObjectsOfTypeAll<ProjectDesignerOnboardingWindow>().Length == 0)
            {
                ProjectDesignerOnboardingWindow.Open();
            }
        }
    }
}
