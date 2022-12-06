using App.Modules.Example.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Modules.Example.Data.Storage.Db.EF
{
    public class ModuleModelBuilderInitialiser
    {
        static string MySchema = "Module";


        public void Initialise(ModelBuilder modelBuilder)
        {

            var model = modelBuilder.Entity<BarEntity>();

            model.ToTable("BarEntity", MySchema);

            model.HasKey(x => x.Id);

        }


    }
}
