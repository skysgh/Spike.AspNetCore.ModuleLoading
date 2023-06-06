using Autofac;
using Autofac.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using App.Base.Host.AppDomains;
using App.Base.Shared.Services;
using Spike.DotNetCore.ILSpy._01.Services.Configuration;
using System.Reflection;
using App.Base.DI;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using App.Base.MVC.Infrastructure;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using App.Base.API.OData.ModelBuilders;
using Microsoft.OData.ModelBuilder;
using System;
using App.Base.Shared.Services.Implementations;
using App.ModuleLoadingAndDI;
using System.ComponentModel.Design;
using Microsoft.Extensions.DependencyInjection;
using App.Base.Data.Storage.Db.EF;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Autofac.Builder;

namespace App.Base.Host.Services.Implementations
{
    public class ModuleLoadingService : IModuleLoadingService
    {
        private readonly ApplicationPartManager _applicationPartManager;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPluginValidationService _pluginValidationService;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public ControllerTypeToScopeDictionary Scopes
        {
            get
            {
                return ControllerTypeToScopeDictionary.Instance;
            }
        }

        public ModuleLoadingService(
            IServiceProvider serviceProvider,
            IPluginValidationService pluginValidationService,
            IHostApplicationLifetime applicationLifetime,
            ApplicationPartManager applicationPartManager,
            ILifetimeScope lifetimeScope)
        {
            _applicationPartManager = applicationPartManager;
            this._lifetimeScope = lifetimeScope;
            _serviceProvider = serviceProvider;
            this._pluginValidationService = pluginValidationService;
            _applicationLifetime = applicationLifetime;
        }


        public Assembly Load(string assemblyFilePath, string? assemblyResolutionBaseDirectoryPath = null)
        {
            if (string.IsNullOrEmpty(assemblyResolutionBaseDirectoryPath))
            {
                assemblyResolutionBaseDirectoryPath =
                    Path.GetDirectoryName(assemblyFilePath);
            }

            ValidationConstraintConfiguration validationConstraintConfiguration =
                new ValidationConstraintConfiguration();
            validationConstraintConfiguration
                .Excluded
                .Words
                .AddRange(new[] { "Activator", "FileStream", "FileReader", "FileWriter", "File" });

            if (!_pluginValidationService.ValidateAssembly(assemblyFilePath,
                validationConstraintConfiguration))
            {
                return null;
            }


            var loadContext =
                new AppModuleLoadContext(assemblyResolutionBaseDirectoryPath);

            //var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            var assembly = loadContext.LoadFromAssemblyPath(assemblyFilePath);


            if (assembly == null)
            {
                return null;
            }

            //Note that this is not working:
            // As it can't see basic assemblies, 
            // such as ones holding File types....
            // :-(
            // Maybe another AssemblyLoader is required...
            // to iterate over assemblies and then dump?


            _applicationPartManager
                .ApplicationParts
                .Add(new AssemblyPart(assembly));

                RegisterDependenciesInDIScope(loadContext, assembly);

            //scope.s.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", GetEdmModel()));

            // Notify change so that next request
            // knows about new Controllers:
            AppActionDescriptorChangeProvider.Instance.Reset();
            // TODO: This turns off/on the whole server.
            // _applicationLifetime.StopApplication();

            return assembly;
        }



