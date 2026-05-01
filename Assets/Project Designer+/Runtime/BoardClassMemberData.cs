using System;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [Serializable]
    public sealed class BoardClassMemberData
    {
        [SerializeField]
        private string _signature;
        [SerializeField]
        private string _visibility;

        public string Signature
        {
            get { return _signature; }
            set { _signature = value ?? string.Empty; }
        }

        public string Visibility
        {
            get { return _visibility; }
            set { _visibility = value ?? string.Empty; }
        }

        public BoardClassMemberData()
        {
            _signature = string.Empty;
            _visibility = "private";
        }

        public BoardClassMemberData(string signature, string visibility)
        {
            Signature = signature;
            Visibility = visibility;
        }

        public BoardClassMemberData Clone()
        {
            return new BoardClassMemberData(Signature, Visibility);
        }
    }
}
