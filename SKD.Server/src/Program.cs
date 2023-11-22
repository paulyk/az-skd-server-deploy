

using SKD.Server.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
AppSettings appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
DCWSServiceOptions dcwsServiceOptions = builder.Configuration.GetSection("DCWSServiceOptions").Get<DCWSServiceOptions>() ?? new DCWSServiceOptions();

// Configurations
builder.Services.ConfigureCors();
builder.Services.ConfigureSkdContext(connectionString);
builder.Services.ConfigureGraphQLServer(builder.Environment, appSettings);
builder.Services.ConfigureKitStatusFeedService(builder.Configuration);

// Service Registrations
builder.Services.AddSingletonServices(appSettings, dcwsServiceOptions);
builder.Services.AddScopedServices(appSettings);
builder.Services.AddAppServices();

if (!builder.Environment.IsDevelopment()) {
    builder.Services.AddApplicationInsightsTelemetry();
}

WebApplication app = builder.Build();

app.UseCors();
app.MapGraphQL();
app.MapRouting();

app.Run();
