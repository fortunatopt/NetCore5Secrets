using InMemorySecrets.ConfigurationModels;
using InMemorySecrets.DatabaseContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InMemorySecrets
{
    public class Startup
    {
        public IConfiguration _config { get; }
        public IWebHostEnvironment _env { get; }
        public Startup(IConfiguration config, IWebHostEnvironment env)
        {
            _env = env;
            _config = _env.GetConfiguration(config.GetValue<string>("SecretKey"), config.GetValue<string>("Region"));
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // get db connection string by name
            var connString = _config.GetConnectionString("DB1");

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
