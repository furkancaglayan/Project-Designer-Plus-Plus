using System;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// Adds metadata to <see cref="NodeBase"/> types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NodeBaseMetaDataAttribute : Attribute
    {
        /// <summary>
        /// Display name of the node in the context menu.
        /// </summary>
        public string DisplayName { get; private set; }
        /// <summary>
        /// Maximum count of a node type to create in the editor.
        /// </summary>
        public int MaxCount { get; private set; }
        /// <summary>
        /// If set to true, a node of type is created and added to the window when it's created.
        /// </summary>
        public bool AddOnWindowCreation { get; private set; }

        public NodeBaseMetaDataAttribute(string displayName, int maxCount, bool addOnWindowCreation = false)
        {
            DisplayName = displayName;
            MaxCount = maxCount;
            AddOnWindowCreation = addOnWindowCreation;
        }

        public NodeBaseMetaDataAttribute(string displayName, bool addOnWindowCreation = false) : this(displayName, int.MaxValue, addOnWindowCreation)
        {

        }
    }
}
