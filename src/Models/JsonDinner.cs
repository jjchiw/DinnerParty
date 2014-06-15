﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DinnerParty.Models
{
    public class JsonDinner
    {
        public long DinnerID { get; set; }
        public DateTime EventDate { get; set; }
        public string Title { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Description { get; set; }
        public int RSVPCount { get; set; }
        public string Url { get; set; }
    }
}