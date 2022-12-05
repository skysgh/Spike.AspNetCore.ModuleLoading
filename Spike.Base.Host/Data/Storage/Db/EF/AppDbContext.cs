using Microsoft.EntityFrameworkCore;

namespace App.Base.Data.Storage.Db.EF
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) :
              base(options)
        {
            //Nothing special...
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new AppModelBuilderInitialiser().Initialise(modelBuilder);

            //Note that calling this will cause it to double loop through here...Hum...
            new AppModelBuilderSeeder().Initialise(modelBuilder);

            //Does nothing:
            base.OnModelCreating(modelBuilder);

            //configuringOrConfigured = true;
        }

    }

}
