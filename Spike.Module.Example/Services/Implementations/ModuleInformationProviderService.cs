using App.Base.Shared.Services;
using App.Modules.Example.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Modules.Example.Services.Implementations
{
    public class ModuleInformationProviderService : IModuleInformationProviderService
    {
        public string Name
        {
            get => ApiConstants.Areas.Module.Name;
        }
        public string Description { get => "..."; }

        public string RestRoutePrefix
        {
            get => ApiConstants.Areas.Module.Rest.V1.Routing.RoutePrefix;
        }


        public string RestODataRoutePrefix
        {
            get => ApiConstants.Areas.Module.OData.V1.Routing.RoutePrefix;
        }
    }
}
