using InMemorySecrets.AWSModels;
using InMemorySecrets.DatabaseContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace InMemorySecrets
{
    public class Startup
    {
        public IConfiguration _config { get; }
        public IWebHostEnvironment _env { get; }
        public Startup(IConfiguration config, IWebHostEnvironment env)
        {
            // get the region from the appsettings.json
            string region = config.GetValue<string>("AWSRegion");
            // set environment
            _env = env;
            // build the config, with secrets in memory
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(_env.ContentRootPath);
            // get non-secret config items with environment specific file if it exists
            builder.AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{_env.EnvironmentName}.json", optional: true);

            // build connection string object
            // DB Connection string stored as plain text
            var connectionStrings = new Dictionary<string, string>
            {
                [$"ConnectionStrings:DB-Pass"] = "DB-Pass".GetSecret(region)
            };
            // add in-memory collection
            builder.AddInMemoryCollection(connectionStrings);
            // build config
            _config = builder.Build();

        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // get db connection string by name
            var connString = _config.GetConnectionString("DB-Pass");

            // add db context
            services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(connString).EnableSensitiveDataLogging(_env.IsDevelopment())
            );
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
