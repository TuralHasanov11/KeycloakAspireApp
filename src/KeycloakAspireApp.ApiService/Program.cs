using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using KeycloakAspireApp.ApiService.Data;
using Microsoft.Extensions.Caching.Distributed;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

//builder.Services.AddNpgsqlDataSource("KeycloakDb");

builder.AddRedisDistributedCache("cache");

builder.AddNpgsqlDbContext<ApplicationDbContext>("KeycloakDb");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(sp =>
{
    var smtpUri = new Uri(builder.Configuration.GetConnectionString("maildev")!);

    return new SmtpClient(smtpUri.Host, smtpUri.Port);
});

var secret = builder.Configuration.GetValue("secret", "");
Console.WriteLine($"Secret: {secret}");

builder.AddAzureBlobClient("BlobConnection");
builder.AddAzureQueueClient("QueueConnection");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    //var blobService = app.Services.GetRequiredService<BlobServiceClient>();
    //var docsContainer = blobService.GetBlobContainerClient("fileuploads");
    //await docsContainer.CreateIfNotExistsAsync();

    //var queueService = app.Services.GetRequiredService<QueueServiceClient>();
    //var queueClient = queueService.GetQueueClient("tickets");
    //await queueClient.CreateIfNotExistsAsync();
}

app.UseExceptionHandler();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (IDistributedCache cache) =>
{
    var cachedForecast = await cache.GetAsync("forecast");

    if(cachedForecast is null)
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();

        await cache.SetAsync("forecast", Encoding.UTF8.GetBytes(JsonSerializer.Serialize(forecast)), new()
        {
            AbsoluteExpiration = DateTime.Now.AddSeconds(10)
        });

        return forecast;
    }


    return JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(cachedForecast);
})
    .WithName("GetWeatherForecast")
    .WithOpenApi();


app.MapPost("/subscribe", async (SmtpClient smtpClient, string email) =>
{
    using var message = new MailMessage("newsletter@yourcompany.com", email)
    {
        Subject = "Welcome to our newsletter!",
        Body = "Thank you for subscribing to our newsletter!"
    };

    await smtpClient.SendMailAsync(message);
}).WithName("TestMailDev")
  .WithOpenApi(); ;

app.MapDefaultEndpoints();

await app.RunAsync();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}