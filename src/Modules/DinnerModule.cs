using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using DinnerParty.Models;
using PagedList;
using DinnerParty.Helpers;
using Nancy.RouteHelpers;
using Nancy.ModelBinding;
using Nancy.Validation;
using System.ComponentModel;
using Arango.Client;
using DinnerParty.Infrastructure;
using DinnerParty.Data;

namespace DinnerParty.Modules
{
    public class DinnerModule : BaseModule
    {
        private readonly ArangoStore _store;
        private const int PageSize = 25;

        public DinnerModule(ArangoStore store)
        {
            this._store = store;
            const string basePath = "/dinners";

            Get[basePath] = Dinners;
            Get[basePath + "/page/{pagenumber}"] = Dinners;

            Get[basePath + "/{id}"] = parameters =>
            {
                if (!parameters.id.HasValue && String.IsNullOrWhiteSpace(parameters.id))
                {
                    return 404;
                }

                Dinner dinner = _store.Get<Dinner>(parameters.id);

                if (dinner == null)
                {
                    return 404;
                }

                base.Page.Title = dinner.Title;
                base.Model.Dinner = dinner;

                return View["Dinners/Details", base.Model];
            };
        }

        private Negotiator Dinners(dynamic parameters)
        {
            base.Page.Title = "Upcoming Nerd Dinners";
            List<Dinner> dinners = null;

            //Searching?
            if (this.Request.Query.q.HasValue)
            {
                string query = this.Request.Query.q;

                 var op = new ArangoQueryOperation();
                 op.Aql(_ => _.FILTER(_.CONTAINS(_.Var("item.Title"), _.Val(query)))
                               .OR(_.CONTAINS(_.Var("item.Description"), _.Val(query)))
                               .OR(_.CONTAINS(_.Var("item.HostedBy"), _.Val(query)))
                               .SORT(_.Var("item.EventDate"))
                     );

                 dinners = _store.Query<Dinner>(op);
            }
            else
            {
                var where = string.Format(@" 
                                                FILTER DATE_TIMESTAMP(item.EventDate) > DATE_TIMESTAMP('{0}')
                                                SORT item.EventDate", DateTime.Now.Date.ToString("u"));

                ArangoQueryOperation op = new ArangoQueryOperation().Aql(where);

                dinners = _store.Query<Dinner>(op);
            }

            int pageIndex = parameters.pagenumber.HasValue && !String.IsNullOrWhiteSpace(parameters.pagenumber) ? parameters.pagenumber : 1;

            base.Model.Dinners = dinners.ToPagedList(pageIndex, PageSize);

            return View["Dinners/Index", base.Model];
        }
    }

