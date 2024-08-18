var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.AddRequestTimeouts();
builder.Services.AddOutputCache();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRequestTimeouts();
app.UseOutputCache();

app.MapGet("/weatherforecast", async (IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient();

    httpClient.BaseAddress = new Uri("https+http://apiservice");

    var response = await httpClient.GetAsync("/weatherforecast");

    var forecast = await response.Content.ReadAsStringAsync();

    return TypedResults.Ok(forecast);
})
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapDefaultEndpoints();


await app.RunAsync();