using Api.Utils;
using Logic.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
     // more specific than AddMvc()
     .AddControllersWithViews()
     .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            var config = new Config(3);  // deserialize from appsettings.json
            services.AddSingleton(config);

            var connectionString = new ConnectionString(Configuration["ConnectionString"]);
            services.AddSingleton(connectionString);

            services.AddSingleton<SessionFactory>();
            services.AddSingleton<Messages>();
            services.AddHandlers();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandler>();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                // Which is the same as the template
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
