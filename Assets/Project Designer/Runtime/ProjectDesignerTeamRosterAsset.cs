using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectDesigner.V2.Data
{
    [CreateAssetMenu(fileName = ProjectDesignerProductInfo.TeamRosterAssetName, menuName = ProjectDesignerProductInfo.TeamRosterCreateAssetMenuPath)]
    public sealed class ProjectDesignerTeamRosterAsset : ScriptableObject
    {
        [SerializeField]
        private List<ProjectDesignerTeamMemberData> _members = new List<ProjectDesignerTeamMemberData>();

        public List<ProjectDesignerTeamMemberData> Members
        {
            get
            {
                EnsureDefaults();
                return _members;
            }
        }

        public void EnsureDefaults()
        {
            _members = _members ?? new List<ProjectDesignerTeamMemberData>();

            var sanitizedMembers = new List<ProjectDesignerTeamMemberData>();
            HashSet<string> usedIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            foreach (ProjectDesignerTeamMemberData member in _members.Where(member => member != null))
            {
                member.EnsureDefaults();

                string baseId = string.IsNullOrWhiteSpace(member.Id)
                    ? ProjectDesignerTeamMemberData.CreateId(member.DisplayName)
                    : member.Id;
                string uniqueId = baseId;
                int suffix = 2;
                while (usedIds.Contains(uniqueId))
                {
                    uniqueId = baseId + "-" + suffix;
                    suffix++;
                }

                member.Id = uniqueId;
                usedIds.Add(uniqueId);
                sanitizedMembers.Add(member);
            }

            _members = sanitizedMembers;
        }

        private void OnEnable()
        {
            EnsureDefaults();
        }
    }
}
