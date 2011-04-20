﻿using System.Linq;
using System.Web.Routing;
using FubuCore;
using FubuFastPack.NHibernate;
using FubuFastPack.StructureMap;
using FubuMusicStore.Actions.Home;
using FubuMVC.Core;
using FubuMVC.Core.Configuration;
using FubuMVC.Core.Diagnostics;
using FubuMVC.Core.Packaging;
using FubuMVC.Core.Registration.Conventions;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.Routes;
using FubuMVC.Core.Urls;
using FubuMVC.StructureMap;
using NHibernate.Dialect;
using NHibernate.Driver;
using StructureMap;

namespace FubuMusicStore
{
    public class FubuMusicStoreRegistry : FubuRegistry
    {
        public FubuMusicStoreRegistry()
        {
            IncludeDiagnostics(true);

            Actions.IncludeTypesNamed(x => x.EndsWith("Action"));

            Routes
                .IgnoreNamespaceText("FubuMusicStore.Actions")
                .IgnoreControllerNamesEntirely()
                .IgnoreClassSuffix("Action")
                .IgnoreMethodsNamed("Execute")
                .IgnoreMethodSuffix("Post")
                .IgnoreMethodSuffix("Get")
                .IgnoreMethodSuffix("Delete")
                .IgnoreMethodSuffix("Put")
                .ConstrainToHttpMethod(action => action.Method.Name.Equals("Post"), "POST")
                .ConstrainToHttpMethod(action => action.Method.Name.Equals("Put"), "PUT")
                .ConstrainToHttpMethod(action => action.Method.Name.Equals("Get"), "GET")
                .ConstrainToHttpMethod(action => action.Method.Name.Equals("Delete"), "DELETE");
                //.ForInputTypesOf<IRequestById>(x => x.RouteInputFor(request => request.Id));
            
            Routes.HomeIs<HomeAction>(x => x.Get(null));

            Views.TryToAttachWithDefaultConventions();
        }
    }

    public static class DatabaseDriver
    {
        private static IContainer _container;
        private static DatabaseSettings _settings;
        private static string FILE_NAME;

        static DatabaseDriver()
        {
            FILE_NAME = FileSystem.Combine(FubuMvcPackageFacility.GetApplicationPath(), "chinook.db");
        }
        public static void Bootstrap(bool cleanFile)
        {
            if (_container != null) return;

            _container = BootstrapContainer();
        }

        public static IContainer BootstrapContainer()
        {
          
            return new Container(x =>
            {
                x.AddRegistry(new FastPackRegistry());
                _settings = new DatabaseSettings()
                {
                    ConnectionString = "Data Source={0};Version=3;".ToFormat(FILE_NAME),
                    DialectType = typeof(SQLiteDialect),
                    ProxyFactory = "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle",
                    ShowSql = true,
                    DriverType = typeof(SQLite20Driver)
                };
                x.For<DatabaseSettings>().Use(_settings);
                x.BootstrapNHibernate<FubuMusicStoreNHibernateRegistry>(ConfigurationBehavior.AlwaysUseNewConfiguration);
                x.UseExplicitNHibernateTransactionBoundary();
            });
        }

        public static IContainer GetFullFastPackContainer()
        {
            _container = BootstrapContainer();
            _container.Configure(x =>
            {
                x.AddRegistry(new FastPackRegistry());
                x.For<IObjectConverter>().Use<ObjectConverter>();
            });


            FubuApplication.For<FubuMusicStoreRegistry>().StructureMap(() => _container).Bootstrap();

            _container.GetInstance<ISchemaWriter>().BuildSchema();

            return _container;
        }

        public static IContainer BuildWebsiteContainer()
        {
            Bootstrap(true);

            return _container;
        }

        public static IContainer ContainerWithoutDatabase()
        {
            return _container.GetNestedContainer();
        }

        public static IContainer ContainerWithDatabase()
        {
            var nested = _container.GetNestedContainer();

            var writer = nested.GetInstance<ISchemaWriter>();
            writer.BuildSchema();

            return nested;
        }

        
    }

    
}