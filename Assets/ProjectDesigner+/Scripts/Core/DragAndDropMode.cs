/// <summary>
/// Mode of the dragging
/// </summary>

namespace ProjectDesigner.Core
{
    public enum DragAndDropMode
    {
        /// <summary>
        /// Empty drag refers to the motion done by middle mouse button.
        /// </summary>
        Empty,
        /// <summary>
        /// Refers to left click mouse button drag. Can be used for selecting items on the window.
        /// </summary>
        Selection,
        /// <summary>
        /// Used when trying to connect a <see cref="ConnectionBase"/> to a <see cref="NodeBase"/>. Does not completely refer to a dragging action.
        /// </summary>
        Connection,
        /// <summary>
        /// Used for trying to move items via dragging.
        /// </summary>
        MultipleItems
    }


}
