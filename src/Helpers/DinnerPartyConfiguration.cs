using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DinnerParty.Helpers
{
    public static class DinnerPartyConfiguration
    {
        public static string ArangoDbHost = ConfigurationManager.AppSettings["arangoDbHost"];
        public static int ArangoDbPort = int.Parse(ConfigurationManager.AppSettings["arangoDbPort"]);
        public static bool ArangoDbIsSecured = bool.Parse(ConfigurationManager.AppSettings["arangoDbIsSecured"]);
        public static string ArangoDbName = ConfigurationManager.AppSettings["arangoDbName"];
        public static string ArangoDbAlias = ConfigurationManager.AppSettings["arangoDbAlias"];
    }
}