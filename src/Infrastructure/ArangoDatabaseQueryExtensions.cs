using Arango.Client;
using DinnerParty.Data;
using DinnerParty.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DinnerParty.Infrastructure
{
    //public class DinnersIndexModel
    //{
    //    public List<string> RSVPs_AttendeeName { get; set; }
    //    public List<string> RSVPs_AttendeeNameId { get; set; }
    //    public string HostedById { get; set; }
    //    public string HostedBy { get; set; }
    //    public int DinnerID { get; set; }
    //    public string Title { get; set; }
    //    public double Latitude { get; set; }
    //    public double Longitude { get; set; }
    //    public string Description { get; set; }
    //    public DateTime EventDate { get; set; }
    //    public int RSVPCount { get; set; }
    //    public string Url { get; set; }
    //}

    //public class UserIndexModel
    //{
    //    public string LoginType {get;set;} 
    //    public string UserId {get;set;} 
    //    public string Username {get;set;} 
    //    public string Password {get;set;}
    //    public string EMailAddress { get; set; }
    //    public string FriendlyName { get; set; }
    //}

    public class IndexUserLogin : IArangoViewIndex
    {
        public ArangoQueryOperation Execute(ArangoQueryOperation filterOperation, ArangoQueryOperation sortOperation = null, string forItemName = "item")
        {

            ArangoQueryOperation expression = new ArangoQueryOperation()
           .Aql(_ => _.FOR(forItemName)
                       .IN(ArangoStore.GetCollectionName<UserModel>(), filterOperation)
                            .RETURN.Object(
                                _.Field("LoginType").Var(forItemName + ".LoginType")
                                .Field("UserId").Var(forItemName + ".UserId")
                                .Field("Username").Var(forItemName + ".Username")
                                .Field("Password").Var(forItemName + ".Password")
                                .Field("EMailAddress").Var(forItemName + ".EMailAddress")
                                .Field("FriendlyName").Var(forItemName + ".FriendlyName")
                            ));

            return expression;
        }
    }

    public class IndexDinner : IArangoViewIndex
    {
        public ArangoQueryOperation Execute(ArangoQueryOperation filterOperation, ArangoQueryOperation sortOperation = null, string forItemName = "item")
        {
            ArangoQueryOperation expression = new ArangoQueryOperation()
           .Aql(_ => _.FOR(forItemName)
                       .IN(ArangoStore.GetCollectionName<Dinner>(), filterOperation)
                            .LET("RSVPs_AttendeeName").List(_
                                .FOR("rsvp")
                                .Var(forItemName + ".RSVPs")
                                .RETURN.Var("rsvp.AttendeeName")
                            )
                             .LET("RSVPs_AttendeeNameId").List(_
                                .FOR("rsvp")
                                .Var(forItemName + ".RSVPs")
                                .RETURN.Var("rsvp.AttendeeNameId")
                            )
                            .LET("RSVPTemp").List(_
                                .FOR("rsvp")
                                .Var(forItemName + ".RSVPs")
                                .RETURN.Val(1)
                            )
                            .LET("RSVPCount").LENGTH(_
                                .Var("RSVPTemp")
                            ));

            if (sortOperation != null)
                expression.Aql(_ => sortOperation);

            expression.Aql(_ => _.RETURN.Var(forItemName));

            return expression;
        }
    }
}