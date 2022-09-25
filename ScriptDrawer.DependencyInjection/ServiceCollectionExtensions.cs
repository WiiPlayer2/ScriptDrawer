using ScriptDrawer.Core;
using ScriptDrawer.Serialization;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScriptDrawer(this IServiceCollection services)
        => services
            .AddSingleton<Engine>()
            .AddSingleton<ConfigSerializer>();
}
