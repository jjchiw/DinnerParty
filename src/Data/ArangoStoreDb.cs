/*
 * Created by SharpDevelop.
 * User: JuanJ
 * Date: 6/11/2014
 * Time: 1:15 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Arango.Client;
using Inflector;

namespace Commons.ArangoDb
{
	public class ArangoStoreDb : IArangoStoreDb
    {
        readonly ArangoDatabase _db;
        readonly static Dictionary<Type, IArangoViewIndex> ArangoViewIndexDictionary = new Dictionary<Type, IArangoViewIndex>();
        
        readonly Dictionary<Type, List<dynamic>> _documentsCreated;
        readonly Dictionary<Type, List<dynamic>> _documentsUpdated;
        readonly Dictionary<Type, List<dynamic>> _documentsDeleted;

        readonly Dictionary<Type, List<dynamic>> _edgesCreated;
        readonly Dictionary<Type, List<dynamic>> _edgesUpdated;
        readonly Dictionary<Type, List<dynamic>> _edgesDeleted;

        public ArangoStoreDb(string alias)
        {
            _db = new ArangoDatabase(alias);
            EnsureCollectionsExists();
            EnsureEdgeCollectionsExists();
            
            _documentsCreated = new Dictionary<Type, List<dynamic>>();
            _documentsUpdated = new Dictionary<Type, List<dynamic>>();
            _documentsDeleted = new Dictionary<Type, List<dynamic>>();

            _edgesCreated = new Dictionary<Type, List<dynamic>>();
            _edgesUpdated = new Dictionary<Type, List<dynamic>>();
            _edgesDeleted = new Dictionary<Type, List<dynamic>>();
        }
        
        public IEnumerable<T> GetDocumentsCreated<T>() where T : class
        {
        	return GetDocuments<T>(_documentsCreated);
        }
        
        public IEnumerable<T> GetDocumentsUpdated<T>() where T : class
        {
        	return GetDocuments<T>(_documentsUpdated);
        }
        
        public IEnumerable<T> GetDocumentsDeleted<T>() where T : class
        {
        	return GetDocuments<T>(_documentsDeleted);
        }

        public IEnumerable<T> GetEdgesCreated<T>() where T : class
        {
            return GetDocuments<T>(_edgesCreated);
        }

        public IEnumerable<T> GetEdgesUpdated<T>() where T : class
        {
            return GetDocuments<T>(_edgesUpdated);
        }

        public IEnumerable<T> GetEdgesDeleted<T>() where T : class
        {
            return GetDocuments<T>(_edgesDeleted);
        }

        #region Documents

        public int Count<T>(ArangoQueryOperation filterOperation = null, string forItemName = "item")
        {
            return InnerCount(GetCollectionName<T>(), filterOperation, forItemName);
        }

        public List<T> Query<T>(ArangoQueryOperation filterOperation, ArangoQueryOperation sortOperation = null, ArangoQueryOperation limitOperation = null, string forItemName = "item")
        {
            return InnerQuery<T>(GetCollectionName<T>(), filterOperation, sortOperation, limitOperation, forItemName);
        }

        public List<T> Query<T, TIndex>(ArangoQueryOperation filterOperation, ArangoQueryOperation sortOperation = null, ArangoQueryOperation limitOperation = null, string forItemName = "item") where TIndex : IArangoViewIndex
        {
            return InnerQuery<T, TIndex>(filterOperation, sortOperation, limitOperation, forItemName);
        }

        public T Single<T>(ArangoQueryOperation filterOperation, string forItemName = "item")
        {
            return InnerSingle<T>(GetCollectionName<T>(), filterOperation, forItemName);
        }

        public T Single<T, TIndex>(ArangoQueryOperation filterOperation, string forItemName = "item") where TIndex : IArangoViewIndex
        {
            return InnserSingle<T, TIndex>(filterOperation, forItemName);
        }

        public T Get<T>(string id) where T : class, new()
        {
            var searchId = id;
            if (!searchId.Contains("/"))
                searchId = BuildDocumentId<T>(searchId);

            return _db.Document.Get<T>(searchId);
        }

        public void Create<T>(T genericObject, bool waitForSync = false, bool createCollection = false) where T : class
        {
            Fire(BeforeItemAdded, item: genericObject);

            _db.Document.Create<T>(GetCollectionName<T>(), genericObject, waitForSync, createCollection);
            AddToDictionary<T>(_documentsCreated, genericObject);

            Fire(ItemAdded, item: genericObject);
        }

        public bool Delete<T>(string id) where T : class, new()
        {
            var objectToDelete = Get<T>(id);

            Fire(BeforeItemRemoved, item: objectToDelete);

            var deleteId = id;
            if (!deleteId.Contains("/"))
                deleteId = BuildDocumentId<T>(deleteId);

            if (!_db.Document.Delete(deleteId))
            {
                return false;
            }

            AddToDictionary<T>(_documentsDeleted, objectToDelete);

            Fire(ItemRemoved, item: objectToDelete);

            return true;
        }

        public bool Update<T>(T genericObject, bool waitForSync = false, string revision = null) where T : class, new()
        {
            Fire(BeforeItemUpdated, item: genericObject);

            if (!_db.Document.Update<T>(genericObject, waitForSync, revision))
            {
                return false;
            }

            AddToDictionary<T>(_documentsUpdated, genericObject);

            Fire(ItemUpdated, item: genericObject);

            return true;
        }

        #endregion

        #region Edges
        public int CountEdge<T>(ArangoQueryOperation filterOperation = null, string forItemName = "item")
        {
            return InnerCount(GetEdgeCollectionName<T>(), filterOperation, forItemName);
        }

        public List<T> QueryEdge<T>(ArangoQueryOperation filterOperation, ArangoQueryOperation sortOperation = null, ArangoQueryOperation limitOperation = null, string forItemName = "item")
        {
            return InnerQuery<T>(GetEdgeCollectionName<T>(), filterOperation, sortOperation, limitOperation, forItemName);
        }

        public List<T> QueryEdge<T, TIndex>(ArangoQueryOperation filterOperation, ArangoQueryOperation sortOperation = null, ArangoQueryOperation limitOperation = null, string forItemName = "item") where TIndex : IArangoViewIndex
        {
            return InnerQuery<T, TIndex>(filterOperation, sortOperation, limitOperation, forItemName);
        }

        public T SingleEdge<T>(ArangoQueryOperation filterOperation, string forItemName = "item")
        {
            return InnerSingle<T>(GetEdgeCollectionName<T>(), filterOperation, forItemName);
        }

        public T SingleEdge<T, TIndex>(ArangoQueryOperation filterOperation, string forItemName = "item") where TIndex : IArangoViewIndex
        {
            return InnserSingle<T, TIndex>(filterOperation, forItemName);
        }

        public T GetEdge<T>(string id) where T : class, new()
        {
            var searchId = id;
            if (!searchId.Contains("/"))
                searchId = BuildDocumentId<T>(searchId);

            return _db.Edge.Get<T>(searchId);
        }

        public void CreateEdge<T>(T genericObject, bool waitForSync = false, bool createCollection = false) where T : class
        {
            Fire(BeforeItemAdded, item: genericObject);

            _db.Edge.Create<T>(GetEdgeCollectionName<T>(), genericObject, waitForSync, createCollection);
            AddToDictionary<T>(_edgesCreated, genericObject);

            Fire(ItemAdded, item: genericObject);
        }

        public bool DeleteEdge<T>(string id) where T : class, new()
        {
            var objectToDelete = GetEdge<T>(id);

            Fire(BeforeItemRemoved, item: objectToDelete);

            var deleteId = id;
            if (!deleteId.Contains("/"))
                deleteId = BuildDocumentId<T>(deleteId);

            if (!_db.Edge.Delete(deleteId))
            {
                return false;
            }

            AddToDictionary<T>(_edgesDeleted, objectToDelete);

            Fire(ItemRemoved, item: objectToDelete);

            return true;
        } 
        #endregion
        
		protected virtual void Fire(EventHandler<ArangoStoreEventArgs> @event, dynamic item)
		{
			if (@event != null) {
				var args = new ArangoStoreEventArgs { Item = item };
				@event(this, args);
			}
		}

        IEnumerable<T> GetDocuments<T>(IDictionary<Type, List<dynamic>> documents)  where T : class
        {
        	List<dynamic> list = null;
        	
        	if(!documents.TryGetValue(typeof(T), out list)) {
        		yield return null;
        	}
        	
        	foreach (var item in list) {
        		yield return item as T;
        	}
        }

        void EnsureCollectionsExists()
        {
            var type = typeof(ArangoBaseModel);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
                .ToList();


            foreach (var t in types)
            {
                var collectionName = GetCollectionName(t);

                var collection = _db.Collection.Get(collectionName);
                if (collection == null)
                {
                    collection = new ArangoCollection();
                    collection.Name = collectionName;
                    collection.Type = ArangoCollectionType.Document;
                    _db.Collection.Create(collection);
                }
            }
        }
        
        void EnsureEdgeCollectionsExists()
        {
            var type = typeof(ArangoBaseEdgeModel);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
                .ToList();


            foreach (var t in types)
            {
                var collectionName = GetEdgeCollectionName(t);
      
                var collection = _db.Collection.Get(collectionName);
                if (collection == null)
                {
                    collection = new ArangoCollection();
                    collection.Name = collectionName;
                    collection.Type = ArangoCollectionType.Edge;
                    _db.Collection.Create(collection);
                }
            }
        }
        
        void AddToDictionary<T>(IDictionary<Type, List<dynamic>> documents, T genericObject)
		{
			List<dynamic> list = null;
			if (!documents.TryGetValue(typeof(T), out list)) {
				list = new List<dynamic>();
                documents.Add(typeof(T), list);
			}
			list.Add(genericObject);
		}

        int InnerCount(string collectionName, ArangoQueryOperation filterOperation, string forItemName)
        {
            ArangoQueryOperation expression = new ArangoQueryOperation()
           .Aql(_ => _.LET("counter")
                     .FOR(forItemName));

            if (filterOperation == null)
                expression.Aql(_ => _.IN(collectionName, _.RETURN.Val(1)));
            else
                expression.Aql(_ => _.IN(collectionName, filterOperation.RETURN.Val(1)));

            expression.Aql(_ => _.RETURN.LENGTH(_.Var("counter")));

            return _db.Query.Aql(expression.ToString()).ToObject<int>();
        }

        List<T> InnerQuery<T>(string collectionName, ArangoQueryOperation filterOperation, ArangoQueryOperation sortOperation, ArangoQueryOperation limitOperation, string forItemName)
        {
            var expression = new ArangoQueryOperation();
            expression.Aql(_ => _.FOR(forItemName)
                     .IN(collectionName, filterOperation)
                     .RETURN.Var(forItemName)
            );

            if (sortOperation != null)
                expression.Aql(_ => sortOperation);

            if (limitOperation != null)
                expression.Aql(_ => limitOperation);

            return _db.Query.Aql(expression.ToString()).ToList<T>();
        }

        List<T> InnerQuery<T, TIndex>(ArangoQueryOperation filterOperation, ArangoQueryOperation sortOperation, ArangoQueryOperation limitOperation, string forItemName) where TIndex : IArangoViewIndex
        {
            IArangoViewIndex index;
            if (!ArangoViewIndexDictionary.TryGetValue(typeof(TIndex), out index))
                index = Activator.CreateInstance<TIndex>();

            var expression = index.Execute(filterOperation, forItemName: forItemName);

            if (sortOperation != null)
                expression.Aql(_ => sortOperation);

            if (limitOperation != null)
                expression.Aql(_ => limitOperation);

            expression.Aql(_ => _.RETURN.Var("returnObj"));


            return _db.Query.Aql(expression.ToString()).ToList<T>();
        }

        T InnserSingle<T, TIndex>(ArangoQueryOperation filterOperation, string forItemName) where TIndex : IArangoViewIndex
        {
            IArangoViewIndex index;
            if (!ArangoViewIndexDictionary.TryGetValue(typeof(TIndex), out index))
                index = Activator.CreateInstance<TIndex>();

            var expression = index.Execute(filterOperation, forItemName: forItemName);

            expression.Aql(_ => _.RETURN.Var("returnObj"));

            return _db.Query.Aql(expression.ToString()).ToObject<T>();
        }

        T InnerSingle<T>(string collectionName, ArangoQueryOperation filterOperation, string forItemName)
        {
            var expression = new ArangoQueryOperation();
            expression.Aql(_ => _.FOR(forItemName)
                     .IN(collectionName, filterOperation)
                     .RETURN.Var(forItemName)
            );

            return _db.Query.Aql(expression.ToString()).ToObject<T>();
        }


		public event EventHandler<ArangoStoreEventArgs> BeforeItemRemoved;
		public event EventHandler<ArangoStoreEventArgs> BeforeItemAdded;
		public event EventHandler<ArangoStoreEventArgs> BeforeItemUpdated;
		public event EventHandler<ArangoStoreEventArgs> ItemRemoved;
		public event EventHandler<ArangoStoreEventArgs> ItemAdded;
		public event EventHandler<ArangoStoreEventArgs> ItemUpdated;        
        
        public static string GetCollectionName(Type t)
        {
            return t.Name.Pluralize();
        }

        public static string GetCollectionName<T>()
        {
            return GetCollectionName(typeof(T));
        }
        
        public static string GetEdgeCollectionName(Type t)
        {
            return t.Name.Pluralize()+"Edge";
        }

        public static string GetEdgeCollectionName<T>()
        {
            return GetEdgeCollectionName(typeof(T));
        }

        public static string BuildDocumentId<T>(long id)
        {
            return BuildDocumentId(typeof(T), id.ToString());
        }

        public static string BuildDocumentId(Type t, long id)
        {
            return string.Format("{0}/{1}", GetCollectionName(t), id);
        }

        public static string BuildDocumentId<T>(string id)
        {
            return BuildDocumentId(typeof(T), id);
        }

        public static string BuildDocumentId(Type t, string id)
        {
            return string.Format("{0}/{1}", GetCollectionName(t), id);
        }
    }
}
