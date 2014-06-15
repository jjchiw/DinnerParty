using Arango.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DinnerParty.Data
{
    public abstract class ArangoModelBase
    {
        [ArangoProperty(Identity=true)]
        public virtual string _Id { get; set; }

        [ArangoProperty(Key=true)]
        public virtual string _Key { get; set; }

        [ArangoProperty(Serializable=false)]
        public virtual long Id 
        {
            get
            {
                if (_Key == null) return 0;

                return long.Parse(_Key);
            }
        }
    }
}