
using IdentityModel.Client;

using SKD.KitStatusFeed;
namespace SKD.Server.Configuration;

public static class ServiceConfigurations {
    public static IServiceCollection ConfigureSkdContext(this IServiceCollection services, string connectionString) {
        return services
            .AddPooledDbContextFactory<SkdContext>(
                options => options.UseSqlServer(connectionString, sqlOptions => {
                    sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                }))
            .AddScoped(p => p.GetRequiredService<IDbContextFactory<SkdContext>>().CreateDbContext());
    }

    public static IServiceCollection ConfigureGraphQLServer(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        AppSettings appSettings
    ) {
        services
            .AddGraphQLServer()
            .RegisterDbContext<SkdContext>(DbContextKind.Pooled)
            .AddQueryType<Query>()
                .AddTypeExtension<ProjectionQuery>()
                .AddTypeExtension<SummaryQuery>()
                .AddTypeExtension<PartnerStatusQuery>()
            .AddMutationType<Mutation>()
                .AddTypeExtension<PartnerStatusMutation>()
                .AddTypeExtension<KitMutation>()
            .AddType<UploadType>()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .AddInMemorySubscriptions()
            .ModifyRequestOptions(opt => {
                opt.IncludeExceptionDetails = environment.IsDevelopment();
                opt.ExecutionTimeout = TimeSpan.FromSeconds(appSettings.ExecutionTimeoutSeconds);
            })
            .AllowIntrospection(appSettings.AllowGraphqlIntrospection)
            .InitializeOnStartup();

        return services;
    }

    public static IServiceCollection ConfigureCors(this IServiceCollection services) {
        return services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod()));
    }

    public static IServiceCollection ConfigureKitStatusFeedService(this IServiceCollection services, IConfiguration configuration) {

        // for staging / test environment KitStatusFeed should not be set
        if (configuration.GetSection("KitStatusFeed") == null) {
            Console.WriteLine("KitStatusFeed section not found in appsettings.json");
            return services;
        }

        AccessTokenOptions accessTokenOptions = configuration
            .GetSection("KitStatusFeed:AccessToken")
            .Get<AccessTokenOptions>() ?? throw new Exception("Failed to get KsfAccessTokenOptions");
        string kitStatusFeedUrl = configuration["KitStatusFeed:KitStatusFeedUrl"] ?? "";

        services.AddSingleton(accessTokenOptions);

        services.AddHttpClient<KitStatusFeedService>("KitStatusFeedService", client => {
            client.BaseAddress = new Uri(kitStatusFeedUrl);
        }).AddClientAccessTokenHandler();

        services.AddAccessTokenManagement(options => {
            options.Client.Clients.Add("identity", new ClientCredentialsTokenRequest {
                Address = accessTokenOptions.TokenGenerationEndpoint,
                ClientId = accessTokenOptions.ClientId,
                ClientSecret = accessTokenOptions.ClientSecret,
                Scope = accessTokenOptions.Scope,
                GrantType = accessTokenOptions.GrantType
            });
        });

        return services;
    }

}