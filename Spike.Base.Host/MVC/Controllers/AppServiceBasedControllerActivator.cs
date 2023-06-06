using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Autofac;
using App.Base.DI;

namespace App.Base.MVC.Controllers
{
    /// <summary>
    /// Builder to build Controllers.
    /// <para>
    /// We replace the default solution because of late loaded
    /// Controllers that come in via Modules.
    /// </para>
    /// <para>
    /// They may have require services that do 
    /// not exist in Base
    /// but only a loaded Module.
    /// </para>
    /// <para>
    /// So the default Services can't solve/build the controller.
    /// </para>
    /// <para>
    /// In which case we use the ControllerType to guide us back 
    /// to a DI sub scope that was used to register Module
    /// Services and Controllers
    /// </para>
    /// </summary>
    /// <remarks>
    /// See: https://andrewlock.net/controller-activation-and-dependency-injection-in-asp-net-core-mvc/
    /// </remarks>
    public class AppServiceBasedControllerActivator : IControllerActivator
    {
        private readonly IServiceProvider _serviceProvider;

        public AppServiceBasedControllerActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        //DefaultHttpControllerActivator
        public object Create(ControllerContext actionContext)
        {
            var controllerType =
                actionContext
                .ActionDescriptor
                .ControllerTypeInfo
                .AsType();

            // Try instantiating the Controller
            // using the default DI solution first:
            var httpController =
                actionContext
                .HttpContext
                .RequestServices
                .GetService(controllerType);

            if (httpController != null)
            {
                return httpController;
            }
            // Try finding the DI scope associated to the 
            // Controller:
            ControllerToScopeDictionaryEntry scopeDictionaryEntry;
            ControllerTypeToScopeDictionary.Instance.TryGetValue(controllerType, out scopeDictionaryEntry);
            if (scopeDictionaryEntry == null)
            {
                return null;
            }
            IServiceProvider moduleServiceProvider = scopeDictionaryEntry.ServiceProvider;

            IServiceScope scope = moduleServiceProvider.CreateScope();
            IServiceProvider serviceProvider = scope.ServiceProvider;
            httpController = serviceProvider.GetService(controllerType);
            actionContext.HttpContext.Items["MODULESCOPE"] = scope;
            return httpController;
        }
    
        public virtual void Release(ControllerContext context, object controller)
        {
            IServiceScope scope = context.HttpContext.Items["MODULESCOPE"] as IServiceScope;
            if (scope != null)
            {
                scope.Dispose();

                context.HttpContext.Items.Remove("MODULESCOPE");

               ;
            }
            scope = null;
            //GC.Collect();
            
            //long mem = GC.GetTotalMemory(false);
            //long pinnedObjects = GC.GetGCMemoryInfo().PinnedObjectsCount;

            //Console.WriteLine($"Memorybytes: {mem}, Pinned: {pinnedObjects}");
            // Not sure what to put here yet.

        }
    }
}
