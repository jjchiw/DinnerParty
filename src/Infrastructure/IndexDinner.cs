using Arango.Client;
using Commons.ArangoDb;
using DinnerParty.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DinnerParty.Infrastructure
{
    public class IndexDinner : IArangoViewIndex
    {
        public ArangoQueryOperation Execute(ArangoQueryOperation filterOperation, string forItemName = "item")
        {
            ArangoQueryOperation expression = new ArangoQueryOperation()
           .Aql(_ => _.FOR(forItemName)
                       .IN(ArangoStoreDb.GetCollectionName<Dinner>(), filterOperation)
                            .LET("RSVPs").List(_
                                .FOR("rsvp")
                                .Var(ArangoStoreDb.GetEdgeCollectionName<RSVP>())
                                .FILTER(_.Var("rsvp._from"), ArangoOperator.Equal, _.Var(forItemName + "._id"))
                                .SORT(_.Var("rsvp.ReservedAt")).Direction(ArangoSortDirection.DESC)
                                .LIMIT(10)
                                .RETURN.Var("rsvp")
                            )
                            .LET("RSVPTemp").List(_
                                .FOR("rsvp")
                                .Var(ArangoStoreDb.GetEdgeCollectionName<RSVP>())
                                .FILTER(_.Var("rsvp._from"), ArangoOperator.Equal, _.Var(forItemName + "._id"))
                                .RETURN.Val(1)
                            )
                            .LET("RSVPCount").LENGTH(_
                                .Var("RSVPTemp")
                            )
                            .LET("returnObj").Aql("MERGE("+forItemName+", {RSVPs : RSVPs})"));

            return expression;
        }
    }
}