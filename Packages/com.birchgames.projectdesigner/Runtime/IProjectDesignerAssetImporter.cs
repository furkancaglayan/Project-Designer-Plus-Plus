using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    public interface IProjectDesignerAssetImporter
    {
        int Priority { get; }
        bool CanImport(Object asset);
        IEnumerable<BoardNodeModel> Import(Object asset, Vector2 position);
    }
}
