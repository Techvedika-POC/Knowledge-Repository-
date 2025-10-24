using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services;
using KnowLedger_Synaptix.Services.Implementations;
using KnowLedger_Synaptix.Services.Interfaces;
using KnowledgeSynaptix.Services.Implementations;
using KnowledgeSynaptix.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Database Context

builder.Services.AddDbContext<Knowledge_Repository_dbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion


// Dependency Injection

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IKnowledgeItemService, KnowledgeItemService>();
builder.Services.AddScoped<IDomainService, DomainService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IGlobalSearchService, GlobalSearchService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IFreshPickService, FreshPickService>();
builder.Services.AddScoped<ITrendingService, TrendingService>();
builder.Services.AddScoped<ITopicHighlightService, TopicHighlightService>();
builder.Services.AddScoped<IDaySpotlightService, DaySpotlightService>();
builder.Services.AddScoped<IEngagementService, EngagementService>();
builder.Services.AddScoped<IApproverService, ApproverService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IFileEmbeddingService, FileEmbeddingService>();
builder.Services.AddScoped<IQdrantService, QdrantService>();
builder.Services.AddScoped<IEventRegistrationService, EventRegistrationService>();

#region ----------------- HttpClient Services -----------------
// For EmbeddingService (allow self-signed certs)
builder.Services.AddHttpClient<IEmbeddingService, EmbeddingService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromMinutes(2));


// Named HttpClient for Qdrant

builder.Services.AddHttpClient("QdrantClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:6333/");
    client.Timeout = TimeSpan.FromMinutes(2);
});
#endregion

#region ----------------- CORS Configuration -----------------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[]
{
    "http://localhost:3000",
    "https://knowledge-frontend-n567.onrender.com"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
#endregion


// Controllers

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
#endregion


// JWT Authentication

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
    };
});
builder.Services.AddAuthorization();
#endregion


// Authorization & Swaggerui

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();///it will collect all the metadaa about the endpoints in controllers
builder.Services.AddSwaggerGen();


// Build Application

var app = builder.Build();

// Middleware

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve static files (wwwroot)
app.UseStaticFiles();

// Serve uploads directory (optional)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});
app.UseRouting();
// Use CORS
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
#endregion
