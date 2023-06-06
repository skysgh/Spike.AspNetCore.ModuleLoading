using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace App.Modules.Host
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var builder = CreateHostBuilder(args);

            //var host = builder.Build();

            //host.Run();

        }

        //public static IHostBuilder CreateHostBuilder(string[] args)
        //{
        //    var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
        //        .ConfigureServices((hostContext, services) =>
        //            {
        //                // register your services here.


        //            });

        //    string connectionString = builder.Configuration.GetConnectionString("DefaultSqlServer");

        //    Action<DbContextOptionsBuilder> callback = x =>
        //    {
        //        x.EnableSensitiveDataLogging(true);
        //        x.UseSqlServer(connectionString);
        //    };

        //    hostBuilder.ser.AddDbContext<AppDbContext>(callback);


        //    return hostBuilder;
        //}
        //}

        // Migrate:
    }
}