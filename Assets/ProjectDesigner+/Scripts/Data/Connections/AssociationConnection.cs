using ProjectDesigner.Core;
using ProjectDesigner.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Data.Connections
{
    /// <summary>
    /// Association connection class. Association connection is represented by a straight line with no other decorators.
    /// </summary>
    [Serializable, ConnectionBaseMetaData("Associate")]
    public class AssociationConnection : ClassConnection
    {
        //<inheritdoc>
        protected override void DrawConnection(IEditorContext context, Vector2 fromOutputScreenPos, Vector2 toInputScreenPos, Vector2 fromCenterScreenPos, Vector2 toCenterScreenPos, Color color)
        {
            Vector2 start = fromOutputScreenPos + (toInputScreenPos - fromOutputScreenPos).normalized * 10;
            Vector2 end = toInputScreenPos - (toInputScreenPos - fromOutputScreenPos).normalized * 15;
            GUIUtilities.DrawLine(start, end, color);
            Vector2 midPoint = (fromOutputScreenPos + toInputScreenPos) / 2;
            GUI.Label(new Rect(midPoint, new Vector2(80, 30)), "associates");
        }
    }
}
