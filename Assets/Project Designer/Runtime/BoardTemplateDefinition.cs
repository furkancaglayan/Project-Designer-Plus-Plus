using System;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [Serializable]
    public sealed class BoardTemplateDefinition
    {
        [SerializeField]
        private string _id;
        [SerializeField]
        private string _name;
        [SerializeField]
        private string _description;

        public string Id
        {
            get { return _id; }
            set { _id = value ?? BoardPresetIds.Empty; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value ?? string.Empty; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value ?? string.Empty; }
        }

        public BoardTemplateDefinition()
        {
            _id = BoardPresetIds.Empty;
            _name = "Empty";
            _description = "Start with a blank planning board.";
        }

        public BoardTemplateDefinition(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public BoardTemplateDefinition Clone()
        {
            return new BoardTemplateDefinition(Id, Name, Description);
        }
    }
}
