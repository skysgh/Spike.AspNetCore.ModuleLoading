﻿using Autofac;
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

            ILifetimeScope scope =
                RegisterDependenciesInDIScope(loadContext, assembly);

            //scope.s.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", GetEdmModel()));

            // Notify change so that next request
            // knows about new Controllers:
            AppActionDescriptorChangeProvider.Instance.Reset();
            // TODO: This turns off/on the whole server.
            // _applicationLifetime.StopApplication();

            return assembly;
        }



        private ILifetimeScope RegisterDependenciesInDIScope(AppModuleLoadContext context, Assembly assembly)
        {
            ContainerBuilder autoFacContainerBuilderUsed = null;

            List<Type> serviceTypes;
            List<Type> controllerTypes = new List<Type>();
            List<Type> oDataControllerTypes;

            // TODO: Primitive/improvable way to look for Services
            var assemblyExportedTypes = assembly.ExportedTypes;

            // Now go through Types:
            ILifetimeScope moduleLifetimeScope =
                _lifetimeScope.BeginLifetimeScope(autoFacContainerBuilder =>
                {
                    //Save a copy for after we leave scope:
                    autoFacContainerBuilderUsed
                      = autoFacContainerBuilder;

                    //Even though you have a copy of it, saved, 
                    // you have to add new services and controllers to 
                    // it before you leave...


                    serviceTypes =
                        RegisterNewServicesInNewDIScope(
                        autoFacContainerBuilderUsed,
                        assemblyExportedTypes);

                    controllerTypes.AddRange(
                        RegisterControllersInNewDIScope(
                           autoFacContainerBuilderUsed,
                           assemblyExportedTypes));
                    // Routes and Services registered...but now
                    // the magic of EDM and OData...which I don't
                    // know enough port what's in the ExtensionMethods
                    // to this...
                    oDataControllerTypes =
                        RegisterODataControllerTypes(controllerTypes);

                });

            //think it's ok to register this outside the scope
            //as I need Module services at this point.
            RegisterODataEDMModel(assemblyExportedTypes, moduleLifetimeScope);

            // Save for later (it's how info is shared with
            // our custom MyServiceBasedControllerActivator
            ControllerToScopeDictionaryEntry controllerTypeScopeInfo = new ControllerToScopeDictionaryEntry()
            {
                Context = context,
                Assembly = assembly,
                Scope = moduleLifetimeScope
            };


            SaveControllerTypeAgainstDIScopeForLaterUse(controllerTypes, controllerTypeScopeInfo);

            // Return the Scope being created
            return moduleLifetimeScope;
        }

        private static List<Type> RegisterNewServicesInNewDIScope(ContainerBuilder autofacContainerBuilder, IEnumerable<Type> assemblyTypes)
        {
            List<Type> results = new List<Type>();

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
                // If ONLY WE COULD DO THIS NOW!!!!
                //_serviceCollection.AddSingleton(tInterface, type);
                //instead, add to builder:
                autofacContainerBuilder.RegisterType(serviceType).As(tInterface);
                results.Add(tInterface);
            }
            return results;
        }
        private static List<Type> RegisterControllersInNewDIScope(ContainerBuilder autoFacContainerBuilderUsed, IEnumerable<Type> assemblyTypes)
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
                    autoFacContainerBuilderUsed.RegisterType(controllerType);
                }
            }
            return results;
        }

        private static List<Type> RegisterODataControllerTypes(IEnumerable<Type> controllerInstances)
        {
            List<Type> results = new List<Type>();

            //Search for and register controllers:
            foreach (var controllerType in controllerInstances)
            {
                if (typeof(ODataController).IsAssignableFrom(controllerType))
                {
                    // Save for using next...                            
                    results.Add(controllerType);
                }
            }

            // Don't know how to do it so late:
            ////
            //// Add services to the container.
            //containerBuilder.Services
            //    .AddControllersWithViews()
            //    //no!!!! Fails for Modules
            //    .AddControllersAsServices()
            //    //But OData works as always:
            //    .AddOData(
            //                    opt =>
            //                    opt.Count()
            //                    .Filter()
            //                    .Expand()
            //                    .Select()
            //                    .OrderBy()
            //                    .SetMaxTop(5)
            //                    //Add Module/PluginA Routes:
            //                    .AddRouteComponents(
            //                        AppAPIConstants.Areas.Base.OData.V1.Routing.ODataPrefix,
            //                        AppModuleBaseEdmModelBuilder.BuildModel())
            //                    // But we won't add the next model, until 
            //                    // it comes online when the Module is loaded
            //                    //Add Module/PluginB Routes:
            //                    //.AddRouteComponents(
            //                    //     AppAPIConstants.Areas.ModuleB.OData.V1.Routing.ODataPrefix,
            //                    //    AppModuleBEdmModelBuilder.BuildModel())
            //                    //Uses AttributeRoutingConvention:
            //                    .EnableAttributeRouting = true
            //    );

            return results;
        }

        private void RegisterODataEDMModel(IEnumerable<Type> assemblyExportedTypes, ILifetimeScope lifetimeScope)
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
            // Now register the sucker.

            var odataOptionsCheck = _serviceProvider.GetService<ODataOptions>();

            var firstEntry = odataOptionsCheck.RouteComponents.First().Value;
            IServiceProvider odataServiceProvider = firstEntry.ServiceProvider;


            //Notice we want the Modules ServiceProvider, *not* the Base one.
            
            var moduleInformationProviderService = lifetimeScope.Resolve<IModuleInformationProviderService>();

            // will need to not hard coded later:
            string serviceRoute = moduleInformationProviderService?.RestODataRoutePrefix;

            odataOptionsCheck
                .RouteComponents[serviceRoute] = 
                (edmModel, odataServiceProvider);

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
