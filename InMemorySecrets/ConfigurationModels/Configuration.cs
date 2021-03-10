using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace InMemorySecrets.ConfigurationModels
{
    public static class ConfigurationManager
    {
        public static IConfiguration GetConfiguration(this IWebHostEnvironment env, string secret = "", string region = "")
        {
            // build the config, with secrets in memory
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(env.ContentRootPath);
            // get non-secret config items with environment specific file if it exists
            builder.AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            // if not dev, go get secrets from AWS, otherwise get local dev file
            if (env.EnvironmentName != "Development")
            {
                // get secret string
                var appSecretsString = secret.GetSecret(region);
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
            return builder.Build();
        }
    }
    public static class ConfigurationManagerExtensions
    {
        public static IConfiguration UseConfigurationManager(this IWebHostEnvironment env, string secret = "", string region = "")
        {
            return env.GetConfiguration(secret, region);
        }
    }
}
