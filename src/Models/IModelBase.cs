using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DinnerParty.Models
{
    public abstract class ModelBase : IModelBase
    {
        string _collectionName = null;

        public virtual string Id { get; set; }

        public virtual string CollectionName
        {
            get
            {
                if(_collectionName == null)
                    _collectionName = GetType().Name + "Collection";
                return _collectionName;
            }
            
        }
    }

    public interface IModelBase
    {
        string Id { get; set; }
        string CollectionName { get; }
    }
}