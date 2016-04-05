using System.Collections.Generic;
using Nancy.ViewEngines.Razor;

namespace Foundoc.Manager.Nancy
{
    public class NancyRazorConfiguration : IRazorConfiguration
    {
        public IEnumerable<string> GetAssemblyNames()
        {
            yield return "Nancy";
            yield return "Nancy.ViewEngines.Razor";
            yield return "Foundoc.Manager";
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return "Nancy";
            yield return "Nancy.ViewEngines.Razor";
            yield return "Nancy.Validation";
            yield return "System.Globalization";
            yield return "System.Collections.Generic";
            yield return "System.Linq";
            yield return "Foundoc.Manager";
        }

        public bool AutoIncludeModelNamespace { get { return true; } }
    }
}
