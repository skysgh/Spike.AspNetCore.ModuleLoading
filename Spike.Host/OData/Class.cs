using Microsoft.OData;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace App.Base.OData
{
    public class AppODataContainerBuilder : IContainerBuilder
    {
        internal IServiceCollection Services { get; } = new ServiceCollection();

        /// <summary>
        /// Adds a service of <paramref name="serviceType"/> with an <paramref name="implementationType"/>.
        /// </summary>
        /// <param name="lifetime">The lifetime of the service to register.</param>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationType">The implementation type of the service.</param>
        /// <returns>The <see cref="IContainerBuilder"/> instance itself.</returns>
        public virtual IContainerBuilder AddService(Microsoft.OData.ServiceLifetime lifetime,
            Type serviceType,
            Type implementationType)
        {
            if (serviceType == null)
            {
                throw new Exception("....");// Error.ArgumentNull(nameof(serviceType));
            }

            if (implementationType == null)
            {
                throw new Exception("....");// Error.ArgumentNull(nameof(implementationType));
            }

            Services.Add(new ServiceDescriptor(
                serviceType, implementationType, TranslateServiceLifetime(lifetime)));

            return this;
        }

        /// <summary>
        /// Adds a service of <paramref name="serviceType"/> with an <paramref name="implementationFactory"/>.
        /// </summary>
        /// <param name="lifetime">The lifetime of the service to register.</param>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>The <see cref="Microsoft.OData.IContainerBuilder"/> instance itself.</returns>
        public virtual IContainerBuilder AddService(Microsoft.OData.ServiceLifetime lifetime,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory)
        {
            if (serviceType == null)
            {
                throw new Exception("....");// Error.ArgumentNull(nameof(serviceType));
            }

            if (implementationFactory == null)
            {
                throw new Exception("....");// Error.ArgumentNull(nameof(implementationFactory));
            }

            Services.Add(new ServiceDescriptor(
                serviceType, implementationFactory, TranslateServiceLifetime(lifetime)));

            return this;
        }

        /// <summary>
        /// Builds a container which implements <see cref="IServiceProvider"/> and contains
        /// all the services registered.
        /// </summary>
        /// <returns>The container built by this builder.</returns>
        public virtual IServiceProvider BuildContainer()
        {
            return Services.BuildServiceProvider();
        }

        private static Microsoft.Extensions.DependencyInjection.ServiceLifetime TranslateServiceLifetime(
            Microsoft.OData.ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case Microsoft.OData.ServiceLifetime.Scoped:
                    return Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped;

                case Microsoft.OData.ServiceLifetime.Singleton:
                    return Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton;

                default:
                    return Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient;
            }
        }
    }

}
