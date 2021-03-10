using InMemorySecrets.AWSModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace InMemorySecrets
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureAppConfiguration((context, builder) =>
                {
                    // set base path for configs
                    builder.SetBasePath(context.HostingEnvironment.ContentRootPath);
                    // get non-secret config items with environment specific file if it exists
                    builder.AddJsonFile("appsettings.json", optional: true)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                    // if not dev, go get secrets from AWS, otherwise get local dev file
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        // use short names for env
                        var envShort = context.HostingEnvironment.EnvironmentName.ToLower() switch
                        {
                            "production" => "prd",
                            "staging" => "stg",
                            "development" => "dev",
                            _ => "NoConnectionString"
                        };
                        // get secret string
                        var appSecretsString = $"App-Secrets-{envShort}".GetSecret("us-west-2");
                        // deserialize to dictionary
                        var appSecrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(appSecretsString);
                        // add dictionary to in memory config collection
                        builder.AddInMemoryCollection(appSecrets);
                    }
                    else
                    {
                        // get local file for developing
                        builder.AddJsonFile("appsettings.local.json", optional: true);
                    }
                });
    }
}