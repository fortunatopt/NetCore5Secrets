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
            var connectionStrings = new Dictionary<string, string>
            {
                [$"ConnectionStrings:DB1"] = "DB1".GetSecret(region)
            };
            // add in-memory collection
            builder.AddInMemoryCollection(connectionStrings);
            // build config
            _config = builder.Build();

        }
        public class DbSecret
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Engine { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public string DbName { get; set; }
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // get db connection string by name
            var db1 = _config.GetConnectionString("DB1");
            // TODO: deserialize secret depending on structure
            var connObject = JsonConvert.DeserializeObject<DbSecret>(db1);
            var connString = $"Server={connObject.Host};Database={connObject.DbName};User Id={connObject.Username}; Password={connObject.Password};";

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
