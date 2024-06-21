using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectDesigner.Helpers
{
    /// <summary>
    /// Helper class to draw lines and textures on GUI.
    /// </summary>
    public static class GUIUtilities
    {
        private const float StartThreshold = 15f;
        private const int ActualPointsInBezier = 35;

        /// <summary>
        /// Draws a bezier curve from <paramref name="start"/> to <paramref name="end"/>.
        /// </summary>
        /// <param name="start">Start screenPosition of the curve.</param>
        /// <param name="end">End screenPosition of the curve.</param>
        /// <param name="color">Color of the line.</param>
        /// <param name="bezierOffset">An offset from start and end points.</param>
        public static void DrawBezierCurve(Vector2 start, Vector2 end, Color color, float bezierOffset = 100f)
        {
            Vector3[] lines = GetCurvedPoints(start, end, bezierOffset);
            DrawSolidLineArray(lines, color);
        }

        /// <summary>
        /// Draws a texture in a given <paramref name="screenPosition"/>.
        /// </summary>
        /// <param name="screenPosition">Screen position to draw.</param>
        /// <param name="color">Color of the texture</param>
        /// <param name="texture">Texture to draw</param>
        /// <param name="radius">Radius of the texture</param>
        public static void DrawTexture(Vector2 screenPosition, Color color, Texture2D texture, float radius)
        {
            Rect rect = new Rect(screenPosition.x - radius, screenPosition.y - radius, radius * 2, radius * 2);
            Color oldColor = GUI.color;

            GUI.color = color;
            GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            GUI.color = oldColor;
        }

        /// <summary>
        /// Returns a set of curved points from <paramref name="start"/> to <paramref name="end"/>.
        /// </summary>
        /// <param name="start">Start screenPosition of the curve.</param>
        /// <param name="end">End screenPosition of the curve.</param>
        /// <param name="bezierOffset">An offset from start and end points.</param>
        /// <returns></returns>
        public static Vector3[] GetCurvedPoints(Vector2 start, Vector2 end, float bezierOffset)
        {
            List<Vector3> points = new List<Vector3>();

            Vector2 bezierStart = new Vector2(start.x, start.y);
            Vector2 bezierEnd = new Vector2(end.x, end.y);

            Vector3 startTangent = bezierStart + Vector2.right * bezierOffset;
            Vector3 endTangent = bezierEnd - Vector2.right * bezierOffset;

            points.Add(start);
            points.AddRange(Handles.MakeBezierPoints(bezierStart, bezierEnd, startTangent, endTangent, ActualPointsInBezier));
            points.Add(end);
            return points.ToArray();
        }

        /// <summary>
        /// Returns the distance of a <paramref name="position"/> to a curve from <paramref name="start"/> to <paramref name="end"/>.
        /// </summary>
        /// <param name="position">Position to get distance to.</param>
        /// <param name="start">Start position of the curve.</param>
        /// <param name="end">End position of the curve.</param>
        /// <param name="bezierOffset">An offset from start and end points.</param>
        /// <returns></returns>
        public static float GetDistanceToCurve(Vector2 position, Vector2 start, Vector2 end, float bezierOffset)
        {
            GetTangentPoints(start, end, bezierOffset, out Vector3 tangentStart, out Vector3 tangentEnd);
            return HandleUtility.DistancePointBezier(position, start, end, tangentStart, tangentEnd);
        }

        private static void GetTangentPoints(Vector2 start, Vector2 end, float bezierOffset, out Vector3 tangentStart, out Vector3 tangentEnd)
        {
            Vector2 bezierStart = new Vector2(start.x + StartThreshold, start.y);
            Vector2 bezierEnd = new Vector2(end.x - StartThreshold, end.y);

            tangentStart = bezierStart + Vector2.right * bezierOffset;
            tangentEnd = bezierEnd - Vector2.right * bezierOffset;
        }

        public static void DrawTriangle(Vector2 position, float size, Color col, Vector2 direction, bool isSolid = true)
        {
            Vector3[] points = new Vector3[3];
            Color oldColor = Handles.color;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Calculate the positions of the triangle vertices
            Vector2 basePoint = position + direction.normalized * size / 2f;
            Vector2 leftPoint = position + (Vector2)(Quaternion.Euler(0, 0, angle + 30) * Vector2.up * size / 2f);
            Vector2 rightPoint = position + (Vector2)(Quaternion.Euler(0, 0, angle + 150) * Vector2.up * size / 2f);

            points[0] = basePoint;
            points[1] = leftPoint;
            points[2] = rightPoint;



            if (isSolid)
            {
                Handles.DrawAAConvexPolygon(points);
            }
            else
            {

                Handles.DrawDottedLine(points[0], points[1], size * .1f);
                Handles.DrawDottedLine(points[1], points[2], size * .1f);
                Handles.DrawDottedLine(points[2], points[0], size * .1f);
            }
            Handles.color = oldColor;
        }

        public static void DrawSolidLineArray(Vector3[] points, Color color, Texture2D texture = null, float thickness = 5f)
        {
            Color oldColor = Handles.color;
            Handles.color = color;
            Handles.DrawAAPolyLine(texture, thickness, points);
            Handles.color = oldColor;
        }

        public static void DrawDottedLine(Vector3 start, Vector3 end, Color color, float screenSpaceSize = .1f)
        {
            Color oldColor = Handles.color;
            Handles.color = color;
            Handles.DrawDottedLine(start, end, screenSpaceSize);
            Handles.color = oldColor;
        }


        public static void DrawLine(Vector3 start, Vector3 end, Color color, float screenSpaceSize = .1f)
        {
            Color oldColor = Handles.color;
            Handles.color = color;
#if UNITY_2020_2_OR_NEWER
            Handles.DrawLine(start, end, screenSpaceSize);
#else
            Handles.DrawLine(start, end);
#endif
            Handles.color = oldColor;
        }
    }
}
