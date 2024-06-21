using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// Basic interface to make items saveable/serializable in the used <see cref="Project"/>. This is useful for saving some of the <see cref="IDrawable"/>s outside of the <see cref="Project"/>.
    /// One such example is <see cref="ConnectionBase"/>s.
    /// </summary>
    public interface ISerialized : IDrawable
    {
        /// <summary>
        /// Serializes items into <paramref name="serializeds"/> list.
        /// </summary>
        /// <param name="serializeds">List of all saved items.</param>
        void Serialize(List<ISerialized> serializeds);
        /// <summary>
        /// Deserializes items into <paramref name="drawables"/>.
        /// </summary>
        /// <param name="drawables">Final drawables to draw in the editor.</param>
        void Deserialize(List<IDrawable> drawables);
    }
}
