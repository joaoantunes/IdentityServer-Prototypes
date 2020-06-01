// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Reflection;

namespace IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }

        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // uncomment, if you want to add an MVC-based UI
            services.AddControllersWithViews();

            services.AddAuthentication()
            .AddGoogle("Google", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                options.ClientId = "<insert here>"; //TODO não sei onde está o ClientId, tentei criar mas não sei como fazer com Urls de dominio etc,,
                options.ClientSecret = "AIzaSyDzTOSMQok3laHSkTeFR8cit9GIEXu1-OY";
            })
            .AddGitHub(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = "71d691134a7a2a1f0f53";
                options.ClientSecret = "a7fa120819cb37996b6964434df5fd07505249b8";
            })
            .AddOpenIdConnect("oidc", "Demo IdentityServer", options =>
             {
                 options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                 options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                 options.SaveTokens = true;

                 options.Authority = "https://demo.identityserver.io/";
                 options.ClientId = "native.code";
                 options.ClientSecret = "secret";
                 options.ResponseType = "code";

                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     NameClaimType = "name",
                     RoleClaimType = "role"
                 };
             });

            // Because we are using EF migrations, the call to MigrationsAssembly is used to inform Entity Framework that the host project will contain the migrations code. 
            // This is necessary since the host project is in a different assembly than the one that contains the DbContext classes.
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            const string connectionString = @"Data Source=PRTLAPB8SZTP2\SQLEXPRESS;database=IdentityServer4.Quickstart.EntityFramework-3.0.0;trusted_connection=yes;";

            // Tis are for using EF for 
            var builder = services.AddIdentityServer()
                .AddTestUsers(TestUsers.Users)
                // ConfigurationDbContext . used for configuration data such as clients, resources, and scopes
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                // PersistedGrantDbContext - used for temporary operational data such as authorization codes, and refresh tokens
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                });

            // This is use for in memory registration
            //    .AddInMemoryIdentityResources(Config.Ids)
            //    .AddInMemoryApiResources(Config.Apis)
            //    .AddInMemoryClients(Config.Clients)
            //    .AddTestUsers(TestUsers.Users); // an in-memory “user database” for testing!

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();
        }

        public void Configure(IApplicationBuilder app)
        {
            // this will do the initial DB population, In the future this could be called in another project, just to run the initialization
            // since the start of the server shouldn't Initialize a DB.
            //InitializeDatabase(app);

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // uncomment if you want to add MVC
            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();

            // uncomment, if you want to add MVC
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                PrePopulateEmptyDatabaseTables(context);
            }
        }

        private void PrePopulateEmptyDatabaseTables(ConfigurationDbContext context)
        {
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
                foreach (var resource in Config.Ids)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var resource in Config.Apis)
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
        }
    }
}
