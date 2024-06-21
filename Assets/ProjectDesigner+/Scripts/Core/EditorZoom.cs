using ProjectDesigner.Helpers;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// Used to start a zoom area in the editor. <br/>
    /// Reference: <a href="https://martinecker.com/martincodes/unity-editor-window-zooming/"/>
    /// </summary>
    public static class EditorZoom
    {
        private const float EditorWindowTabHeight = 21.0f;
        private static Matrix4x4 _prevGuiMatrix;

        /// <summary>
        /// Starts a zooming area starting with the given <paramref name="screenCoordsArea"/>.
        /// </summary>
        /// <param name="zoomScale"></param>
        /// <param name="screenCoordsArea"></param>
        /// <returns></returns>
        public static Rect Begin(float zoomScale, Rect screenCoordsArea)
        {
            GUI.EndGroup(); // End the card Unity begins automatically for an EditorWindow to clip out the window tab. This allows us to draw outside of the size of the EditorWindow.

            screenCoordsArea.y += EditorWindowTabHeight;

            screenCoordsArea.size *= zoomScale + 0.001f;

            Rect clippedArea = screenCoordsArea.ScaleSizeBy(1.0f / zoomScale, screenCoordsArea.TopLeft());

            GUI.BeginGroup(clippedArea);

            _prevGuiMatrix = GUI.matrix;
            Matrix4x4 translation = Matrix4x4.TRS(clippedArea.TopLeft(), Quaternion.identity, Vector3.one);
            Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));
            GUI.matrix = translation * scale * translation.inverse;

            return clippedArea;
        }


        /// <summary>
        /// Ends the zooming area.
        /// </summary>
        public static void End()
        {
            GUI.matrix = _prevGuiMatrix;
            GUI.EndGroup();
            GUI.BeginGroup(new Rect(0.0f, EditorWindowTabHeight, UnityEngine.Screen.width, UnityEngine.Screen.height));
        }
    }

}
