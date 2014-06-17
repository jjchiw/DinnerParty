/*
 * Created by SharpDevelop.
 * User: JuanJ
 * Date: 6/9/2014
 * Time: 7:09 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Arango.Client;

namespace Commons.ArangoDb
{
	public interface IArangoStoreDb
	{
		IEnumerable<T> GetDocumentsCreated<T>() where T : class;
		IEnumerable<T> GetDocumentsUpdated<T>() where T : class;
        IEnumerable<T> GetDocumentsDeleted<T>() where T : class;

        #region Documents
        int Count<T>(ArangoQueryOperation filterOperation = null, string forItemName = "item");
        List<T> Query<T>(ArangoQueryOperation filterOperation,
                                ArangoQueryOperation sortOperation = null,
                                ArangoQueryOperation limitOperation = null,
                                string forItemName = "item");
        List<T> Query<T, TIndex>(ArangoQueryOperation filterOperation,
                                        ArangoQueryOperation sortOperation = null,
                                        ArangoQueryOperation limitOperation = null,
                                        string forItemName = "item") where TIndex : IArangoViewIndex;
        T Single<T>(ArangoQueryOperation filterOperation, string forItemName = "item");
        T Single<T, TIndex>(ArangoQueryOperation filterOperation, string forItemName = "item") where TIndex : IArangoViewIndex;
        T Get<T>(string id) where T : class, new();
        void Create<T>(T genericObject, bool waitForSync = false, bool createCollection = false) where T : class;
        bool Delete<T>(string id) where T : class, new();
        bool Update<T>(T genericObject, bool waitForSync = false, string revision = null) where T : class, new();

        #endregion

        #region edges
        int CountEdge<T>(ArangoQueryOperation filterOperation = null, string forItemName = "item");
        List<T> QueryEdge<T>(ArangoQueryOperation filterOperation,
                                ArangoQueryOperation sortOperation = null,
                                ArangoQueryOperation limitOperation = null,
                                string forItemName = "item");
        List<T> QueryEdge<T, TIndex>(ArangoQueryOperation filterOperation,
                                        ArangoQueryOperation sortOperation = null,
                                        ArangoQueryOperation limitOperation = null,
                                        string forItemName = "item") where TIndex : IArangoViewIndex;
        T SingleEdge<T>(ArangoQueryOperation filterOperation, string forItemName = "item");
        T SingleEdge<T, TIndex>(ArangoQueryOperation filterOperation, string forItemName = "item") where TIndex : IArangoViewIndex;
        T GetEdge<T>(string id) where T : class, new();
        void CreateEdge<T>(T genericObject, bool waitForSync = false, bool createCollection = false) where T : class;
        bool DeleteEdge<T>(string id) where T : class, new(); 
        #endregion
		
		event EventHandler<ArangoStoreEventArgs> BeforeItemRemoved;
        event EventHandler<ArangoStoreEventArgs> BeforeItemAdded;
        event EventHandler<ArangoStoreEventArgs> BeforeItemUpdated;
        
		event EventHandler<ArangoStoreEventArgs> ItemRemoved;
        event EventHandler<ArangoStoreEventArgs> ItemAdded;
        event EventHandler<ArangoStoreEventArgs> ItemUpdated;
	}
}
