using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProjectDesigner.Helpers;
using System.Linq;
using System.IO;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// A helper class used to get specific <see cref="GUIStyle"/> and <see cref="Texture2D"/> objects. If you want to extend this tool you can check this class out
    /// but to be honest i'd advise using your own functions.
    /// </summary>
    [Serializable]
    public class GUIStyleCollection
    {
        /// <summary>
        /// <see cref="GUIStyle"/> categories defined in <see cref="GUIStyleCollection"/>.
        /// </summary>
        public enum StyleCategory
        {
            Any,
            Label,
            LabelRichText,
            LabelLeftAligned,
            HeaderLeftAligned,
            Foldout,
            Image,
            Header,
            NodeOn,
            NodeOff
        }

        /// <summary>
        /// Default <see cref="GUIStyle"/> objects. Keys are defines from <seealso cref="StyleCategory"/> string values.
        /// </summary>
        [SerializeField, HideInInspector]
        internal SerializableDictionary<string, GUIStyle> DefaultGUIStyles = new SerializableDictionary<string, GUIStyle>();
        [SerializeField, HideInInspector]
        private SerializableDictionary<string, Texture2D> _textures;

        internal static GUIStyleCollection GetDefaultStyles()
        {
            GUIStyleCollection collection = new GUIStyleCollection();
            collection.DefaultGUIStyles.Add(GetNodeStyleOnVariation());
            collection.DefaultGUIStyles.Add(GetNodeStyleOffVariation());
            collection.DefaultGUIStyles.Add(GetDefaultLabelStyle());
            collection.DefaultGUIStyles.Add(GetDefaultLabelWithRichTextStyle());
            collection.DefaultGUIStyles.Add(GetDefaultLabelLeftAlignedStyle());
            collection.DefaultGUIStyles.Add(GetDefaultFoldoutStyle());
            collection.DefaultGUIStyles.Add(GetDefaultImageStyle());
            collection.DefaultGUIStyles.Add(GetDefaultHeaderStyle());
            collection.DefaultGUIStyles.Add(GetDefaultLeftAlignedHeaderStyle());
            return collection;
        }

        private static (string, GUIStyle) GetDefaultLabelStyle()
        {
            GUIStyle label = new GUIStyle();
            label.name = StyleCategory.Label.ToString();
            label.font = GetDefaultFont();
            label.wordWrap = true;
            label.richText = false;
            label.normal.textColor = Color.white;
            label.active.textColor = Color.white;
            label.fontSize = 16;
            label.stretchHeight = true;
            label.stretchWidth = true;
            label.alignment = TextAnchor.MiddleCenter;
            return (label.name, label);
        }

        private static (string, GUIStyle) GetDefaultLabelWithRichTextStyle()
        {
            GUIStyle label = new GUIStyle();
            label.name = StyleCategory.LabelRichText.ToString();
            label.font = GetDefaultFont();
            label.wordWrap = true;
            label.richText = true;
            label.normal.textColor = Color.white;
            label.active.textColor = Color.white;
            label.fontSize = 16;
            label.stretchHeight = true;
            label.stretchWidth = true;
            label.alignment = TextAnchor.MiddleCenter;
            return (label.name, label);
        }

        private static (string, GUIStyle) GetDefaultLabelLeftAlignedStyle()
        {
            GUIStyle label = new GUIStyle();
            label.name = StyleCategory.LabelLeftAligned.ToString();
            label.font = GetDefaultFont();
            label.wordWrap = false;
            label.richText = false;
            label.normal.textColor = Color.white;
            label.active.textColor = Color.white;
            label.fontSize = 16;
            label.alignment = TextAnchor.MiddleLeft;
            return (label.name, label);
        }

        private static (string, GUIStyle) GetDefaultFoldoutStyle()
        {
            GUIStyle label = new GUIStyle(EditorStyles.foldout);
            label.name = StyleCategory.Foldout.ToString();
            label.font = GetDefaultFont();
            label.wordWrap = false;
            label.normal.textColor = Color.white;
            label.active.textColor = Color.white;
            label.fontSize = 16;
            label.alignment = TextAnchor.MiddleLeft;
            return (label.name, label);
        }

        private static (string, GUIStyle) GetDefaultImageStyle()
        {
            GUIStyle label = new GUIStyle();
            label.name = StyleCategory.Image.ToString();
            label.font = GetDefaultFont();
            label.wordWrap = false;
            label.normal.textColor = Color.white;
            label.active.textColor = Color.white;
            label.fontSize = 16;
            label.alignment = TextAnchor.MiddleCenter;
            label.imagePosition = ImagePosition.ImageAbove;

            return (label.name, label);
        }

        private static (string, GUIStyle) GetDefaultHeaderStyle()
        {
            GUIStyle header = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            header.name = StyleCategory.Header.ToString();
            header.font = GetDefaultFont();
            header.wordWrap = true;
            header.normal.textColor = Color.white;
            header.active.textColor = Color.white;
            header.alignment = TextAnchor.MiddleCenter;
            header.fontStyle = FontStyle.Italic;
            header.fontSize = 24;
            return (header.name, header);
        }
        private static (string, GUIStyle) GetDefaultLeftAlignedHeaderStyle()
        {
            GUIStyle header = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            header.name = StyleCategory.HeaderLeftAligned.ToString();
            header.font = GetDefaultFont();
            header.wordWrap = true;
            header.normal.textColor = Color.white;
            header.active.textColor = Color.white;
            header.alignment = TextAnchor.MiddleLeft;
            header.fontStyle = FontStyle.Italic;
            header.fontSize = 24;
            return (header.name, header);
        }
        private static (string, GUIStyle) GetNodeStyleOnVariation()
        {
            GUIStyle styleOn = new GUIStyle();
            styleOn.name = StyleCategory.NodeOn.ToString();
            styleOn.font = ProjectDesignerSettings.Instance.CustomFont;

            styleOn.padding.bottom = 15;
            styleOn.padding.left = 20;
            styleOn.padding.right = 20;
            styleOn.padding.top = 15;

            styleOn.stretchWidth = true;

            styleOn.fontSize = 21;
            styleOn.alignment = TextAnchor.MiddleCenter;

            styleOn.contentOffset = new Vector2(0, -14);

            return (styleOn.name, styleOn);
        }

        private static (string, GUIStyle) GetNodeStyleOffVariation()
        {
            GUIStyle styleOff = new GUIStyle(GetNodeStyleOnVariation().Item2);
            styleOff.name = StyleCategory.NodeOff.ToString();
            return (styleOff.name, styleOff);
        }

        private static Font GetDefaultFont()
        {
            Font customFont = ProjectDesignerSettings.Instance.CustomFont;
            if (customFont != null)
            {
                return customFont;
            }

            Font defaultFont = ProjectDesignerSettings.Instance.DefaultFont;
            if (defaultFont != null)
            {
                return defaultFont;
            }

            string[] osFonts = Font.GetOSInstalledFontNames();
            string[] fontNames = new string[] { "Verdana", "Consolas"};
            foreach (var name in fontNames)
            {
                if (osFonts.Contains(name))
                {
                    return Font.CreateDynamicFontFromOSFont(name, 18);
                }
            }

            return Font.CreateDynamicFontFromOSFont("Arial", 18);
        }

        /// <summary>
        /// Loads all assets of type <typeparamref name="T"/> in given <paramref name="path"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns>A serializable dictionary with asset names as keys and assets as values.</returns>
        public static SerializableDictionary<string, T> LoadAssetsFromResources<T>(string path) where T: UnityEngine.Object
        {
            UnityEngine.Object[] assets = Resources.LoadAll(path, typeof(T));
            SerializableDictionary<string, T> dictionary = new SerializableDictionary<string, T>();
            foreach (var asset in assets)
            {
                dictionary.Add(asset.name, (T)asset);
            }

            return dictionary;
        }

        /// <summary>
        /// Loads a spesific asset from a path in a Resources folder. Does not need the extension.  Example: <br/>
        /// <code>
        /// LoadAssetFromResources("Fonts", "Raleway-SemiBold")
        /// </code>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T LoadAssetFromResources<T>(string path, string name) where T : UnityEngine.Object
        {
            UnityEngine.Object asset = Resources.Load<T>(Path.Combine(path, name));
            return (T)asset;
        }

        /// <summary>
        /// Gets the texture with the given name. The texture is first searched within <see cref="ProjectDesignerSettings.CustomTextures"/> and then 
        /// <see cref="ProjectDesignerSettings.DefaultTextures"/>. That way default textures used in the tool such as node textures can be overridden.
        /// As a fallback, this function calls <see cref="EditorGUIUtility.IconContent(string)"/>, so it's possible to get built-in textures.<br/>
        /// <code>
        /// GUIStyleCollection.GetTexture("UnityLogoLarge")
        /// </code>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Texture2D GetTexture(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            ProjectDesignerSettings settings = ProjectDesignerSettings.Instance;

            if (settings.CustomTextures != null && settings.CustomTextures.TryGetValue(name, out Texture2D texture))
            {
                return texture;
            }

            if (settings.DefaultTextures != null && settings.DefaultTextures.TryGetValue(name, out texture))
            {
                return texture;
            }

            return EditorGUIUtility.IconContent(name).image as Texture2D;
        }

        /// <summary>
        /// Returns the default <see cref="GUIStyle"/> object defined in <see cref="DefaultGUIStyles"/> for given category.<br></br>
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static GUIStyle GetDefaultStyleForCategory(StyleCategory category)
        {
            GUIStyleCollection collection = ProjectDesignerSettings.Instance.Styles;

            if (collection == null)
            {
                ProjectDesignerSettings.Instance.Styles = GetDefaultStyles();
            }

            if (collection.DefaultGUIStyles != null && collection.DefaultGUIStyles.Count > 0 && collection.DefaultGUIStyles.TryGetValue(category.ToString(), out GUIStyle style))
            {
                return style;
            }

            Debug.Assert(false);
            return null;
        }
    }
}