        private void RegisterDependenciesInDIScope(AppModuleLoadContext context, Assembly assembly)
        {
            ContainerBuilder autoFacContainerBuilderUsed = null;


            // TODO: Primitive/improvable way to look for Services
            var assemblyExportedTypes = assembly.ExportedTypes;

            //Gawd I can be soooo dumb.
            var baseServiceCollection = _serviceProvider.GetService<IServiceCollection>();

            var clonedServiceCollection = baseServiceCollection.CloneIntelligently(_serviceProvider);


            RegisterNewServicesInNewDIScope(
               clonedServiceCollection,
               assemblyExportedTypes);

            var controllerTypes =
                RegisterControllersInNewDIScope(
             clonedServiceCollection,
             assemblyExportedTypes);

            RegisterODataEDMModel(clonedServiceCollection, assemblyExportedTypes);


            string connectionString = _serviceProvider.GetService<IConfiguration>().GetConnectionString("DefaultSqlServer");



            Type? moduleDbContextType =
                assemblyExportedTypes
                .Where(x =>
                    typeof(DbContext).IsAssignableFrom(x))
                .FirstOrDefault();

            if (moduleDbContextType != null)
            {
                //ModuleDbContext: DbContext
                Type t = typeof(EntityFrameworkServiceCollectionExtensions);

                MethodInfo extensionMethodMethodInfo = t.GetMethod(
                        name: nameof(EntityFrameworkServiceCollectionExtensions.AddDbContext),
                        genericParameterCount: 1,
                        types: new Type[] {
                                typeof(IServiceCollection),
                                System.Linq.Expressions.Expression.GetActionType(typeof(DbContextOptionsBuilder)),
                                typeof(ServiceLifetime),
                                typeof(ServiceLifetime)
                                    }
                        );

                MethodInfo genericExtensionMethodMethodInfo =
                    extensionMethodMethodInfo.MakeGenericMethod(moduleDbContextType);

                Action<DbContextOptionsBuilder> callback = x =>
                {
                    x.EnableSensitiveDataLogging(true);
                    x.UseSqlServer(connectionString);
                };

                genericExtensionMethodMethodInfo.Invoke(
                    obj: null /*extensionmethods are null*/,
                    parameters: new object[] { clonedServiceCollection, callback, ServiceLifetime.Scoped, ServiceLifetime.Scoped }
                );
            }
            //var sop = new ServiceProviderOptions();
            
            var serviceProvider = 
                clonedServiceCollection.BuildServiceProvider();


            // Save for later (it's how info is shared with
            // our custom MyServiceBasedControllerActivator
            ControllerToScopeDictionaryEntry controllerTypeScopeInfo = new ControllerToScopeDictionaryEntry()
            {
                Context = context,
                Assembly = assembly,
                ServiceCollection = clonedServiceCollection,
                ServiceProvider = serviceProvider
            };


            SaveControllerTypeAgainstDIScopeForLaterUse(controllerTypes, controllerTypeScopeInfo);


            using (var scope = serviceProvider.CreateScope())
            {
                DbContext context2 = serviceProvider.GetService(moduleDbContextType) as DbContext;

                //context.Database.EnsureCreated();

                ////context.Persons.Add(new Data.Storage.Models.ExamplePerson { Id = Guid.NewGuid(), Title = "Something", Description = "Else" });

                //context.SaveChanges();

                context2.Database.Migrate();

            }
        }

        private static void RegisterNewServicesInNewDIScope(IServiceCollection serviceCollection, IEnumerable<Type> assemblyTypes)
        {

            //Search for Services first:
            foreach (var serviceType in assemblyTypes)
            {
                if (serviceType.IsInterface)
                {
                    continue;
                }
                if (!typeof(IModuleService).IsAssignableFrom(serviceType))
                {
                    continue;
                }
                var tInterface = serviceType.GetInterfaces().First();
     
                serviceCollection.AddSingleton(tInterface, serviceType);

            }
        }


  
  
        private static List<Type> RegisterControllersInNewDIScope(IServiceCollection serviceCollection, IEnumerable<Type> assemblyTypes)
        {
            List<Type> results = new List<Type>();

            //Search for and register controllers:
            foreach (var controllerType in assemblyTypes)
            {
                if (typeof(ControllerBase).IsAssignableFrom(controllerType))
                {
                    // Save for using next...                            
                    results.Add(controllerType);

                    // But register the service:
                    serviceCollection.AddTransient(controllerType);
                }
            }
            return results;
        }




        private void RegisterODataEDMModel(IServiceCollection serviceCollection, IEnumerable<Type> assemblyExportedTypes)
        {
            Type? edmModelBuilderType =
                assemblyExportedTypes
                .Where(x =>
                    typeof(IEdmModelBuilder).IsAssignableFrom(x))
                .FirstOrDefault();
            if (edmModelBuilderType == null)
            {
                return;
            }

            var edmModel = 
                ((IEdmModelBuilder)Activator.CreateInstance(edmModelBuilderType))
                .BuildModel();

            var routePrefix = Base.API.AppAPIConstants.Areas.Base.OData.V1.Routing.RoutePrefix.Replace("Base", "Module");

            serviceCollection
             .AddControllersWithViews()
             //Force this on to ensure DI works after modules are loaded
             .AddControllersAsServices()
             //But OData works as always:
             .AddOData(
                             opt =>
                             {
                                 //Note. Not called until MapControllerRoute is invoked later down the page.


                                 opt.Count()
                                     .Filter()
                                     .Expand()
                                     .Select()
                                     .OrderBy()
                                     .SetMaxTop(5)
                                     //Add Base EDM:
                                     .AddRouteComponents(
                                         routePrefix,
                                         edmModel)
                                     // This is on by default, but still...
                                     .EnableAttributeRouting = true;
                             }
             );

        }

        private static void SaveControllerTypeAgainstDIScopeForLaterUse(List<Type> controllerTypes, ControllerToScopeDictionaryEntry controllerTypeScopeInfo)
        {
            // This is the info that is shared with
            // our custom MyServiceBasedControllerActivator
            foreach (Type controllerType in controllerTypes)
            {
                // Save the scope so that it doesn't get Disposed.
                // We'll try to use it again from a Controller creator.
                ControllerTypeToScopeDictionary.Instance[controllerType] = controllerTypeScopeInfo;
            }
        }

    }
}
