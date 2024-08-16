var builder = DistributedApplication.CreateBuilder(args);

var dbHost = builder.AddPostgres("keycloakaspire-dbserver")
    .WithEnvironment("POSTGRES_DB", "KeycloakDb")
    .WithPgAdmin();

//var cache = builder.AddRedis("cache");


var db = dbHost.AddDatabase("KeycloakDb");

var apiService = builder.AddProject<Projects.KeycloakAspireApp_ApiService>("apiservice")
    //.WithReference(cache)
    .WithReference(db);

builder.AddProject<Projects.KeycloakAspireApp_Spa>("keycloakaspireapp-spa")
    .WithExternalHttpEndpoints()
    //.WithReference(cache)
    .WithReference(apiService);

await builder.Build().RunAsync();