using Commons.ArangoDb;
using System.ComponentModel.DataAnnotations;
using System;

namespace DinnerParty.Models
{
    public class RSVP : ArangoBaseEdgeModel
    {
        public string AttendeeName { get; set; }
        public string AttendeeNameId { get; set; }
        public DateTime ReservedAt { get; set; }
    }
}