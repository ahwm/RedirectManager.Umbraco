using Umbraco.Cms.Core.Sections;

namespace RedirectManager
{
    public class RedirectSection : ISection
    {
        public string Alias => "redirects";

        public string Name => "Redirects";
    }
}