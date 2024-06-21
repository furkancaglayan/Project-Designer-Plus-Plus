using ProjectDesigner.Core;
using ProjectDesigner.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// A <see cref="Project"/> object is a scriptable object where all NodeBases and IDrawables are saved.
    /// </summary>
    [Serializable, CreateAssetMenu(fileName = "Project", menuName = "Project Designer+/New Project")]
    public class Project : ScriptableObject, ISerializationCallbackReceiver
    {
        private const string AssetName = "Project";
        /// <summary>
        /// Maximum drawable limit a project designer window can have.
        /// </summary>
        public const int DrawableLimit = 120;
        [SerializeReference]
        private List<ISerialized> SerializedObjects = new List<ISerialized>();
        private List<IDrawable> _drawables = new List<IDrawable>();
        /// <summary>
        /// All drawable objects currently saved to the project.
        /// </summary>
        public ReadOnlyCollection<IDrawable> Drawables;

        [SerializeField]
        private EditorLogHistory _logHistory = new EditorLogHistory();

        private void OnEnable()
        {
            Drawables = new ReadOnlyCollection<IDrawable>(_drawables);
        }

        /// <summary>
        /// Sets the asset dirty.
        /// </summary>
        public void Save()
        {
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Creates and returns a new <see cref="Project"/> object with the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Project NewDesign(string name)
        {
            Project project = AssetHelpers.CreateAsset<Project>(Core.ProjectDesigner.DataAssetFolder, AssetName);
#if UNITY_2021_2_OR_NEWER
            EditorGUIUtility.SetIconForObject(project, GUIStyleCollection.GetTexture("project_designer_icon"));
#endif
            return project;
        }

        /// <summary>
        /// Adds a new <see cref="IDrawable"/> to the project.
        /// </summary>
        /// <param name="drawable"></param>
        /// <returns></returns>
        public bool AddDrawable(IDrawable drawable)
        {
            if (_drawables.Count < DrawableLimit)
            {
                _drawables.Add(drawable);
                Save();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a <see cref="IDrawable"/> from the project.
        /// </summary>
        /// <param name="drawable"></param>
        public void RemoveDrawable(IDrawable drawable)
        {
            _drawables.Remove(drawable);
            Save();
        }

        public void OnBeforeSerialize()
        {
            SerializedObjects.Clear();
            foreach (var drawable in _drawables)
            {
                if (drawable is ISerialized serialized)
                {
                    serialized.Serialize(SerializedObjects);
                }
            }
        }

        public void OnAfterDeserialize()
        {
            _drawables = new List<IDrawable>();
            foreach (var drawable in SerializedObjects)
            {
                if (drawable is ISerialized serialized)
                {
                    serialized.Deserialize(_drawables);
                }
            }
        }

        /// <summary>
        /// Registers a log to the project log history.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="description"></param>
        public void RegisterLog(string text, string description = null)
        {
            _logHistory.TextLog(text, description);
        }

        /// <summary>
        /// Registers a warning to the project log history.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="description"></param>
        public void RegisterWarning(string text, string description = null)
        {
            _logHistory.TextWarning(text, description);
        }

        /// <summary>
        /// Registers an error to the project log history.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="description"></param>
        public void RegisterError(string text, string description = null)
        {
            _logHistory.TextError(text, description);
        }

        /// <summary>
        /// Returns all logs saved to the project.
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<EditorLogHistory.Log> GetLogs()
        {
            return _logHistory.Logs;
        }

        /// <summary>
        /// Clears all log history.
        /// </summary>
        public void ClearLogHistory()
        {
            _logHistory.Clear();
        }
    }
}
