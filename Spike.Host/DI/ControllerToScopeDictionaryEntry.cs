using Autofac;
using App.Base.Host.AppDomains;
using System.Reflection;

namespace App.Base.DI
{
    public class ControllerToScopeDictionaryEntry
    {
        // The Assembly context (for dropping Modules later?)
        public AppModuleLoadContext Context { get; set; }
        // The Autofac scope that is used by
        // AppServiceBasedControllerActivator
        // to instantiate a new Controller.
        public ILifetimeScope Scope { get; set; }

        // The Assembly containing the Controller Type
        public Assembly Assembly { get; set; }
    }
}
