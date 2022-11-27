//using Microsoft.OData;
//using Microsoft.OData.Edm;
//using Microsoft.OData.UriParser;
//using System.Diagnostics.Contracts;

//namespace App.Base.OData
//{
//    public class ODataPatch
//    {

//        /// <summary>
//        /// Build the container.
//        /// </summary>
//        /// <param name="model">The Edm model.</param>
//        /// <param name="setupAction">The setup config.</param>
//        /// <returns>The built service provider.</returns>
//        public  IServiceProvider BuildRouteContainer(IEdmModel model, Action<IServiceCollection> setupAction)
//        {
//            Contract.Assert(model != null);

//            ServiceCollection services = new ServiceCollection();
//            /*REPLACE DEFAULT INTERNAL ONE DefaultContainerBuilder*/
//            AppODataContainerBuilder builder = new AppODataContainerBuilder();

//            // Inject the core odata services.
//            builder.AddDefaultODataServices();

//            // Inject the default query setting from this options.
//            builder.Services.AddSingleton(sp => QuerySettings);

//            // Inject the default Web API OData services.
//            builder.AddDefaultWebApiServices();

//            // Set Uri resolver to by default enabling unqualified functions/actions and case insensitive match.
//            builder.Services.AddSingleton<ODataUriResolver>(sp =>
//                new UnqualifiedODataUriResolver
//                {
//                    EnableCaseInsensitive = true, // by default to enable case insensitive
//                    EnableNoDollarQueryOptions = EnableNoDollarQueryOptions // retrieve it from global setting
//                });

//            // Inject the Edm model.
//            // From Current ODL implement, such injection only be used in reader and writer if the input
//            // model is null.
//            builder.Services.AddSingleton(sp => model);

//            // Inject the customized services.
//            setupAction?.Invoke(builder.Services);

//            return builder.BuildContainer();
//        }

//    }
//}
