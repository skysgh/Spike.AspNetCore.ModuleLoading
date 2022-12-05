using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Modules.Example.Data.Storage.Db.EF
{
    public class ModuleDbContext : DbContext
    {
        public ModuleDbContext(DbContextOptions<ModuleDbContext> options) :
      base(options)
        {
            //Nothing special...
        }

    }
}