    public class DinnerModuleAuth : BaseModule
    {
        public DinnerModuleAuth(ArangoStore store)
            : base("/dinners")
        {
            this.RequiresAuthentication();

            Get["/create"] = parameters =>
            {
                Dinner dinner = new Dinner()
                {
                    EventDate = DateTime.Now.AddDays(7)
                };

                base.Page.Title = "Host a Nerd Dinner";

                base.Model.Dinner = dinner;

                return View["Create", base.Model];
            };

            Post["/create"] = parameters =>
                {
                    var dinner = this.Bind<Dinner>();
                    var result = this.Validate(dinner);

                    if (result.IsValid)
                    {
                        UserIdentity nerd = (UserIdentity)this.Context.CurrentUser;
                        dinner.HostedById = nerd.UserName;
                        dinner.HostedBy = nerd.FriendlyName;

                        RSVP rsvp = new RSVP();
                        rsvp.AttendeeNameId = nerd.UserName;
                        rsvp.AttendeeName = nerd.FriendlyName;

                        dinner.RSVPs = new List<RSVP>();
                        dinner.RSVPs.Add(rsvp);

                        store.Create<Dinner>(dinner);


                        return this.Response.AsRedirect("/dinners/" + dinner.Id);
                    }
                    else
                    {
                        base.Page.Title = "Host a Nerd Dinner";
                        base.Model.Dinner = dinner;
                        foreach (var item in result.Errors)
                        {
                            foreach (var err in item.Value)
                            {
                                foreach (var member in err.MemberNames)
                                {
                                    base.Page.Errors.Add(new ErrorModel() { Name = member, ErrorMessage = err.ErrorMessage });
                                }
                            }
                            
                        }
                    }

                    return View["Create", base.Model];
                };

            Get["/delete/" + Route.AnyIntAtLeastOnce("id")] = parameters =>
                {
                    Dinner dinner = store.Get<Dinner>(parameters.id);

                    if (dinner == null)
                    {
                        base.Page.Title = "Nerd Dinner Not Found";
                        return View["NotFound", base.Model];
                    }

                    if (!dinner.IsHostedBy(this.Context.CurrentUser.UserName))
                    {
                        base.Page.Title = "You Don't Own This Dinner";
                        return View["InvalidOwner", base.Model];
                    }

                    base.Page.Title = "Delete Confirmation: " + dinner.Title;

                    base.Model.Dinner = dinner;

                    return View["Delete", base.Model];
                };

            Post["/delete/" + Route.AnyIntAtLeastOnce("id")] = parameters =>
                {
                    Dinner dinner = store.Get<Dinner>(parameters.id);

                    if (dinner == null)
                    {
                        base.Page.Title = "Nerd Dinner Not Found";
                        return View["NotFound", base.Model];
                    }

                    if (!dinner.IsHostedBy(this.Context.CurrentUser.UserName))
                    {
                        base.Page.Title = "You Don't Own This Dinner";
                        return View["InvalidOwner", base.Model];
                    }

                    store.Delete<Dinner>(dinner._Id);

                    base.Page.Title = "Deleted";
                    return View["Deleted", base.Model];
                };

            Get["/edit" + Route.And() + Route.AnyIntAtLeastOnce("id")] = parameters =>
                {
                    Dinner dinner = store.Get<Dinner>(parameters.id);

                    if (dinner == null)
                    {
                        base.Page.Title = "Nerd Dinner Not Found";
                        return View["NotFound", base.Model];
                    }

                    if (!dinner.IsHostedBy(this.Context.CurrentUser.UserName))
                    {
                        base.Page.Title = "You Don't Own This Dinner";
                        return View["InvalidOwner", base.Model];
                    }

                    base.Page.Title = "Edit: " + dinner.Title;
                    base.Model.Dinner = dinner;

                    return View["Edit", base.Model];
                };

            Post["/edit" + Route.And() + Route.AnyIntAtLeastOnce("id")] = parameters =>
                {
                    Dinner dinner = store.Get<Dinner>(parameters.id);

                    if (!dinner.IsHostedBy(this.Context.CurrentUser.UserName))
                    {
                        base.Page.Title = "You Don't Own This Dinner";
                        return View["InvalidOwner", base.Model];
                    }

                    this.BindTo(dinner);

                    var result = this.Validate(dinner);

                    if (!result.IsValid)
                    {
                        base.Page.Title = "Edit: " + dinner.Title;
                        base.Model.Dinner = dinner;
                        foreach (var item in result.Errors)
                        {
                            foreach (var err in item.Value)
                            {
                                foreach (var member in err.MemberNames)
                                {
                                    base.Page.Errors.Add(new ErrorModel() { Name = member, ErrorMessage = err.ErrorMessage });
                                }
                            }
                            
                        }

                        return View["Edit", base.Model];
                    }

                    store.Update<Dinner>(dinner);

                    return this.Response.AsRedirect(string.Format("{0}/{1}", ModulePath, dinner.Id));

                };

            Get["/my"] = parameters =>
                {
                    string nerdName = this.Context.CurrentUser.UserName;

                    var op = new ArangoQueryOperation();
                    op.Aql(_ => _.LET("RSVPs_AttendeeNameList").List(_
                                .FOR("rsvp")
                                .Var("item.RSVPs")
                                .RETURN.Var("rsvp.AttendeeName")
                                )
                                 .LET("RSVPs_AttendeeNameIdList").List(_
                                    .FOR("rsvp")
                                    .Var("item.RSVPs")
                                    .RETURN.Var("rsvp.AttendeeNameId")
                                )
                                .FILTER(_.Var("item.HostedById"), ArangoOperator.Equal, _.Val(nerdName))
                                    .OR(_.Var("item.HostedBy"), ArangoOperator.Equal, _.Val(nerdName))
                                    .OR(_.Val(nerdName), ArangoOperator.In, _.Var("RSVPs_AttendeeNameList"))
                                    .OR(_.Val(nerdName), ArangoOperator.In, _.Var("RSVPs_AttendeeNameIdList"))
                                  .SORT(_.Var("item.EventDate"))
                        );

                    var userDinners = store.Query<Dinner, IndexDinner>(op).ToList();

                    base.Page.Title = "My Dinners";
                    base.Model.Dinners = userDinners;

                    return View["My", base.Model];
                };
        }
    }
}

