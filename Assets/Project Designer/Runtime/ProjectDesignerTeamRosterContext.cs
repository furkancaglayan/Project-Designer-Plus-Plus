using System;

namespace ProjectDesigner.V2.Data
{
    public static class ProjectDesignerTeamRosterContext
    {
        private static Func<ProjectDesignerTeamRosterAsset> _currentRosterProvider;

        public static ProjectDesignerTeamRosterAsset CurrentRoster
        {
            get { return _currentRosterProvider == null ? null : _currentRosterProvider.Invoke(); }
        }

        public static void SetProvider(Func<ProjectDesignerTeamRosterAsset> provider)
        {
            _currentRosterProvider = provider;
        }
    }
}
