using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;

namespace MVCClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

           services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies"; // We are using a cookie to locally sign-in the user
                options.DefaultChallengeScheme = "oidc"; // because when we need the user to login, we will be using the OpenID Connect protocol.
            })
            .AddCookie("Cookies") // to add the handler that can process cookies
            .AddOpenIdConnect("oidc", options => // is used to configure the handler that perform the OpenID Connect protocol
            {
                options.Authority = "https://localhost:5000"; //indicates where the trusted token service is located
                options.RequireHttpsMetadata = false;

                options.ClientId = "mvc"; //identify this client by ClientId + ClientSecret
                options.ClientSecret = "secret";
                options.ResponseType = "code";

                //options.ClaimActions.MapUniqueJsonKey("email", "email"); // Just a test

                //options.Scope.Add("email");
                ////options.Scope.Add("Email");
                //options.Scope.Add("FaroResource"); ////IdentityServerConstants.StandardScopes.Email

                options.Scope.Add("api1");
                // To add the support of refresh token
                // Since SaveTokens is enabled, ASP.NET Core will automatically store the resulting access and refresh token in the authentication session.
                options.Scope.Add("offline_access");

                options.SaveTokens = true; // is used to persist the tokens from IdentityServer in the cookie (as they will be needed later).
            })
            ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication(); // To ensure the authentication services execute on each request
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "{controller=Home}/{action=Index}/{id?}"); // old default code 

                endpoints.MapDefaultControllerRoute() // method disables anonymous access for the entire application
                    .RequireAuthorization();
            });
        }
    }
}
