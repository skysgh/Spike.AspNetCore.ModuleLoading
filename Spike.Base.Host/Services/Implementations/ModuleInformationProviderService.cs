using App.Base.Shared.Services;

namespace App.Base.Services.Implementations
{
    public class ModuleInformationProviderService : IModuleInformationProviderService
    {
        public string Name
        {
            get => API.AppAPIConstants.Areas.Base.Name;
        }
        public string Description { get => "..."; }

        public string RestRoutePrefix
        {
            get => API.AppAPIConstants.Areas.Base.Rest.V1.Routing.RootPrefix;
        }


        public string RestODataRoutePrefix
        {
            get => API.AppAPIConstants.Areas.Base.OData.V1.Routing.RoutePrefix;
        }
    }
}
