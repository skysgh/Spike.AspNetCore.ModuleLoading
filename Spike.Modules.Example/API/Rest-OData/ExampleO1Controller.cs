using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using App.Modules.Example.Data;
using App.Modules.Example.Models;
using App.Modules.Example.API;
using App.Modules.Example.Services;
using App.Modules.Example.Data.Storage.Db.EF;

namespace App.Modules.Example.Controllers.OData
{
    [Route(ModuleApiConstants.Areas.Module.OData.V1.Routing.Controllers.Controller1.Route)]
    public class ExampleO1Controller : ODataController
    {
        private readonly ModuleDbContext _moduleDbContext;

        public ExampleO1Controller(IExampleModuleService exampleMService, ModuleDbContext moduleDbContext) {
            _moduleDbContext = moduleDbContext;
        }


        [EnableQuery(PageSize = 100)]
        [HttpGet("")]
        //Not good for OData: [HttpGet("Get")]
        [HttpGet("$count")]
        //[ApiExplorerSettings(GroupName = AppAPIConstants.OpenAPI.Generation.Areas.ModuleA.OData.ID)]
        public IEnumerable<ExampleModuleModel> Get()
        {
            var t = _moduleDbContext.Bar.Select(x=>x).ToArray();

            return FakeDataBuilder.Get();
        }
    }
}
