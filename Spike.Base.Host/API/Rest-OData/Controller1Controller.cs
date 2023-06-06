using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using App.Base.Data;
using App.Base.API;
using App.Base.Shared.Services;
using App.Base.Data.Storage.Db.EF;

namespace App.Base.Host.API.OData
{
    [Route(AppAPIConstants.Areas.Base.OData.V1.Routing.Controllers.Controller1.Route)]
    public class ControllerO1Controller : ODataController
    {
        private IExampleHService _exampleHService;
        private readonly AppDbContext _appDbContext;

        public ControllerO1Controller(IExampleHService exampleSharedService, AppDbContext appDbContext)
        {
            _exampleHService = exampleSharedService;
            _appDbContext = appDbContext;
        }


        [EnableQuery(PageSize = 100)]
        [HttpGet("")]
        //Not good for OData: [HttpGet("Get")]
        [HttpGet("$count")]
        //[ApiExplorerSettings(GroupName = AppAPIConstants.OpenAPI.Generation.Areas.ModuleA.OData.ID)]
        public IActionResult Get()
        {
            return Ok(FakeDataBuilder.Get());
        }
    }
}
