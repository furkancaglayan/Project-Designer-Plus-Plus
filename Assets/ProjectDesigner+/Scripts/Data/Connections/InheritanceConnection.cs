using ProjectDesigner.Core;
using ProjectDesigner.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Data.Connections
{
    /// <summary>
    /// Inheritance connection class. Inheritance is represented by a straight line with an arrow indicating the direction at the end.
    /// </summary>
    [Serializable, ConnectionBaseMetaData("Inherit")]
    public class InheritanceConnection : ClassConnection
    {
        //<inheritdoc>
        protected override void DrawConnection(IEditorContext context, Vector2 fromOutputScreenPos, Vector2 toInputScreenPos, Vector2 fromCenterScreenPos, Vector2 toCenterScreenPos, Color color)
        {
            Vector2 start = fromOutputScreenPos + (toInputScreenPos - fromOutputScreenPos).normalized * 10;
            Vector2 end = toInputScreenPos - (toInputScreenPos - fromOutputScreenPos).normalized * 20;
            GUIUtilities.DrawLine(start, end, color);
            GUIUtilities.DrawTriangle(end, 15f, color, toInputScreenPos - fromOutputScreenPos, false);

            Vector2 midPoint = (fromOutputScreenPos + toInputScreenPos) / 2;
            GUI.Label(new Rect(midPoint, new Vector2(60, 18)), "inherits");
        }
    }
}
