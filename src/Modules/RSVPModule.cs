using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Security;
using DinnerParty.Models;
using Nancy.RouteHelpers;
using Arango.Client;
using DinnerParty.Data;

namespace DinnerParty.Modules
{
    public class RSVPAuthorizedModule : BaseModule
    {
        public RSVPAuthorizedModule(ArangoStore store)
            : base("/RSVP")
        {
            this.RequiresAuthentication();

            Post["/Cancel/{id}"] = parameters =>
            {
                Dinner dinner = store.Get<Dinner>(parameters.id);

                RSVP rsvp = dinner.RSVPs
                    .Where(r => this.Context.CurrentUser.UserName == (r.AttendeeNameId ?? r.AttendeeName))
                    .SingleOrDefault();

                if (rsvp != null)
                {
                    dinner.RSVPs.Remove(rsvp);
                    store.Update<Dinner>(dinner);

                }

                return "Sorry you can't make it!";
            };

            Post["/Register/{id}"] = parameters =>
            {
                Dinner dinner = store.Get<Dinner>(parameters.id);

                if (!dinner.IsUserRegistered(this.Context.CurrentUser.UserName))
                {

                    RSVP rsvp = new RSVP();
                    rsvp.AttendeeNameId = this.Context.CurrentUser.UserName;
                    rsvp.AttendeeName = ((UserIdentity)this.Context.CurrentUser).FriendlyName;

                    dinner.RSVPs.Add(rsvp);

                    store.Update<Dinner>(dinner);
                }

                return "Thanks - we'll see you there!";
            };
        }
    }
}