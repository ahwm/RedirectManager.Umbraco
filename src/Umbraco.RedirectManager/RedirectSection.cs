using Umbraco.Core.Models.Sections;

namespace RedirectManager
{
    public class RedirectSection : ISection
    {
        public string Alias => "redirects";

        public string Name => "Redirects";
    }
}