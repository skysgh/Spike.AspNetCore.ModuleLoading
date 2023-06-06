using App.Modules.Example.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace App.Modules.Example.Data.Storage.Db.EF
{
    public class ModuleDbContext : DbContext
    {
        public DbSet<BarEntity> Bar { get; set; }
        //public ModuleDbContext()
        //{

        //    var t = true;
        //}

        public ModuleDbContext(DbContextOptions<ModuleDbContext> options) :
      base(options)
        {
            //Nothing special...
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Spike20221206A;Integrated Security = True;");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new ModuleModelBuilderInitialiser().Initialise(modelBuilder);
            new ModuleModelBuilderSeeder().Initialise(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

    }
}
