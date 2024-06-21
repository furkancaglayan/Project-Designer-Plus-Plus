using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Helpers
{
    /// <summary>
    /// A GUILayout class similar to <see cref="GUILayout"/> and <see cref="EditorGUILayout"/>. Most of the functions are shortened versions of the original ones from <see cref="GUILayout"/> and <see cref="EditorGUILayout"/>
    /// but it is still recommended to use the functions here because some of them implements spesific details that helps with some arrangements made in GUI. One example for this is <see cref="TextField(string, string, GUIStyle, float, float, int)"/> 
    /// which takes a string "id" parameter to keep track of focus control on editor GUI.
    /// </summary>
    public static class CustomGUILayout 
    {
        /// <summary>
        /// Shows a content. See also: <see cref="EditorGUILayout.LabelField(GUIContent, GUIStyle, GUILayoutOption[])"/>.
        /// </summary>
        /// <param name="label">Label to display.</param>
        /// <param name="style">GUIStyle used for the content. If left as null, <see cref="EditorStyles.label"/> will be used as default.</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="characterLimit">Character limit is used to cut text.</param>
        public static void Label(string label, GUIStyle style = null, float w = -1f, float h = -1f, int characterLimit = 0)
        {
            GUILayoutOption[] options = GetOptions(w, h);
            GUIContent labelContent = new GUIContent(label);
            if (characterLimit > 0 && label.Length > characterLimit)
            {
                labelContent.tooltip = label;
                labelContent.text = $"{label.Substring(0, characterLimit)}...";
            }

            EditorGUILayout.LabelField(labelContent, style ?? EditorStyles.label, options);
        }

        /// <summary>
        /// Shows a colored content. See also: <see cref="EditorGUILayout.LabelField(GUIContent, GUIStyle, GUILayoutOption[])"/>.
        /// </summary>
        /// <param name="label">Label to display.</param>
        /// <param name="color">Color to show the content in.</param>
        /// <param name="style">GUIStyle used for the content. If left as null, <see cref="EditorStyles.label"/> will be used as default.</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="characterLimit">Character limit is used to cut text.</param>
        public static void ColoredLabel(string label, Color color, GUIStyle style = null, float w = -1f, float h = -1f, int characterLimit = 0)
        {
            Color old = GUI.color;
            GUI.color = color;
            Label(label, style, w, h, characterLimit);
            GUI.color = old;
        }

        /// <summary>
        /// Works as the same as <see cref="EditorGUILayout.LabelField(GUIContent, GUIStyle, GUILayoutOption[])"/>.
        /// </summary>
        /// <param name="texture">Texture content</param>
        /// <param name="style">GUIStyle used for the content. If left as null, <see cref="EditorStyles.label"/> will be used as default.</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public static void Image(Texture2D texture, GUIStyle style = null, float w = -1f, float h = -1f)
        {
            GUILayoutOption[] options = GetOptions(w, h);
            EditorGUILayout.LabelField(new GUIContent(texture), style ?? EditorStyles.label, options);
        }

        /// <summary>
        /// Creates and shows a generic menu from given <paramref name="labels"/>, <paramref name="menuFunction"/> and <paramref name="addCondition"/>.
        /// </summary>
        /// <typeparam name="T">Type of the objects which will be passes as data to <paramref name="menuFunction"/></typeparam>
        /// <param name="labels">Labels as a list.</param>
        /// <param name="menuFunction">A consequence function that will run on click.</param>
        /// <param name="addCondition">A function to check if an option will be enabled or added on the menu.</param>
        /// <param name="showDisabledOptions">Will check if the failing items from <paramref name="addCondition"/> will be shown.</param>
        public static void CollectionGenericMenu<T>(IReadOnlyCollection<T> labels, Action<T> menuFunction, Func<T, bool> addCondition = null, bool showDisabledOptions = true)
        {
            GenericMenu menu = new GenericMenu();
            foreach (var label in labels)
            {
                if (addCondition == null || addCondition(label))
                {
                    menu.AddItem(new GUIContent(label.ToString()), false, (obj) => { menuFunction.Invoke(label); }, label);
                }
                else if (showDisabledOptions)
                {
                    menu.AddDisabledItem(new GUIContent(label.ToString()));
                }
            }

            menu.ShowAsContext();
        }

        /// <summary>
        /// Starts a horizontal layout group.<br></br>See: <see cref="GUILayout.BeginHorizontal(GUIContent, GUIStyle, GUILayoutOption[])"/>.
        /// </summary>
        /// <param name="style">GUIStyle to use</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public static void BeginHorizontal(GUIStyle style = null, float w = -1f, float h = -1f)
        {
            GUILayout.BeginHorizontal(style ?? GUIStyle.none, GetOptions(w, h));
        }

        /// <summary>
        /// Ends the horizontal layout group. <br></br>See: <see cref="GUILayout.EndHorizontal"/>
        /// </summary>
        public static void EndHorizontal()
        {
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Starts a vertival layout group.<br></br>See: <see cref="GUILayout.BeginVertical(GUIContent, GUIStyle, GUILayoutOption[])"/>.
        /// </summary>
        /// <param name="style">GUIStyle to use</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        public static void BeginVertical(GUIStyle style = null, float w = -1f, float h = -1f)
        {
            GUILayout.BeginVertical(style ?? GUIStyle.none, GetOptions(w, h));
        }

        /// <summary>
        /// Ends the vertical layout group. <br></br>See: <see cref="GUILayout.EndHorizontal"/>
        /// </summary>
        public static void EndVertical()
        {
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Make a content with a foldout arrow to the left of it. See: <see cref="EditorGUILayout.Foldout(bool, string, bool, GUIStyle)"/>
        /// </summary>
        /// <param name="foldout"></param>
        /// <param name="label"></param>
        /// <param name="style"></param>
        public static void Foldout(ref bool foldout, string label, GUIStyle style = null)
        {
            foldout = EditorGUILayout.Foldout(foldout, label, true, style ?? EditorStyles.foldout);
        }

        /// <summary>
        /// Begin a GUILayout block of GUI controls in a fixed screen area. See: <see cref="GUILayout.BeginArea(Rect, string, GUIStyle)"/>
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="style"></param>
        public static void BeginArea(Rect rect, GUIStyle style = null)
        {
            GUILayout.BeginArea(rect, style ?? GUIStyle.none);
        }

        /// <summary>
        /// Close a GUILayout block started with BeginArea. <see cref="GUILayout.EndArea"/>
        /// </summary>
        public static void EndArea()
        {
            GUILayout.EndArea();
        }

        /// <summary>
        /// Inserts a flexible space element. <see cref="GUILayout.FlexibleSpace"/>
        /// </summary>
        public static void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }

        /// <summary>
        /// Inserts a space in the current layout group. <see cref="GUILayout.Space(float)"/>
        /// </summary>
        /// <param name="space">Space in pixels</param>
        public static void Space(float space)
        {
            GUILayout.Space(space);
        }

        /// <summary>
        /// Makes a single press button. <see cref="GUILayout.Button(string, GUIStyle, GUILayoutOption[])"/>
        /// </summary>
        /// <param name="label">Label to show</param>
        /// <param name="style">GUIStyle to use</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <returns></returns>
        public static bool Button(string label, GUIStyle style = null, float w = -1f, float h = -1f)
        {
            return GUILayout.Button(label, style ?? GUI.skin.button, GetOptions(w, h));
        }
        /// <summary>
        /// Makes a single press button. <see cref="GUILayout.Button(GUIContent, GUIStyle, GUILayoutOption[])"/>
        /// </summary>
        /// <param name="content">GUIContent to show</param>
        /// <param name="style">GUIStyle to use</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <returns></returns>
        public static bool Button(GUIContent content, GUIStyle style = null, float w = -1f, float h = -1f)
        {
            return GUILayout.Button(content, style ?? GUI.skin.button, GetOptions(w, h));
        }

        /// <summary>
        /// Makes a single press dropdown button. See: <see cref="EditorGUILayout.DropdownButton(GUIContent, FocusType, GUIStyle, GUILayoutOption[])"/>
        /// </summary>
        /// <param name="content">Content to show</param>
        /// <param name="style">GUIStyle to use</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <returns></returns>
        public static bool DropdownButton(GUIContent content, GUIStyle style = null, float w = -1f, float h = -1f)
        {
            return EditorGUILayout.DropdownButton(content, FocusType.Keyboard, style ?? GUI.skin.button, GetOptions(w, h));
        }
        /// <summary>
        /// Makes a single press button. <see cref="GUILayout.Button(Texture2D, GUIStyle, GUILayoutOption[])"/>
        /// </summary>
        /// <param name="texture">Texture to show</param>
        /// <param name="style">GUIStyle to use</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <returns></returns>
        public static bool Button(Texture2D texture, GUIStyle style = null, float w = -1f, float h = -1f)
        {
            return GUILayout.Button(texture, style ?? GUI.skin.button, GetOptions(w, h));
        }

        /// <summary>
        /// Make an on/off toggle button. <see cref="GUILayout.Toggle(bool, string, GUILayoutOption[])"/>
        /// </summary>
        /// <param name="value">Is the button on or off.</param>
        /// <param name="label">Label to show</param>
        /// <param name="style"></param>
        /// <returns></returns>
        public static bool Toggle(bool value, string label = null, GUIStyle style = null)
        {
            return GUILayout.Toggle(value, label ?? string.Empty, style ?? EditorStyles.toggle);
        }

        /// <summary>
        /// Makes a texts area. It is important to enter a unique <paramref name="id"/>, otherwise some of the controls on the editor would break. See: <see cref="EditorGUILayout.TextArea(string, GUIStyle, GUILayoutOption[])"/>
        /// </summary>
        /// <param name="text">The text to edit</param>
        /// <param name="id">Id is used to keep track which text area or text field has the focus.</param>
        /// <param name="style">GUIStyle to use</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="characterLimit">Character limit is used to cut text if lenght of the a<paramref name="text"/> is more than it.</param>
        /// <returns></returns>
        public static string TextArea(string text, string id, GUIStyle style = null, float w = -1f, float h = -1f, int characterLimit = 0)
        {
            CustomGUIUtility.SetNextControl($"{CustomGUIUtility.EditControlId}_{id}");
            string newText = EditorGUILayout.TextArea(text, style ?? EditorStyles.textField, GetOptions(w, h));

            if (newText != null && characterLimit > 0 && newText.Length > characterLimit)
            {
                newText = text.Substring(0, characterLimit);
            }

            return newText;
        }

        /// <summary>
        /// Makes a texts field. It is important to enter a unique <paramref name="id"/>, otherwise some of the controls on the editor would break. See: <see cref="EditorGUILayout.TextField(string, GUIStyle, GUILayoutOption[])"/>
        /// </summary>
        /// <param name="text">The text to edit</param>
        /// <param name="id">Id is used to keep track which text area or text field has the focus.</param>
        /// <param name="style">GUIStyle to use</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="characterLimit">Character limit is used to cut text if lenght of the a<paramref name="text"/> is more than it.</param>
        /// <returns></returns>
        public static string TextField(string text, string id, GUIStyle style = null, float w = -1f, float h = -1f, int characterLimit = 0)
        {
            CustomGUIUtility.SetNextControl($"{CustomGUIUtility.EditControlId}_{id}");
            string newText = EditorGUILayout.TextField(text, style ?? EditorStyles.textField, GetOptions(w, h));

            if (newText != null && characterLimit > 0 && newText.Length > characterLimit)
            {
                newText = text.Substring(0, characterLimit);
            }

            return newText;
        }

        /// <summary>
        /// Makes a texts field. It is important to enter a unique <paramref name="id"/>, otherwise some of the controls on the editor would break. See: <see cref="EditorGUILayout.TextField(GUIContent, string, GUIStyle, GUILayoutOption[])(string, GUIStyle, GUILayoutOption[])"/>
        /// </summary>
        /// <param name="label">The content to display</param>
        /// <param name="id">Id is used to keep track which text area or text field has the focus.</param>
        /// <param name="text">The text to edit</param>
        /// <param name="style">GUIStyle to use</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="characterLimit">Character limit is used to cut text if lenght of the a<paramref name="text"/> is more than it.</param>
        /// <returns></returns>
        public static string TextField(string label, string id, string text, GUIStyle style = null, float w = -1f, float h = -1f, int characterLimit = 0)
        {
            CustomGUIUtility.SetNextControl($"{CustomGUIUtility.EditControlId}_{id}");
            string newText = EditorGUILayout.TextField(label, text, style ?? EditorStyles.textField, GetOptions(w, h));

            if (newText != null && characterLimit > 0 && newText.Length > characterLimit)
            {
                newText = text.Substring(0, characterLimit);
            }

            return newText;
        }

        /// <summary>
        /// Creates and returns a disposable <see cref="GUILayout.ScrollViewScope"/> object to handle scrollable areas. See <see cref="GUILayout.BeginScrollView(Vector2, GUIStyle, GUILayoutOption[])"/>
        /// </summary>
        /// <param name="scroll">The position to use display.</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <returns>Disposable <see cref="GUILayout.ScrollViewScope"/> object</returns>
        public static GUILayout.ScrollViewScope CreateScrollView(Vector2 scroll, float w = -1f, float h = -1f)
        {
            return new GUILayout.ScrollViewScope(scroll, GetOptions(w, h));
        }

        /// <summary>
        /// Makes a slider that the user can drag between <paramref name="min"/> and <paramref name="max"/>.
        /// </summary>
        /// <param name="value">Value to edit</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <returns></returns>
        public static float Slider(float value, float min, float max, float w = -1f, float h = -1f)
        {
            return EditorGUILayout.Slider(value, min, max, GetOptions(w, h));
        }

        /// <summary>
        /// Makes a field that receive the objects of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="obj">The object the field shows.</param>
        /// <param name="label">Label to display.</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <returns></returns>
        public static T ObjectField<T>(T obj, string label = null, float w = -1f, float h = -1f) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(label))
            {
                return EditorGUILayout.ObjectField(obj, typeof(T), false, GetOptions(w, h)) as T;
            }
            else
            {
                return EditorGUILayout.ObjectField(label, obj, typeof(T), false, GetOptions(w, h)) as T;
            }
        }

        /// <summary>
        /// Makes a field for selecting color.
        /// </summary>
        /// <param name="color">THe currently selected color.</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <returns>The color selected by the user.</returns>
        public static Color ColorField(Color color, float w = -1f, float h = -1f)
        {
            return EditorGUILayout.ColorField(color, GetOptions(w, h));
        }

        /// <summary>
        /// Makes an enum popup selection field.
        /// </summary>
        /// <typeparam name="T">Type of the enum to show</typeparam>
        /// <param name="t">Currently selected enum option</param>
        /// <param name="label">Label to show</param>
        /// <param name="style">GUIStyle to use</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <returns>The selected enum option.</returns>
        public static T EnumPopup<T>(T t, string label = null, GUIStyle style = null, float w = -1f, float h = -1f) where T : Enum
        {
            if (string.IsNullOrEmpty(label))
            {
                return (T)EditorGUILayout.EnumPopup(t, style ?? EditorStyles.popup, GetOptions(w, h));
            }
            else
            {
                return (T)EditorGUILayout.EnumPopup(label, t, style ?? EditorStyles.popup, GetOptions(w, h));
            }
        }

        /// <summary>
        /// Show a text field or a content field depending on <paramref name="editCondition"/>. See: <see cref="TextField(string, string, GUIStyle, float, float, int)"/> and <see cref="Label(string, GUIStyle, float, float)"/>
        /// </summary>
        /// <param name="text">Text to edit</param>
        /// <param name="label">Label to show</param>
        /// <param name="style">GUIStyle to use</param>
        /// <param name="editCondition">The condition that controls the editability of the text.</param>
        /// <param name="id">Id is used to keep track which text area or text field has the focus.</param>
        /// <param name="w">Width</param>
        /// <param name="h">Height</param>
        /// <param name="characterLimit">Character limit is used to cut text if lenght of the a<paramref name="text"/> is more than it.</param>
        public static void EditableText(ref string text, string label, GUIStyle style, bool editCondition, string id, float w = -1f, float h = -1f, int characterLimit = -1)
        {
            if (editCondition)
            {
                text = TextField(text, id, style, w, h, characterLimit: characterLimit);
            }
            else
            {
                Label(label, style, w, h);
            }
        }

        /// <summary>
        /// Shows a help box with the given <paramref name="message"/>. See: <see cref="EditorGUILayout.HelpBox(string, MessageType)"/>
        /// </summary>
        /// <param name="message">Message to show</param>
        /// <param name="messageType">The type of message.</param>
        public static void HelpBox(string message, MessageType messageType)
        {
            EditorGUILayout.HelpBox(message, messageType);
        }

        private static GUILayoutOption[] GetOptions(float w, float h)
        {
            List<GUILayoutOption> options = new List<GUILayoutOption>();
            if (w > 0)
            {
                options.Add(GUILayout.Width(w));
                options.Add(GUILayout.MaxWidth(w));
            }

            if (h > 0)
            {
                options.Add(GUILayout.Height(h));
                options.Add(GUILayout.MaxHeight(h));
            }

            return options.ToArray();
        }

    }
}
