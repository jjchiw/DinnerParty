using Arango.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DinnerParty.Models
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

        public static string GetCollectionName(Type t)
        {
            return t.Name + "Collection";
        }

        public static string GetCollectionName<T>()
        {
            return GetCollectionName(typeof(T));
        }

        public static string BuildDocumentId<T>(long id)
        {
            return BuildDocumentId(typeof(T), id);
        }

        public static string BuildDocumentId(Type t, long id)
        {
            return string.Format("{0}/{1}", GetCollectionName(t), id);
        }
    }
}