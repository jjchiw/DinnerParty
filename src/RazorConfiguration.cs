using Nancy.ViewEngines.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DinnerParty
{
    public class RazorConfig : IRazorConfiguration
    {
        public IEnumerable<string> GetAssemblyNames()
        {
            yield return "System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            yield return "Nancy.Validation.DataAnnotations";
            yield return "DinnerParty";
            yield return "Nancy";
            yield return "PagedList";
            yield return "System.Configuration";
            yield return "Commons.ArangoDb";
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return "DinnerParty.HtmlExtensions";
            yield return "Nancy.Validation";
            yield return "PagedList";
            yield return "DinnerParty.Helpers";
            yield return "System.Globalization";
            yield return "System.Collections.Generic";
            yield return "System.Linq";
            yield return "System.Configuration";
            yield return "Commons.ArangoDb";
        }

        public bool AutoIncludeModelNamespace
        {
            get { return true; }
        }
    }
}