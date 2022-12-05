using App.Base.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Base.Data.Storage.Db.EF
{
    public class AppModelBuilderInitialiser
    {
        static string MySchema = "Base";

        public void Initialise(ModelBuilder modelBuilder)
        {

            var model = modelBuilder.Entity<FooEntity>();

            model.ToTable("FooEntity", MySchema);

            model.HasKey(x => x.Id);

        }
    }
}

