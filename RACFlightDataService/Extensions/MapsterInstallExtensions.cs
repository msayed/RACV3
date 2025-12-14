using System.Reflection;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RACFlightDataService.Extensions;

public static class MapsterInstallExtensions  {
    public static IServiceCollection AddMapster(this IServiceCollection serviceCollection, IConfiguration configuration=null) {
        var config = TypeAdapterConfig.GlobalSettings;
        // config.EnableJsonMapping();
        config.Scan(new Assembly[] {
            (typeof(Program)).Assembly,
        });
        return serviceCollection;

    }
}