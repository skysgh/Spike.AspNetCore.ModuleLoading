using App.Base.DI;
using System.Reflection;

namespace App.Base.Host.Services
{
    public interface IModuleLoadingService
    {
        ControllerTypeToScopeDictionary Scopes { get; }
        public Assembly Load(string assemblyFilePath, string? assemblyResolutionBaseDirectoryPath = null);
    }
}
