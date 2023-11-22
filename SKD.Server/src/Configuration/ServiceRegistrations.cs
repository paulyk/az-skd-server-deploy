using System.Reflection;
using SKD.Application.Common;
using SKD.KitStatusFeed;

namespace SKD.Server.Configuration;

public static class ServiceRegistrations {

    public static IServiceCollection AddSingletonServices(
        this IServiceCollection services,
        AppSettings appSettings,
        DCWSServiceOptions DCWSServiceOptions
    ) => services
            .AddSingleton(DCWSServiceOptions)
            .AddSingleton<DcwsService>()
            .AddSingleton(sp => appSettings);

    public static IServiceCollection AddScopedServices(
        this IServiceCollection services,
        AppSettings appSettings
    ) {
        services
            .AddScoped(sp =>
                new KitService(sp.GetRequiredService<SkdContext>(), currentDate: DateTime.Now))
            .AddScoped<PartnerStatusService>()
            .AddScoped<KitStatusFeedService>();
            
        return services;
    }
    public static IServiceCollection AddAppServices(this IServiceCollection services) {
        Assembly? assembly = GetAssemblyByName("SKD.Application") ?? throw new Exception("SKD.Application assembly not found");

        foreach (Type type in assembly.GetTypes().Where(type => typeof(IAppService).IsAssignableFrom(type) && !type.IsAbstract)) {
            services.AddScoped(type);
        }

        return services;
    }

    private static Assembly? GetAssemblyByName(string assemblyName) {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SingleOrDefault(assembly => string.Equals(assembly.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase));
    }
}

