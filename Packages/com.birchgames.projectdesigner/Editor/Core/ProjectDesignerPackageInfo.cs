using UnityPackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace ProjectDesigner.V2.Editor
{
    internal static class ProjectDesignerPackageInfo
    {
        public const string PackageRoot = "Packages/com.birchgames.projectdesigner";
        public const string BaseStyleSheetPath = PackageRoot + "/Editor/Core/Styles/ProjectDesignerV2.uss";
        public const string DarkStyleSheetPath = PackageRoot + "/Editor/Core/Styles/ProjectDesignerV2.Dark.uss";
        public const string QuickStartPath = PackageRoot + "/Documentation~/quick-start.md";
        public const string SettingsPath = "Project/Project Designer+";

        public static string GetInstalledVersion()
        {
            UnityPackageInfo packageInfo = UnityPackageInfo.FindForAssetPath(PackageRoot);
            return packageInfo != null && !string.IsNullOrEmpty(packageInfo.version)
                ? packageInfo.version
                : "2.0.0-pre.1";
        }
    }
}
