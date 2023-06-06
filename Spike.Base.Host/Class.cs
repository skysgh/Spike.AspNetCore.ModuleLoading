using ICSharpCode.Decompiler.Metadata;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace App
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection CloneNaively(this IServiceCollection source)
        {
            var result = new ServiceCollection();

            foreach (var serv in source)
            {
                result.Add(serv);
            }
            return result;
        }

        public static IServiceCollection CloneIntelligently(this IServiceCollection source, IServiceProvider serviceProvider)
        {
            var childServiceCollection = new ServiceCollection();
            foreach (ServiceDescriptor service in source)
            {
                if (service.Lifetime == ServiceLifetime.Singleton && !service.ServiceType.IsGenericTypeDefinition)
                {
                    object? serviceInstance = serviceProvider.GetService(service.ServiceType);
                    if (serviceInstance != null)
                    {
                        childServiceCollection.AddSingleton(service.ServiceType, serviceInstance);
                        continue;
                    }
                }

                childServiceCollection.Add(service);
            }

            return childServiceCollection;
        }
    }
}
