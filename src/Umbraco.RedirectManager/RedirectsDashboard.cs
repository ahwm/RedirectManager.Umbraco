using System;
using Umbraco.Core.Dashboards;

namespace RedirectManager
{
    public class RedirectsDashboard : IDashboard
    {
        public string Alias => "redirectsDashboard";

        public string[] Sections => new[]
        {
            "redirects"
        };

        public string View => "/App_Plugins/Redirects/overview.html";

        public IAccessRule[] AccessRules => Array.Empty<IAccessRule>();
    }
}