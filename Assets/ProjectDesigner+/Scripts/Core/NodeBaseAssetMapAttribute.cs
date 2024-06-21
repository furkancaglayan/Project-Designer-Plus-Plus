using JetBrains.Annotations;
using System;
using System.Diagnostics;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// Can be used on static methods to set as a certain callback to be called when an asset is dragged into the editor.
    /// <code>
    /// [NodeBaseAssetMap(typeof(MonoScript))]
    /// private static NodeBase CreateClassNodeFromMonoScript(UnityEngine.Object obj)
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class NodeBaseAssetMapAttribute : Attribute
    {
        /// <summary>
        /// Type of the asset to register. For example: <see cref="UnityEngine.Texture2D"/>.
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// Priority is used to override <see cref="NodeBaseAssetMapAttribute"/> functions with the same type. Higher priority function will be used when an asset is dragged.
        /// </summary>
        public int Priority { get; private set; }

        public NodeBaseAssetMapAttribute(Type type, int priority = 1)
        {
            Debug.Assert(type != null);
            if (type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                Type = type;
            }
            else
            {
                Debug.Assert(false);
            }
            Priority = priority;
        }
    }
}
