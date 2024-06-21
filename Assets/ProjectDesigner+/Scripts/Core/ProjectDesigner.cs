using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// A class that holds some data about the tool.
    /// </summary>
    public static class ProjectDesigner
    {
        public static string ToolTitle => "ProjectDesigner+";
        public static string SettingsAssetFolder => $"Assets/{ToolTitle}/Editor/Settings/";
        public static string DataAssetFolder => $"Assets/{ToolTitle}/Editor/Data/";
        public static string ExampleAssetFolder => $"Assets/{ToolTitle}/Editor/Examples/";
        public static string PreferencesTitle => ToolTitle;
        public static string PreferencesPath => "Preferences/" + PreferencesTitle;
        public static string FeedbackMail = "birch_games@hotmail.com";
    }
}
