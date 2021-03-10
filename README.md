# Net Core 5 Secrets!
This project was built to get the secret configuration information for a project and put it into memory instead of a file on the machine.
## Files
The following files are used store configuration information that is not secret:
|Name|Purpose|
|--|--|
|appsettings.json|Configuration settings regardless of tier|
|appsettings.Development.json|Configuration settings for the development server|
|appsettings.local.json|Configuration settings for local development (used instead of secret manager)|

## Getting Secrets
The project is using the AWS SecretsManager to store the secrets. This can be changed by adding a different manager, and updating the method call in the GetConfiguration() method here:
```csharp
var appSecretsString = secret.GetSecret(region);
```

## Using Secrets

The ConfigurationManager creates an IConfiguration object when called in the Startup.cs class. This object can be used using the standard .Net Core functionality, such as:
```csharp
// get connection string
var connString = _config.GetConnectionString("DB1");
```
or in a model using the IOptions object, such as this (this uses a strongly typed class for the configuration object):
```csharp
public GetInfo(IOptions<AppInfo> options)
{
    var key = options.Value.Key;
}
```

## Important Items

If using AWS, the region and name of the key are currently pulled from the appsettings files. Should this be changed, the Startup.cs would be changed as well.

The settings files, and the secret are hierarchical. The configuration object uses a Dictionary<string, string>. In order to accommodate, the hierarchical key is created using a colon separator. The appsettings.local.json shows this:
```json
{
  "ConnectionStrings:DB1": "Server=127.0.0.1;Database=DB1;User Id=user1; Password=password1;",
  "ConnectionStrings:DB2": "Server=127.0.0.1;Database=DB2;User Id=user2; Password=password2;",
  "TestString": "test",
  "TestBool": "false"
}
```
As an example, this is the same as this, but it allows for deserialization:
```json
{
  "ConnectionStrings": {
    "DB1": "Server=127.0.0.1;Database=DB1;User Id=user1; Password=password1;",
    "DB2": "Server=127.0.0.1;Database=DB2;User Id=user2; Password=password2;"
  },
  "TestString": "test",
  "TestBool": "false"
}
```
This method doesn't deserialize to Dictionary<string, string> so there would need to be changes to accommodate, such as using a strongly typed class.
