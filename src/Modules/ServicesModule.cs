using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DinnerParty.Models;
using Nancy;
using Arango.Client;
using DinnerParty.Infrastructure;
using Commons.ArangoDb;

namespace DinnerParty.Modules
{
    public class ServicesModule : BaseModule
    {
        public ServicesModule(IArangoStoreDb store)
            : base("/services")
        {
            Get["/RSS"] = parameters =>
                {

                    var where = string.Format(@" 
                                                FILTER DATE_TIMESTAMP(item.EventDate) == DATE_TIMESTAMP('{0}')
                                                SORT item.EventDate
                                                LIMIT {1}", DateTime.Now.Date.ToString("u"), Request.Form.limit);

                    ArangoQueryOperation whereOperation = new ArangoQueryOperation().Aql(where);

                    var dinners = store.Query<Dinner, IndexDinner>(whereOperation)
                        .Select(x =>
                            new Dinner
                            {
                                _Id = x._Id,
                                HostedBy = x.HostedBy,
                                HostedById = x.HostedById,
                                Title = x.Title,
                                Latitude = x.Latitude,
                                Longitude = x.Longitude,
                                Description = x.Description,
                                EventDate = x.EventDate,
                            }
                        );

                    if (dinners == null)
                    {
                        base.Page.Title = "Nerd Dinner Not Found";
                        return View["NotFound", base.Model];
                    }

                    return this.Response.AsRSS(dinners, "Upcoming Nerd Dinners");
                };
        }
    }
}