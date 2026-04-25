using System;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [Serializable]
    public sealed class ReferenceNodeModel : BoardNodeModel
    {
        [SerializeField]
        private string _summary;
        [SerializeField]
        private string _assetPath;
        [SerializeField]
        private string _imageAssetPath;
        [SerializeField]
        private string _textReference;
        [SerializeField]
        private string _externalUrl;

        public override string Category
        {
            get { return BoardNodeCategories.Reference; }
        }

        public string Summary
        {
            get { return _summary; }
            set { _summary = value ?? string.Empty; }
        }

        public string AssetPath
        {
            get { return _assetPath; }
            set { _assetPath = value ?? string.Empty; }
        }

        public string ImageAssetPath
        {
            get { return _imageAssetPath; }
            set { _imageAssetPath = value ?? string.Empty; }
        }

        public string TextReference
        {
            get { return _textReference; }
            set { _textReference = value ?? string.Empty; }
        }

        public string ExternalUrl
        {
            get { return _externalUrl; }
            set { _externalUrl = value ?? string.Empty; }
        }

        public ReferenceNodeModel()
            : base(BoardNodeTypeIds.Reference, "Reference", new Vector2(460f, 420f), new Vector2(320f, 220f))
        {
            _summary = "Link assets, screenshots, and inspiration to the board.";
            _assetPath = string.Empty;
            _imageAssetPath = string.Empty;
            _textReference = string.Empty;
            _externalUrl = string.Empty;
        }

        public override BoardNodeModel Clone()
        {
            var clone = new ReferenceNodeModel
            {
                Summary = Summary,
                AssetPath = AssetPath,
                ImageAssetPath = ImageAssetPath,
                TextReference = TextReference,
                ExternalUrl = ExternalUrl
            };
            CopyCommonTo(clone);
            return clone;
        }

        public override string GetSearchText()
        {
            return string.Join(" ", new[] { base.GetSearchText(), Summary, AssetPath, TextReference, ExternalUrl });
        }
    }
}
