using ProjectDesigner.Core;
using ProjectDesigner.Data;
using ProjectDesigner.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Editor
{
    /// <summary>
    /// Default implementation for <see cref="IDragAndDropHandler"/>.
    /// </summary>
    public class DragAndDropHandler : IDragAndDropHandler
    {
        private const int SelectionRectLabelBaseWidth = 250;
        private const int SelectionRectLabelBaseHeight = 20;
        private GUIStyle _label;
        private GUIStyle Label
        {
            get
            {
                if (_label == null)
                {
                    _label = new GUIStyle(EditorStyles.label);
                }

                return _label;
            }
        }
        public bool IsDragging {  get; private set; }
        public IDraggable[] DraggedObjects { get; private set; }
        public IDraggable DraggedObject { get; private set; }
        public Vector2 StartPosition;
        public Rect SelectionRect { get; private set; }
        public DragAndDropMode Mode { get; private set; }
        private object _data;
        private DragObjectsDelegate _onObjectsDragged;
        public void OnDragging(IEditorContext context, Event current)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy; // Show copy cursor

            if (Mode == DragAndDropMode.MultipleItems)
            {
                for (int i = 0; i < DraggedObjects.Length; i++)
                {
                    DraggedObjects[i].OnBeingDragged(context, current.delta);
                }
            }
            SelectionRect = new Rect(new Vector2(Mathf.Min(current.mousePosition.x, StartPosition.x), Mathf.Min(current.mousePosition.y, StartPosition.y)),
                            new Vector2(Mathf.Abs(current.mousePosition.x - StartPosition.x), Mathf.Abs(current.mousePosition.y - StartPosition.y)));
        }

        public void StartDragging(IEditorContext context, IEnumerable<IDraggable> items, Vector2 position)
        {
            DraggedObjects = items.ToArray();
            for (int i = 0; i < DraggedObjects.Length; i++)
            {
                DraggedObjects[i].OnStartDragging(context, position);
            }
            StartDraggingInternal(position, DragAndDropMode.MultipleItems);
        }


        public void StartDragging(IEditorContext context, IDraggable item, Vector2 position)
        {
            StartDragging(context, new List<IDraggable>() { item}, position);
        }
        public void StartDraggingEmpty(IEditorContext context, Vector2 position)
        {
            StartDraggingInternal(position, DragAndDropMode.Empty);
        }

        public void StartDraggingSelection(IEditorContext context, Vector2 position)
        {
            StartDraggingInternal(position, DragAndDropMode.Selection);
        }


        public void StartConnection(IEditorContext context, NodeBase nodeBase, Type type)
        {
            DraggedObject = nodeBase;
            _data = type;
            StartDraggingInternal(nodeBase.OutputPoint, DragAndDropMode.Connection);
        }

        private void StartDraggingInternal(Vector2 position, DragAndDropMode mode)
        {
            IsDragging = true;
            Mode = mode;
            StartPosition = position;
            SelectionRect = new Rect(StartPosition, Vector2.zero);
        }

        public void EndDragging(IEditorContext context, Event current, IDrawable drawable)
        {
            Vector2 delta = current == null ? Vector2.zero : current.mousePosition - StartPosition;

            if (Mode == DragAndDropMode.MultipleItems)
            {
                for (int i = 0; i < DraggedObjects.Length; i++)
                {
                    DraggedObjects[i].OnEndDragging(context, delta, drawable, Mode);
                }
            }
            else if (Mode == DragAndDropMode.Connection)
            {
                DraggedObject.OnEndDragging(context, delta, drawable, Mode, _data);
            }
            
            Reset();
        }

        public void OnGUI(IEditorContext context, Event current)
        {
            if (IsDragging)
            {
                if (Mode == DragAndDropMode.Selection)
                {
                    GUI.DrawTexture(SelectionRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0, 0.8f, 0.8f, 0.1f), 0f, 0f);
                    Color color = Handles.color;
                    Handles.color = Color.blue;
                    Handles.DrawLine(SelectionRect.position, SelectionRect.TopRight());
                    Handles.DrawLine(SelectionRect.position, SelectionRect.BottomLeft());
                    Handles.DrawLine(SelectionRect.BottomLeft(), SelectionRect.BottomRight());
                    Handles.DrawLine(SelectionRect.BottomRight(), SelectionRect.TopRight());
                    Handles.color = color;

                    Label.fontSize = (int)(12 / context.GetZoomFactor());
                    GUI.Label(new Rect(SelectionRect.x,
                                       SelectionRect.y - SelectionRectLabelBaseHeight / context.GetZoomFactor(),
                                       SelectionRect.width,
                                       SelectionRectLabelBaseHeight / context.GetZoomFactor()), SelectionRect.ToString(), _label);
                }
                else if (Mode == DragAndDropMode.Connection)
                {
                    Vector2 startPosition = context.GetScreenPosition(StartPosition);
                    Vector2 center = context.GetScreenPosition(DraggedObject.Rect.center);
                    ProjectDesigner.Helpers.GUIUtilities.DrawBezierCurve(startPosition, current.mousePosition, Color.white);
                }
            }
            else
            {
                HandleDraggedObjects();
            }
        }

        public void Reset()
        {
            DraggedObjects = null;
            StartPosition = Vector2.zero;
            IsDragging = false;
            _data = null;
        }

        private void HandleDraggedObjects()
        {
            Event currentEvent = Event.current;
            // Check if an object is being dragged onto the window
            if (currentEvent != null && DragAndDrop.objectReferences.Length > 0 && (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (currentEvent.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    _onObjectsDragged?.Invoke(DragAndDrop.objectReferences, currentEvent.mousePosition);
                }

                currentEvent.Use();
            }
        }


        public void SetDragObjectsDelegate(DragObjectsDelegate dragObjectsDelegate)
        {
            _onObjectsDragged = dragObjectsDelegate;
        }
    }
}
