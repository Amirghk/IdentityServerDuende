using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // The Call to MigrationsAssembly() tells the EF that the host project will contain the migrations
        // This is necessary since the host project is in a different assembly than the one that contains the DbContext classes
        var migrationAssembly = typeof(Program).Assembly.GetName().Name;
        const string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB; Initial Catalog=Duende.IdentityServer.QuickStart; Integrated Security=TRUE";
        // uncomment if you want to add a UI
        builder.Services.AddRazorPages();

        builder.Services.AddIdentityServer(options =>
            {
                // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
                options.EmitStaticAudienceClaim = true;
            })
            // used for configuration data such as clients, resources, and scopes
            // configurationDbContext
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(migrationAssembly));
            })
            // used for dynamic operational data such as authorization codes and refresh tokens
            // PersistedGrantDbContext
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(migrationAssembly));
            })
            // .AddInMemoryIdentityResources(Config.IdentityResources)
            // .AddInMemoryApiScopes(Config.ApiScopes)
            // .AddInMemoryClients(Config.Clients)
            .AddTestUsers(TestUsers.Users);

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        InitializeDatabase(app);
        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();

        app.UseIdentityServer();

        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }

    private static void InitializeDatabase(IApplicationBuilder app)
    {
        using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
        {
            serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

            // seeding data into configurationDbContext
            var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            context.Database.Migrate();
            if (!context.Clients.Any())
            {
                foreach (var client in Config.Clients)
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }
            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Config.IdentityResources)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            if (!context.ApiScopes.Any())
            {
                foreach (var resource in Config.ApiScopes)
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
        }
    }
}
