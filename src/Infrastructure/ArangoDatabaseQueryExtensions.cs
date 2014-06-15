using Arango.Client;
using DinnerParty.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DinnerParty.Infrastructure
{
    public class DinnersIndexModel
    {
        public List<string> RSVPs_AttendeeName { get; set; }
        public List<string> RSVPs_AttendeeNameId { get; set; }
        public string HostedById { get; set; }
        public string HostedBy { get; set; }
        public int DinnerID { get; set; }
        public string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public int RSVPCount { get; set; }
        public string Url { get; set; }
    }

    public class UserIndexModel
    {
        public string LoginType {get;set;} 
        public string UserId {get;set;} 
        public string Username {get;set;} 
        public string Password {get;set;}
        public string EMailAddress { get; set; }
        public string FriendlyName { get; set; }
    }

    public static class ArangoDatabaseQueryExtensions
    {
        public static int Count<T>(this ArangoQueryOperation query, ArangoQueryOperation filterOperation)
        {
            ArangoQueryOperation expression = new ArangoQueryOperation()
           .Aql(_ => _.LET("counter")
                     .FOR("item")
                       .IN(ArangoModelBase.GetCollectionName<T>(), filterOperation.RETURN.Val(1))
                     .RETURN.LENGTH(_.Var("counter")));

            return query.Aql(expression.ToString()).ToObject<int>();
        }

        public static int Count<T>(this ArangoQueryOperation query)
        {
            ArangoQueryOperation expression = new ArangoQueryOperation()
           .Aql(_ => _.LET("counter")
                     .FOR("item")
                       .IN(ArangoModelBase.GetCollectionName<T>(), _.RETURN.Val(1))
                     .RETURN.LENGTH(_.Var("counter")));

            return query.Aql(expression.ToString()).ToObject<int>();
        }

        public static List<T> WhereQuery<T>(this ArangoQueryOperation query, ArangoQueryOperation filterOperation)
        {
            var expression = new ArangoQueryOperation();
            expression.Aql(_ => _.FOR("item")
                     .IN(ArangoModelBase.GetCollectionName<T>(), filterOperation)
                     .RETURN.Var("item")
            );

            return query.Aql(expression.ToString()).ToList<T>();

        }

        public static List<T> GetQuery<T>(this ArangoQueryOperation query, ArangoQueryOperation filterOperation)
        {
            var expression = new ArangoQueryOperation();
            expression.Aql(_ => _.FOR("item")
                     .IN(ArangoModelBase.GetCollectionName<T>(), filterOperation)
                     .RETURN.Var("item")
            );

            return query.Aql(expression.ToString()).ToList<T>();

        }

        public static List<Dinner> DinnersIndex(this ArangoQueryOperation query, ArangoQueryOperation filterOperation,
                                                    ArangoQueryOperation sortOperation = null)
        {

            ArangoQueryOperation expression = new ArangoQueryOperation()
           .Aql(_ => _.FOR("item")
                       .IN(ArangoModelBase.GetCollectionName<Dinner>(), filterOperation)
                            .LET("RSVPs_AttendeeName").List(_
                                .FOR("rsvp")
                                .Var("item.RSVPs")
                                .RETURN.Var("rsvp.AttendeeName")
                            )
                             .LET("RSVPs_AttendeeNameId").List(_
                                .FOR("rsvp")
                                .Var("item.RSVPs")
                                .RETURN.Var("rsvp.AttendeeNameId")
                            )
                            .LET("RSVPTemp").List(_
                                .FOR("rsvp")
                                .Var("item.RSVPs")
                                .RETURN.Val(1)
                            )
                            .LET("RSVPCount").LENGTH(_
                                .Var("RSVPTemp")
                            ));

            if (sortOperation != null)
                expression.Aql(_ => sortOperation);

            //expression.Aql(_ => _.RETURN.Object(_
            //                .Field("item").Var("item")));

            expression.Aql(_ => _.RETURN.Var("item"));

            var q = expression.ToString();
            return query.Aql(q).ToList<Dinner>();
        }

        public static List<UserIndexModel> IndexUserLogin(this ArangoQueryOperation query, ArangoQueryOperation filterOperation)
        {

            ArangoQueryOperation expression = new ArangoQueryOperation()
           .Aql(_ => _.FOR("item")
                       .IN(ArangoModelBase.GetCollectionName<UserModel>(), filterOperation)
                            .RETURN.Object(
                                _.Field("LoginType").Var("item.LoginType")
                                .Field("UserId").Var("item.UserId")
                                .Field("Username").Var("item.Username")
                                .Field("Password").Var("item.Password")
                                .Field("EMailAddress").Var("item.EMailAddress")
                                .Field("FriendlyName").Var("item.FriendlyName")
                            ));

            return query.Aql(expression.ToString()).ToList<UserIndexModel>();
        }

        public static string BuildDelimiters(this object obj)
        {
            return string.Format("\"{0}\"", obj);
        }
    }
}