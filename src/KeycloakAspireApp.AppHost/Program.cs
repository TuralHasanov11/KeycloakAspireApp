var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddPostgres("keycloakaspire-dbserver")
    .WithDataVolume()
    .WithPgAdmin()
    .AddDatabase("KeycloakDb");

var cache = builder.AddRedis("cache");

var maildev = builder.AddMailDev("maildev");

//var storage = builder.AddAzureStorage("Storage");
//if (builder.Environment.IsDevelopment())
//{
//    storage.RunAsEmulator();
//}

//var blobs = storage.AddBlobs("BlobConnection");
//var queues = storage.AddQueues("QueueConnection");


var secret = builder.AddParameter("secret", secret: true);


var apiService = builder.AddProject<Projects.KeycloakAspireApp_ApiService>("apiservice")
    .WithReference(cache)
    .WithReference(db)
    .WithReference(maildev)
    .WithEnvironment("SECRET", secret);

builder.AddProject<Projects.KeycloakAspireApp_Spa>("keycloakaspireapp-spa")
    .WithExternalHttpEndpoints()
    //.WithReplicas(2)
    .WithReference(cache)
    //.WithReference(blobs)
    //.WithReference(queues)
    .WithReference(apiService);

await builder.Build().RunAsync();