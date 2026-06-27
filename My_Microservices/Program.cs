using Microsoft.Identity.Web;
using My_Microservices.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd");

// ── Services ──────────────────────────────────────────────────
builder.Services.AddControllers().AddDapr();          // ✅ Dapr wired into MVC
builder.Services.AddDaprClient();                     // ✅ DaprClient injectable

builder.Services.AddSingleton<ICosmosService, CosmosService>();     // ✅ Via interface
builder.Services.AddSingleton<IServiceBusService, ServiceBusService>();


// OpenAPI / Swagger
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();   // This adds full Swagger UI

var app = builder.Build();

// ✅ Initialize DB/container before accepting requests
var cosmos = app.Services.GetRequiredService<ICosmosService>();
await cosmos.InitializeAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();           // This enables Swagger
    app.UseSwagger();
    app.UseSwaggerUI();     // This enables the nice Swagger page
}

app.UseCloudEvents();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Keep the root endpoint for testing
app.MapGet("/", () => "✅ OrderService is running! Hello from .NET");

app.Run();