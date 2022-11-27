using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Base.Shared.Services
{
    public interface IEdmModelBuilder
    {
        IEdmModel BuildModel();
    }
}
