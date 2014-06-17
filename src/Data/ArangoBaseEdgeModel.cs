/*
 * Created by SharpDevelop.
 * User: JuanJ
 * Date: 6/15/2014
 * Time: 10:58 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Arango.Client;

namespace Commons.ArangoDb
{
	public abstract class ArangoBaseEdgeModel
    {
        [ArangoProperty(Identity=true)]
        public virtual string _Id { get; set; }

        [ArangoProperty(Key=true)]
        public virtual string _Key { get; set; }
        
        [ArangoProperty(From=true)]
        public virtual string _From { get; set; }
        
        [ArangoProperty(To=true)]
        public virtual string _To { get; set; }

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
