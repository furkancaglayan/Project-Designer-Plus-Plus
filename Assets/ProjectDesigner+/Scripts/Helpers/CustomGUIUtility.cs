
using UnityEngine;

namespace ProjectDesigner.Helpers
{
    /// <summary>
    /// GUI Utility class.
    /// </summary>
    public static class CustomGUIUtility
    {
        /// <summary>
        /// Text field and text are control id.
        /// </summary>
        public const string EditControlId = "TextField";

        /// <summary>
        /// Resets the keyboard focus.
        /// </summary>
        public static void ResetControl()
        {
            GUI.FocusControl(null);
        }

        /// <summary>
        /// Set the name of the next control.
        /// </summary>
        /// <param name="id"></param>
        public static void SetNextControl(string id)
        {
            GUI.SetNextControlName(id);
        }
        
        /// <summary>
        /// Is currently editing a text field or text area in project designer window?
        /// </summary>
        /// <returns></returns>
        public static bool IsEditingTextField()
        {
            return GUI.GetNameOfFocusedControl()?.StartsWith(EditControlId) == true;
        }

        /// <summary>
        /// Gets a unique id for the given <see cref="FocusType"/>. See: <see cref="GUIUtility.GetControlID(FocusType)"/>.
        /// </summary>
        /// <param name="focusType"></param>
        /// <returns></returns>
        public static int GetControlId(FocusType focusType)
        {
            return GUIUtility.GetControlID(focusType);
        }
    }
}
