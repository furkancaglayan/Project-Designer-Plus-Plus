namespace ProjectDesigner.V2.Editor
{
    internal static class ProjectDesignerPackageInfo
    {
        public const string PackageRoot = "Assets/Project Designer";
        public const string BaseStyleSheetPath = PackageRoot + "/Editor/Core/Styles/ProjectDesignerV2.uss";
        public const string DarkStyleSheetPath = PackageRoot + "/Editor/Core/Styles/ProjectDesignerV2.Dark.uss";
        public const string QuickStartPath = PackageRoot + "/Documentation/quick-start.md";
        public const string SettingsPath = "Project/Project Designer+";

        public static string GetInstalledVersion()
        {
            return "3.0.0";
        }
    }
}
