using Arango.Client;
using Commons.ArangoDb;
using DinnerParty.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DinnerParty.Infrastructure
{
    public class IndexUserLogin : IArangoViewIndex
    {
        public ArangoQueryOperation Execute(ArangoQueryOperation filterOperation, string forItemName = "item")
        {

            ArangoQueryOperation expression = new ArangoQueryOperation()
           .Aql(_ => _.FOR(forItemName)
                       .IN(ArangoStoreDb.GetCollectionName<UserModel>(), filterOperation)
                       .LET("returnObj").Var(forItemName));

            return expression;
        }
    }
}