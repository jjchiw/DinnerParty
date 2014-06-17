﻿using Commons.ArangoDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DinnerParty.Models
{
    public class UserModel : ArangoBaseModel
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string FriendlyName { get; set; }
        public string EMailAddress { get; set; }
        public string LoginType { get; set; }
        public string Password { get; set; }
    }
}