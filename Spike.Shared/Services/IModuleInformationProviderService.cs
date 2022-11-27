using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Base.Shared.Services
{
    // Note: must implement : IModuleService 
    // to be usable from a Module...
    public interface IModuleInformationProviderService : IModuleService
    {
        public string Name { get;  }    
        public string Description { get;  }

        public string RestRoutePrefix { get; }
        public string RestODataRoutePrefix { get; }

    }
}
