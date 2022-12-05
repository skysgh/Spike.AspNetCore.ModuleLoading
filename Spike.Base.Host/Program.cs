using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using App.Base.Shared.Services;
using App.Base.Shared.Services.Implementations;
using System.Reflection;
using Autofac;
using Grace.AspNetCore.Hosting;
using Grace.DependencyInjection;
using Autofac.Core;
using Microsoft.AspNetCore.Mvc.Controllers;
using App.Base.Host.Services;
using App.Base.Host.Services.Implementations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.OData;
using App.Base.API.OData.ModelBuilders;
using App.Base.API;
using App.Base.MVC.Infrastructure;
using App.Base.MVC.Controllers;
using ICSharpCode.Decompiler.IL;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.EntityFrameworkCore;
using App.Base.Data.Storage.Db.EF;

namespace App.ModuleLoadingAndDI
{
    public class Program
    {

        public static ODataOptions HoldOptions=null;

        private static CancellationTokenSource 
            cancelTokenSource = 
            new System.Threading.CancellationTokenSource();

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // IMPORTANT:
            // Replacing default DI service with Autofac
            // as it permits child scopes...which is necessary
            // to manage new services imported from 3rd party
            // modules.
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            // Demo of registering at startup some service needed by controllers:
            builder.Services.AddSingleton<IExampleHService, ExampleHService>();

            RegisterServicesNeededToImport3rdPartyModulesAndTheirControllersAndServices(builder);



            builder.Services
                .AddControllersWithViews()
                //Force this on to ensure DI works after modules are loaded
                .AddControllersAsServices()
                //But OData works as always:
                .AddOData(
                                opt =>
                                {
                                    //Note. Not called until MapControllerRoute is invoked later down the page.

                                    HoldOptions = opt;

                                    opt.Count()
                                        .Filter()
                                        .Expand()
                                        .Select()
                                        .OrderBy()
                                        .SetMaxTop(5)
                                        //Add Base EDM:
                                        .AddRouteComponents(
                                            AppAPIConstants.Areas.Base.OData.V1.Routing.RoutePrefix,
                                            new AppModuleBaseEdmModelBuilder().BuildModel())
                                        // This is on by default, but still...
                                        .EnableAttributeRouting = true;
                                }
                );

            //Make it easily available later(ie when uploading module):


            AddReplacementFactoryOfControllersThatIsAwareOfChildDIScopes(builder);

            // Wire up custom Resetter invoked by upload controller.
            AddActionDescriptorChangeProvider(builder);

            builder.Services.AddSingleton<ODataOptions>((x)=>HoldOptions);


            // =======================================================
            // TODO: Have not found way to replicate this for modules.
            // =======================================================

            string connectionString = builder.Configuration.GetConnectionString("DefaultSqlServer");

             builder.Services.AddDbContext<AppDbContext>(
                x =>
                {
                    x.EnableSensitiveDataLogging(true);
                    x.UseSqlServer(connectionString);
                });

            // That was the last chance to add anthing before Build is called:
            // =======================================================
            // =======================================================
            var app = builder.Build();
            // =======================================================
            // =======================================================



            // IE: This won't work now:
            // builder.Services.AddSingleton<ILateService, LateService>();
            // Returns null:
            var x2 = app.Services.GetService<ILateService>();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment()){app.UseHsts();}
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();


            app.UseAuthorization();

            // Register default route for WebAPI controllers :
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");
            
            // Note that OData Routes were registered earlier
            // than build(), as Config information.
            // NOTE: Maybe that that will be the way to register them later...
            // Either way, add other odata specific handling to debug:
            // Provide the ~/$odata route:
            app.UseODataRouteDebug();
            app.UseODataQueryRequest();

            // This page will load the Angular page:
            app.MapFallbackToFile("index.html");
            

            //Check to see that we will be able to get this later
            //in implementation of IModuleLoadingService:
            var odataOptionsCheck = app.Services.GetService<ODataOptions>();
            var odataServiceProvider = odataOptionsCheck.RouteComponents.First().Value;




            // =======================================================
            // =======================================================
            // Migrate:
            using (var scope = app.Services.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetService<AppDbContext>())
                {
                    //context.Database.EnsureCreated();

                    ////context.Persons.Add(new Data.Storage.Models.ExamplePerson { Id = Guid.NewGuid(), Title = "Something", Description = "Else" });

                    //context.SaveChanges();

                    context.Database.Migrate();
                }
            }
            // =======================================================
            // =======================================================
            app.Run();
        }

        private static void AddReplacementFactoryOfControllersThatIsAwareOfChildDIScopes(WebApplicationBuilder builder)
        {
            builder.Services.Add(ServiceDescriptor.Transient<IControllerActivator, AppServiceBasedControllerActivator>());
        }

        private static void RegisterServicesNeededToImport3rdPartyModulesAndTheirControllersAndServices(WebApplicationBuilder builder)
        {
            // Register two services needed to process the
            // loading of 3rd party modules:
            builder.Services.AddSingleton<IPluginValidationService, PluginValidationService>();
            builder.Services.AddSingleton<IModuleLoadingService, ModuleLoadingService>();
        }

        private static void AddActionDescriptorChangeProvider(WebApplicationBuilder builder)
        {
            //Replace:
            builder.Services.AddSingleton<IActionDescriptorChangeProvider>(AppActionDescriptorChangeProvider.Instance);
            //builder.Services.AddSingleton(AppActionDescriptorChangeProvider.Instance);
        }

    }
}