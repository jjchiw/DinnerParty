using Arango.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DinnerParty.Data
{
    public interface IArangoViewIndex
    {
        ArangoQueryOperation Execute(ArangoQueryOperation filterOperation, ArangoQueryOperation sortOperation = null, string forItemName = "item");
    }
}