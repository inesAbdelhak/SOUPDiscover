using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SoupDiscover.Common;
using SoupDiscover.Core;
using SoupDiscover.Database;
using SoupDiscover.ICore;
using SoupDiscover.ORM;

namespace SoupDiscover
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
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "dist";
            });
            var databaseType = Configuration.GetDatabaseType();
            switch (databaseType)
            {
                case DatabaseType.SQLite:
                    services.AddDbContext<DataContext, SqliteDataContext>();
                    break;
                case DatabaseType.Postgres:
                    services.AddDbContext<DataContext, PostgresDataContext>();
                    break;
                default:
                    throw new SoupDiscoverException($"The databaseType {databaseType} is not supported!");
            }
            services.AddSingleton<IProjectJobManager, ProjectJobManager>();
            services.AddTransient<IProjectJob, ProjectJob>();
            services.AddTransient<ISearchPackage, SearchNpmPackage>();
            services.AddTransient<ISearchPackage, SearchNugetPackage>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var log4netConfigFile = Configuration.GetSection("Logging").GetValue("log4net.config", "log4net.config");
            if (log4netConfigFile != null)
                loggerFactory.AddLog4Net(log4netConfigFile);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "api/{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
