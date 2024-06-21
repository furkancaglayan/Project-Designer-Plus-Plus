using ProjectDesigner.Core;
using ProjectDesigner.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Data.Connections
{
    /// <summary>
    /// Subtask connection class. Subtask is represented by a straight line.
    /// </summary>
    [Serializable, ConnectionBaseMetaData("Add Subtask")]
    public class SubTaskConnection : ConnectionBase
    {
        //<inheritdoc>
        protected override void DrawConnection(IEditorContext context, Vector2 startPoint, Vector2 endPoint, Vector2 center1, Vector2 center2, Color color)
        {
            Vector2 direction = (endPoint - startPoint).normalized * 10;
            Vector3[] points = new Vector3[]
            {
                startPoint + direction, endPoint - direction,
            };
            GUIUtilities.DrawSolidLineArray(points, color, thickness: 5f);

            Vector2 midPoint = (startPoint + endPoint) / 2;
            GUI.Label(new Rect(midPoint, new Vector2(80, 30)), "subtask");
        }
    }
}
