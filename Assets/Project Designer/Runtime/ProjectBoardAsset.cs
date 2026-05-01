using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [CreateAssetMenu(fileName = ProjectDesignerProductInfo.PlanningBoardName, menuName = ProjectDesignerProductInfo.CreateAssetMenuPath)]
    public sealed class ProjectBoardAsset : ScriptableObject
    {
        [SerializeField]
        private string _schemaVersion = "2.0.0-preview";
        [SerializeField]
        private BoardDocument _document = new BoardDocument(ProjectDesignerProductInfo.DefaultBoardName);

        public string SchemaVersion
        {
            get { return _schemaVersion; }
        }

        public BoardDocument Document
        {
            get
            {
                if (_document == null)
                {
                    _document = new BoardDocument(name);
                }

                _document.EnsureDefaults();
                return _document;
            }
        }

        public void ResetDocument(BoardDocument document)
        {
            if (_document == null)
            {
                _document = new BoardDocument(name);
            }

            if (document == null)
            {
                _document = new BoardDocument(name);
            }
            else
            {
                _document.CopyFrom(document);
            }
        }

        public static ProjectBoardAsset CreateTransient(BoardDocument document = null)
        {
            ProjectBoardAsset asset = CreateInstance<ProjectBoardAsset>();
            asset.name = "Transient " + ProjectDesignerProductInfo.PlanningBoardName;
            asset.ResetDocument(document ?? new BoardDocument(asset.name));
            return asset;
        }

        private void OnEnable()
        {
            if (_document == null)
            {
                _document = new BoardDocument(name);
            }

            _document.EnsureDefaults();
        }
    }
}
