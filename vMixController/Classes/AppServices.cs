using Microsoft.Extensions.DependencyInjection;
using System;

namespace vMixController.Classes
{
    internal static class AppServices
    {
        private static IServiceProvider _provider;

        public static void Configure(IServiceProvider provider)
        {
            _provider = provider;
        }

        public static T GetRequiredService<T>()
        {
            if (_provider == null)
                throw new InvalidOperationException("Service provider is not configured. Call AppServices.Configure during application startup.");

            return _provider.GetRequiredService<T>();
        }

        public static bool IsRegistered<T>() where T : class
        {
            return _provider?.GetService<T>() != null;
        }
    }
}
