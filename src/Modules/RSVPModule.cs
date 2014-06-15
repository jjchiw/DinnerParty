using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Security;
using DinnerParty.Models;
using Nancy.RouteHelpers;
using Arango.Client;

namespace DinnerParty.Modules
{
    public class RSVPAuthorizedModule : BaseModule
    {
        public RSVPAuthorizedModule(ArangoDatabase db)
            : base("/RSVP")
        {
            this.RequiresAuthentication();

            Post["/Cancel/{id}"] = parameters =>
            {
                Dinner dinner = db.Document.Get<Dinner>(ArangoModelBase.BuildDocumentId<Dinner>(parameters.id));

                RSVP rsvp = dinner.RSVPs
                    .Where(r => this.Context.CurrentUser.UserName == (r.AttendeeNameId ?? r.AttendeeName))
                    .SingleOrDefault();

                if (rsvp != null)
                {
                    dinner.RSVPs.Remove(rsvp);
                    db.Document.Update<Dinner>(dinner);

                }

                return "Sorry you can't make it!";
            };

            Post["/Register/{id}"] = parameters =>
            {
                Dinner dinner = db.Document.Get<Dinner>(ArangoModelBase.BuildDocumentId<Dinner>(parameters.id));

                if (!dinner.IsUserRegistered(this.Context.CurrentUser.UserName))
                {

                    RSVP rsvp = new RSVP();
                    rsvp.AttendeeNameId = this.Context.CurrentUser.UserName;
                    rsvp.AttendeeName = ((UserIdentity)this.Context.CurrentUser).FriendlyName;

                    dinner.RSVPs.Add(rsvp);

                    db.Document.Update<Dinner>(dinner);
                }

                return "Thanks - we'll see you there!";
            };
        }
    }
}