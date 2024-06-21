namespace ProjectDesigner.Core
{
    /// <summary>
    /// Defines how a drawable is created. Currently only used for <see cref="NodeBase"/>.
    /// </summary>
    public enum DrawableCreationType
    {
        /// <summary>
        /// Created by context click on the editor.
        /// </summary>
        Default,
        /// <summary>
        /// Created by dragging an asset into the editor.
        /// </summary>
        FromAsset,
        /// <summary>
        /// Created by copying or duplicating node(s).
        /// </summary>
        FromCopy,
        /// <summary>
        /// Created by reverting an <see cref="EditorContextAction"/>.
        /// </summary>
        RevertDelete,
    }
}
