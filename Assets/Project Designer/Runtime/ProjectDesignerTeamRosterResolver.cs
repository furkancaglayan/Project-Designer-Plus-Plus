using System;
using System.Linq;

namespace ProjectDesigner.V2.Data
{
    public static class ProjectDesignerTeamRosterResolver
    {
        public static ProjectDesignerTeamMemberData ResolveMember(ProjectDesignerTeamRosterAsset roster, string assigneeId)
        {
            if (roster == null || string.IsNullOrWhiteSpace(assigneeId))
            {
                return null;
            }

            roster.EnsureDefaults();
            string trimmedAssigneeId = assigneeId.Trim();

            ProjectDesignerTeamMemberData byId = roster.Members.FirstOrDefault(member =>
                member != null &&
                string.Equals(member.Id, trimmedAssigneeId, StringComparison.OrdinalIgnoreCase));
            if (byId != null)
            {
                return byId;
            }

            return roster.Members.FirstOrDefault(member =>
                member != null &&
                string.Equals(member.DisplayName, trimmedAssigneeId, StringComparison.OrdinalIgnoreCase));
        }

        public static bool TryResolveMember(ProjectDesignerTeamRosterAsset roster, string assigneeId, out ProjectDesignerTeamMemberData member)
        {
            member = ResolveMember(roster, assigneeId);
            return member != null;
        }

        public static string GetDisplayName(ProjectDesignerTeamRosterAsset roster, string assigneeId)
        {
            if (string.IsNullOrWhiteSpace(assigneeId))
            {
                return string.Empty;
            }

            ProjectDesignerTeamMemberData member = ResolveMember(roster, assigneeId);
            return member == null ? assigneeId.Trim() : member.DisplayName;
        }

        public static string GetAccentColor(ProjectDesignerTeamRosterAsset roster, string assigneeId)
        {
            ProjectDesignerTeamMemberData member = ResolveMember(roster, assigneeId);
            return member == null ? string.Empty : member.AccentColor;
        }
    }
}
