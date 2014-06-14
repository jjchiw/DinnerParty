using Nancy;
using Nancy.Authentication.Forms;
using Nancy.TinyIoc;
using Nancy.Validation.DataAnnotations;
using DinnerParty.Models.CustomAnnotations;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using System;
using DinnerParty.Models;
using System.Linq;
using Arango.Client;
using DinnerParty.Helpers;

namespace DinnerParty
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

#if !DEBUG
            Cassette.Nancy.CassetteNancyStartup.OptimizeOutput = true;
#endif

            EnsureCollectionsExists();

            //DataAnnotationsValidator.RegisterAdapter(typeof(MatchAttribute), (v, d) => new CustomDataAdapter((MatchAttribute) v));

            //var validatorFactory = new DefaultPropertyValidatorFactory(new IDataAnnotationsValidatorAdapter[]{
            //    new CustomDataAdapter()
            //});

            
            



            
            //var docStore = container.Resolve<DocumentStore>("DocStore");

            //CleanUpDB(docStore);

            //Raven.Client.Indexes.IndexCreation.CreateIndexes(typeof(Dinners_Index).Assembly, docStore);

            pipelines.OnError += (context, exception) =>
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(exception);
                return null;
            };
        }

        private static void EnsureCollectionsExists()
        {
            var type = typeof(IModelBase);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
                .ToList();


            foreach (var t in types)
            {
                var db = new ArangoDatabase(DinnerPartyConfiguration.ArangoDbAlias);
                var modelBase = Activator.CreateInstance(t) as IModelBase;

                var collection = db.Collection.Get(modelBase.CollectionName);
                if (collection == null)
                {
                    collection = new ArangoCollection();
                    collection.Name = modelBase.CollectionName;
                    collection.Type = ArangoCollectionType.Document;
                    db.Collection.Create(collection);
                }
            }
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            //var store = new DocumentStore
            //{
            //    ConnectionStringName = "RavenDB1"
            //};

            //store.Initialize();

            var db = new ArangoDatabase(DinnerPartyConfiguration.ArangoDbAlias);
            container.Register<ArangoDatabase>(db);
        }

        protected override void RequestStartup(TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);

            // At request startup we modify the request pipelines to
            // include forms authentication - passing in our now request
            // scoped user name mapper.
            //
            // The pipelines passed in here are specific to this request,
            // so we can add/remove/update items in them as we please.
            var formsAuthConfiguration =
                new FormsAuthenticationConfiguration()
                {
                    RedirectUrl = "~/account/logon",
                    UserMapper = container.Resolve<IUserMapper>(),
                };

            FormsAuthentication.Enable(pipelines, formsAuthConfiguration);
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            container.Register<IUserMapper, UserMapper>();

            //var docStore = container.Resolve<DocumentStore>("DocStore");
            //var documentSession = docStore.OpenSession();

            //container.Register<IDocumentSession>(documentSession);
        }

        protected override void ConfigureConventions(Nancy.Conventions.NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(Nancy.Conventions.StaticContentConventionBuilder.AddDirectory("/", "public"));
        }

        protected override Nancy.Diagnostics.DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration { Password = @"nancy" }; }
        }

//        private void CleanUpDB(DocumentStore documentStore)
//        {
//            var docSession = documentStore.OpenSession();
//            var configInfo = docSession.Load<Config>("DinnerParty/Config");

//            if (configInfo == null)
//            {
//                configInfo = new Config();
//                configInfo.Id = "DinnerParty/Config";
//                configInfo.LastTruncateDate = DateTime.Now.AddHours(-48); //No need to delete data if config doesnt exist but setup ready for next time

//                docSession.Store(configInfo);
//                docSession.SaveChanges();

//                return;
//            }
//            else
//            {
//                if ((DateTime.Now - configInfo.LastTruncateDate).TotalHours < 24)
//                    return;


//                configInfo.LastTruncateDate = DateTime.Now;
//                docSession.SaveChanges();

//                //If database size >15mb or 1000 documents delete documents older than a week

//#if DEBUG
//                var jsonData =
//                    documentStore.JsonRequestFactory.CreateHttpJsonRequest(null, "http://localhost:8080/database/size", "GET", documentStore.Credentials, documentStore.Conventions).ReadResponseJson();
//#else
//                var jsonData =
//                    documentStore.JsonRequestFactory.CreateHttpJsonRequest(null, "https://aeo.ravenhq.com/databases/DinnerParty-DinnerPartyDB/database/size", "GET", documentStore.Credentials, documentStore.Conventions).ReadResponseJson();
//#endif
//                int dbSize = int.Parse(jsonData.SelectToken("DatabaseSize").ToString());
//                long docCount = documentStore.DatabaseCommands.GetStatistics().CountOfDocuments;


//                if (docCount > 1000 || dbSize > 15000000) //its actually 14.3mb but goood enough
//                {

//                    documentStore.DatabaseCommands.DeleteByIndex("Raven/DocumentsByEntityName",
//                                              new IndexQuery
//                                              {
//                                                  Query = docSession.Advanced.LuceneQuery<object>()
//                                                  .WhereEquals("Tag", "Dinners")
//                                                  .AndAlso()
//                                                  .WhereLessThan("LastModified", DateTime.Now.AddDays(-7)).ToString()
//                                              },
//                                              false);
//                }
//            }
//        }
    }
}