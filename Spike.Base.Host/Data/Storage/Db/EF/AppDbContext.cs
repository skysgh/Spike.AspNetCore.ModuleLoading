using App.Base.Services;
using Microsoft.EntityFrameworkCore;

namespace App.Base.Data.Storage.Db.EF
{

    public abstract class AppDbContextBase<T> : DbContext
        where T : DbContext 
    {
        public AppDbContextBase(DbContextOptions<T> options) :

              base(options)
        {
            //Nothing special...
        }
    }


        public class AppDbContext : AppDbContextBase<AppDbContext>
    {
        private readonly IFooService _fooService;

        public AppDbContext(DbContextOptions<AppDbContext> options, IFooService fooService) :

              base(options)
        {
            _fooService = fooService;

            fooService.Do();
            //Nothing special...
        }

        //public AppDbContext(DbContextOptions dbContext):
        //    base(dbContext)
        //{ }


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
