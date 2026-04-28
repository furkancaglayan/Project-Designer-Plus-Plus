using System;
using System.Text;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [Serializable]
    public sealed class ProjectDesignerTeamMemberData
    {
        [SerializeField]
        private string _id;
        [SerializeField]
        private string _displayName;
        [SerializeField]
        private string _role;
        [SerializeField]
        private string _discipline;
        [SerializeField]
        private string _accentColor = "#4C7AFF";
        [SerializeField]
        private bool _isActive = true;

        public string Id
        {
            get { return string.IsNullOrWhiteSpace(_id) ? CreateId(_displayName) : _id.Trim(); }
            set { _id = string.IsNullOrWhiteSpace(value) ? CreateId(_displayName) : value.Trim(); }
        }

        public string DisplayName
        {
            get { return string.IsNullOrWhiteSpace(_displayName) ? "Team Member" : _displayName.Trim(); }
            set { _displayName = value ?? string.Empty; }
        }

        public string Role
        {
            get { return _role ?? string.Empty; }
            set { _role = value ?? string.Empty; }
        }

        public string Discipline
        {
            get { return _discipline ?? string.Empty; }
            set { _discipline = value ?? string.Empty; }
        }

        public string AccentColor
        {
            get { return string.IsNullOrWhiteSpace(_accentColor) ? "#4C7AFF" : _accentColor.Trim(); }
            set { _accentColor = string.IsNullOrWhiteSpace(value) ? "#4C7AFF" : value.Trim(); }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        public void EnsureDefaults()
        {
            if (string.IsNullOrWhiteSpace(_displayName))
            {
                _displayName = "Team Member";
            }

            if (string.IsNullOrWhiteSpace(_id))
            {
                _id = CreateId(_displayName);
            }

            if (string.IsNullOrWhiteSpace(_accentColor))
            {
                _accentColor = "#4C7AFF";
            }
        }

        public static string CreateId(string displayName)
        {
            string source = string.IsNullOrWhiteSpace(displayName) ? "team-member" : displayName.Trim().ToLowerInvariant();
            var builder = new StringBuilder(source.Length);
            bool previousHyphen = false;

            foreach (char character in source)
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(character);
                    previousHyphen = false;
                    continue;
                }

                if (character == ' ' || character == '-' || character == '_' || character == '/')
                {
                    if (!previousHyphen && builder.Length > 0)
                    {
                        builder.Append('-');
                        previousHyphen = true;
                    }
                }
            }

            string id = builder.ToString().Trim('-');
            return string.IsNullOrWhiteSpace(id) ? "team-member" : id;
        }
    }
}
