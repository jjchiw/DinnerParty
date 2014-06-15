using Arango.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Inflector;

namespace DinnerParty.Data
{
    public class ArangoStore
    {
        readonly ArangoDatabase _db;
        readonly static Dictionary<Type, IArangoViewIndex> ArangoViewIndexDictionary = new Dictionary<Type, IArangoViewIndex>();

        public ArangoStore(string alias)
        {
            _db = new ArangoDatabase(alias);
            EnsureCollectionsExists();
        }

        private void EnsureCollectionsExists()
        {
            var type = typeof(ArangoModelBase);
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

        public int Count<T>(ArangoQueryOperation filterOperation = null, string forItemName = "item")
        {
            ArangoQueryOperation expression = new ArangoQueryOperation()
           .Aql(_ => _.LET("counter")
                     .FOR(forItemName));

            if (filterOperation == null)
                expression.Aql(_ => _.IN(GetCollectionName<T>(), _.RETURN.Val(1)));
            else
                expression.Aql(_ => _.IN(GetCollectionName<T>(), filterOperation.RETURN.Val(1)));

            expression.Aql(_ => _.RETURN.LENGTH(_.Var("counter")));

            return _db.Query.Aql(expression.ToString()).ToObject<int>();
        }

        public List<T> Query<T>(ArangoQueryOperation filterOperation, 
                                ArangoQueryOperation sortOperation = null,
                                string forItemName = "item")
        {
            var expression = new ArangoQueryOperation();
            expression.Aql(_ => _.FOR(forItemName)
                     .IN(GetCollectionName<T>(), filterOperation)
                     .RETURN.Var(forItemName)
            );

            if (sortOperation != null)
                expression.Aql(_ => sortOperation);

            return _db.Query.Aql(expression.ToString()).ToList<T>();

        }


        public List<T> Query<T, TIndex>(ArangoQueryOperation filterOperation,
                                        ArangoQueryOperation sortOperation = null,
                                        string forItemName = "item") where TIndex : IArangoViewIndex
        {

            IArangoViewIndex index;
            if (!ArangoViewIndexDictionary.TryGetValue(typeof(TIndex), out index))
                index = Activator.CreateInstance<TIndex>();

            var expression = index.Execute(filterOperation, forItemName : forItemName);

            if (sortOperation != null)
                expression.Aql(_ => sortOperation);

            return _db.Query.Aql(expression.ToString()).ToList<T>();
        }

        public T Get<T>(string id) where T : class, new()
        {
            var searchId = id;
            if(!searchId.Contains("/"))
                searchId = BuildDocumentId<T>(searchId);

            return _db.Document.Get<T>(searchId);
        }

        public void Create<T>(T genericObject, bool waitForSync = false, bool createCollection = false)
        {
            _db.Document.Create<T>(GetCollectionName<T>(), genericObject, waitForSync, createCollection);
        }
        
        public bool Delete<T>(string id)
        {
            var Delete = id;
            if (!Delete.Contains("/"))
                Delete = BuildDocumentId<T>(Delete);

            return _db.Document.Delete(Delete);
        }

        public bool Update<T>(T genericObject, bool waitForSync = false, string revision = null)
        {
            return _db.Document.Update<T>(genericObject, waitForSync, revision);
        }


        public static string GetCollectionName(Type t)
        {
            return t.Name.Pluralize();
        }

        public static string GetCollectionName<T>()
        {
            return GetCollectionName(typeof(T));
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