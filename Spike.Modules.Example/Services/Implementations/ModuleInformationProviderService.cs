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
            get => ModuleApiConstants.Areas.Module.Name;
        }
        public string Description { get => "..."; }

        public string RestRoutePrefix
        {
            get => ModuleApiConstants.Areas.Module.Rest.V1.Routing.RoutePrefix;
        }


        public string RestODataRoutePrefix
        {
            get => ModuleApiConstants.Areas.Module.OData.V1.Routing.RoutePrefix;
        }

        public string[] Permissions
        {
            get
            {
                return new string[]{
                ModuleApiConstants.Areas.Module.Permissions.StudentsSummarise,
                ModuleApiConstants.Areas.Module.Permissions.StudentsRead,
                ModuleApiConstants.Areas.Module.Permissions.StudentsWrite,
                ModuleApiConstants.Areas.Module.Permissions.StudentsDelete
                };
            }

}

    }
}
