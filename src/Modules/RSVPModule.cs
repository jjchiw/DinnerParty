using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Security;
using DinnerParty.Models;
using Nancy.RouteHelpers;
using Arango.Client;
using Commons.ArangoDb;
using DinnerParty.Infrastructure;

namespace DinnerParty.Modules
{
    public class RSVPAuthorizedModule : BaseModule
    {
        public RSVPAuthorizedModule(IArangoStoreDb store)
            : base("/RSVP")
        {
            this.RequiresAuthentication();

            Post["/Cancel/{id}"] = parameters =>
            {
                Dinner dinner = store.Get<Dinner>(parameters.id);

                var userId = ((UserIdentity)this.Context.CurrentUser).Id;
                ArangoQueryOperation op = new ArangoQueryOperation();
                op.Aql(_ => _.FILTER(_.Var("item._to"), ArangoOperator.Equal, _.Val(userId))
                            .AND(_.Var("item._from"), ArangoOperator.Equal, _.Val(dinner._Id)));

                var rsvp = store.SingleEdge<RSVP>(op);

                if (rsvp != null)
                {
                    store.DeleteEdge<RSVP>(rsvp._Id);
                }

                return "Sorry you can't make it!";
            };

            Post["/Register/{id}"] = parameters =>
            {
                Dinner dinner = store.Get<Dinner>(parameters.id);
                var userId = ((UserIdentity)this.Context.CurrentUser).Id;
                
                ArangoQueryOperation op = new ArangoQueryOperation();
                op.Aql(_ => _.FILTER(_.Var("item._to"), ArangoOperator.Equal, _.Val(userId)));

                if (store.QueryEdge<RSVP>(op).Count == 0)
                {
                    RSVP rsvp = new RSVP
                    {
                        AttendeeNameId = this.Context.CurrentUser.UserName,
                        AttendeeName = ((UserIdentity)this.Context.CurrentUser).FriendlyName,
                        _From = dinner._Id,
                        _To = userId,
                        ReservedAt = DateTime.UtcNow
                    };
                    
                    store.Update<Dinner>(dinner);
                    store.CreateEdge<RSVP>(rsvp);
                }

                return "Thanks - we'll see you there!";
            };
        }
    }
}