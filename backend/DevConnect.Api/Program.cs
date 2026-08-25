using DevConnect.Api.Config;
using DevConnect.Api.Middleware;
using DevConnect.Api.Services;
using DevConnect.Api.Services.QueryRepository;

var builder = WebApplication.CreateBuilder(args);

// -------------------- CognoDB settings from environment variables --------------------
// Never read from appsettings.json — see Config/CognoDbSettings.cs
var cognoDbSettings = CognoDbSettings.FromEnvironment();
builder.Services.AddSingleton(cognoDbSettings);

// -------------------- Neo4j driver / service (singleton — driver manages its own pool) --------------------
builder.Services.AddSingleton<INeo4jService, Neo4jService>();

// -------------------- Query repositories --------------------
builder.Services.AddScoped<DeveloperQueries>();
builder.Services.AddScoped<ProjectQueries>();
builder.Services.AddScoped<RecommendationQueries>();

// -------------------- CORS (so the React frontend on a different port/origin can call this API) --------------------
var allowedOrigins = new[]
{
    "http://localhost:3000",
    "https://devconnect-graph-app-production-d596.up.railway.app"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// -------------------- Controllers + Swagger --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "DevConnect Graph API",
        Version = "v1",
        Description = "REST API backed by CognoDB (graph database) for exploring developers, skills, and projects."
    });
});

var app = builder.Build();

// -------------------- Middleware pipeline --------------------
// Custom exception handler FIRST so it can catch failures from everything after it,
// including CognoDB connectivity issues — this is what makes DB-down graceful.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DevConnect Graph API v1");
});
app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthorization();
app.MapControllers();

// Simple root health message
app.MapGet("/", () => Results.Ok(new { service = "DevConnect Graph API", status = "running" }));

app.Run();
