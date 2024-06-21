using ProjectDesigner.Core;
using ProjectDesigner.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Data.Connections
{
    /// <summary>
    /// Curved relation connection class. Curved-relation connection is represented by a bezier curve with an arrow indicating the direction at the end.
    /// </summary>
    [Serializable, ConnectionBaseMetaData("Relation - Curved")]
    public class RelationCurvedConnection : RelationConnection
    {
        protected override void DrawConnection(IEditorContext context, Vector2 fromOutputScreenPos, Vector2 toInputScreenPos, Vector2 fromCenterScreenPos, Vector2 toCenterScreenPos, Color color)
        {
            Vector2 direction = (toInputScreenPos - fromOutputScreenPos).normalized;
            Vector2 start = fromOutputScreenPos + direction * 10;
            Vector2 end = toInputScreenPos - direction * 20;
            Vector3[] points = GUIUtilities.GetCurvedPoints(start, end, 100f);
            GUIUtilities.DrawSolidLineArray(points, color, thickness: 5f);
            GUIUtilities.DrawTriangle(end, 15f, color, direction);

            Vector2 midPoint = (fromOutputScreenPos + toInputScreenPos) / 2;
            GUI.Label(new Rect(midPoint, new Vector2(60, 18)), "relates");
        }


        public override bool Contains(Vector2 position)
        {
            if (From == null || To == null)
            {
                return false;
            }

            return GUIUtilities.GetDistanceToCurve(position, From.OutputPoint, To.InputPoint, 100f) < 10f;
        }
    }
}
