using ProjectDesigner.Core;
using ProjectDesigner.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectDesigner.Editor
{
    /// <summary>
    /// Used as a context manager for <see cref="ProjectDesignerWindow"/>.
    /// </summary>
    public class EditorContext : IEditorContext
    {
        /// <summary>
        /// Project object used in the context.
        /// </summary>
        public Project Project { get; private set; }
        private readonly ProjectDesignerWindow _window;
        //<inheritdoc>
        public IDragAndDropHandler DragAndDropHandler { get; private set; }

        private List<NodeBase> _nodes = new List<NodeBase>();
        public EditorContext(Project design, ProjectDesignerWindow window)
        {
            Project = design;
            _window = window;
            DragAndDropHandler = new DragAndDropHandler();
            DragAndDropHandler.SetDragObjectsDelegate(OnObjectsDragged);

            if (Project != null && Project.Drawables != null)
            {
                Initialize();
            }
        }

        //<inheritdoc>
        public List<T> GetDrawables<T>(Func<T, bool> func = null)
        {
            List<T> drawables = new List<T>();
            for (int i = 0; i < Project.Drawables.Count; i++)
            {
                IDrawable drawable = Project.Drawables[i];
                if (drawable is T t && (func == null || func(t)))
                {
                    drawables.Add(t);
                }
            }
            return drawables;
        }

        //<inheritdoc>
        public List<IDrawable> GetDrawables(Func<IDrawable, bool> func = null)
        {
            return GetDrawables<IDrawable>(func);
        }

        private void Initialize()
        {
            Debug.Assert(Project != null);
            _nodes = GetDrawables<NodeBase>();
        }


        /// <summary>
        /// <inheritdoc/><br></br>
        /// Calls <see cref="ProcessAction(EditorContextAction)"/>
        /// </summary>
        /// <param name="data"></param>
        public void ProcessAction(object data)
        {
            ProcessAction((EditorContextAction)data);
        }

        /// <summary>
        /// Processes the given <paramref name="editorContextAction"/>.<br></br>
        /// See: <seealso cref="ProcessAction(object)"/>
        /// </summary>
        /// <param name="editorContextAction"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ProcessAction(EditorContextAction editorContextAction)
        {
            if (editorContextAction == null)
            {
                throw new ArgumentNullException();
            }
            _window.ProcessAction(editorContextAction);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="drawable"></param>
        /// <param name="drawableCreationType"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddDrawable(IDrawable drawable, DrawableCreationType drawableCreationType)
        {
            if (drawable == null)
            {
                throw new ArgumentNullException("Drawable is null");
            }

            if (Project.AddDrawable(drawable))
            {
                if (drawable is NodeBase n)
                {
                    _nodes.Add(n);
                }
                drawable.OnAdded(this, drawableCreationType);
            }
            else
            {
                //RegisterError("Drawable limit is reached.", $"Maximum drawable limit is {Project.DrawableLimit}. This includes connections and nodes.");
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="drawable"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void DeleteDrawable(IDrawable drawable)
        {
            if (drawable == null)
            {
                throw new ArgumentNullException("Drawable is null");
            }

            Project.RemoveDrawable(drawable);
            if (drawable is NodeBase n)
            {
                _nodes.Remove(n);
            }
            drawable.OnRemoved(this);
        }

        /// <summary>
        /// Registers a log to the <see cref="Project"/> object. Registered log will be saved. <br></br>
        /// <seealso cref="EditorLogHistory"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="description"></param>
        public void RegisterLog(string text, string description = null)
        {
            Project.RegisterLog(text, description);
        }

        /// <summary>
        /// Registers a warning to the <see cref="Project"/> object. Registered warning will be saved. <br></br>
        /// <seealso cref="EditorLogHistory"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="description"></param>
        public void RegisterWarning(string text, string description = null)
        {
            Project.RegisterWarning(text, description);
        }

        /// <summary>
        /// Registers an error to the <see cref="Project"/> object. Registered error will be saved. <br></br>
        /// <seealso cref="EditorLogHistory"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="description"></param>
        public void RegisterError(string text, string description = null)
        {
            Project.RegisterError(text, description);
        }

        //<inheritdoc>
        IEnumerable<TemplateCollection.ConnectionTypeData> IEditorContext.GetAvailableConnectionTypes()
        {
            return TemplateCollection.GetDrawableConnectionTypes();
        }

        //<inheritdoc>
        public Vector2 GetScreenPosition(Vector2 original)
        {
            return _window.GetPerceivedScreenPosition(original);
        }

        //<inheritdoc>
        public Vector2 GetGUIPosition(Vector2 screenPosition)
        {
            return _window.GetGUIPosition(screenPosition);
        }

        //<inheritdoc>
        public float GetZoomFactor()
        {
            return _window.Zoom;
        }

        //<inheritdoc>
        public ReadOnlyCollection<TeamMember> GetTeamMembers()
        {
            return ProjectDesignerSettings.Instance.TeamMembers.AsReadOnly();
        }

        private void OnObjectsDragged(UnityEngine.Object[] objects, Vector2 screenPosition)
        {
            Vector2 position = screenPosition;
            foreach (var obj in objects)
            {
                string folderPath = AssetDatabase.GetAssetPath(obj); 
                if (folderPath != null && AssetDatabase.IsValidFolder(folderPath))
                {
                    OnFolderDragged(folderPath, screenPosition);
                }
                else if (TemplateCollection.GetNodeCreateDelegate(obj.GetType(), out TemplateCollection.CreateNodeFromAssetDelegate func))
                {
                    NodeBase nodeBase = func.Invoke(obj);
                    if (nodeBase != null)
                    {
                        nodeBase.SetPosition(screenPosition);
                        ProcessAction(new CreateNodeFromAssetAction(nodeBase, obj));
                        screenPosition += new Vector2(UnityEngine.Random.Range(-40, 40), UnityEngine.Random.Range(-40, 40));
                    }
                    else
                    {
                        RegisterLog("Could not create a new node.", $"{obj.name} ({obj.GetType()}) could not be used to create a node");
                    }
                }
            }
        }

        private void OnFolderDragged(string folderPath, Vector2 screenPosition)
        {
            string[] assetPaths = AssetDatabase.FindAssets("", new string[] { folderPath });
            List<Object> assets = new List<Object>();
            foreach (string guid in assetPaths)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            OnObjectsDragged(assets.ToArray(), screenPosition);
        }

        //<inheritdoc>
        public Vector2 CheckIfPositionInBounds(Rect position, out bool result)
        {
            return _window.CheckIfRectInBounds(position, out result);
        }

        //<inheritdoc>
        public ReadOnlyCollection<EditorLogHistory.Log> GetLogs()
        {
            return Project.GetLogs();
        }

        //<inheritdoc>
        public void Overview(IDrawable drawable)
        {
            _window.Overview(new List<IDrawable>() { drawable });
        }

        //<inheritdoc>
        public void ShowDropdownOutsideZoomArea(Func<GenericMenu> getGenericMenu)
        {
            _window.ShowDropdownOutsideZoomArea(getGenericMenu);
        }
    }
}
