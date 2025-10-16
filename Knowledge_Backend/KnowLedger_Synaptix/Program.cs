using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services;
using KnowLedger_Synaptix.Services.Implementations;
using KnowLedger_Synaptix.Services.Interfaces;
using KnowledgeSynaptix.Services.Implementations;
using KnowledgeSynaptix.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region ----------------- Database Context -----------------

// Configure Entity Framework Core with PostgreSQL
builder.Services.AddDbContext<Knowledge_Repository_dbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region ----------------- Application Services -----------------

// Register core application services for dependency injection
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
builder.Services.AddScoped<IFileEmbeddingService, FileEmbeddingService>();
builder.Services.AddScoped<IQdrantService, QdrantService>();

#endregion

#region ----------------- HttpClient Services -----------------

// EmbeddingService: Allows connecting to external embedding APIs with self-signed certificates 
builder.Services.AddHttpClient<IEmbeddingService, EmbeddingService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromMinutes(2));

// Named HttpClient for Qdrant vector database
builder.Services.AddHttpClient("QdrantClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:6333/"); // Qdrant HTTP endpoint
    client.Timeout = TimeSpan.FromMinutes(2);
});

#endregion

#region ----------------- CORS Configuration -----------------

// Configure Cross-Origin Resource Sharing (CORS) to allow frontend apps to access API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins(
                "http://localhost:3000", // local development frontend
                "https://knowledge-frontend-n567.onrender.com") // production frontend
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

#endregion

#region ----------------- Controllers & JSON Settings -----------------

// Add controller support with Newtonsoft.Json for complex object serialization
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

#endregion

#region ----------------- JWT Authentication -----------------

// Configure JWT authentication
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

// Enable authorization policies
builder.Services.AddAuthorization();

#endregion

#region ----------------- Swagger / API Documentation -----------------

// Enable OpenAPI / Swagger for API documentation and testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#endregion

#region ----------------- Build & Configure App -----------------

var app = builder.Build();

// Enable Swagger UI only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve static files (necessary for file uploads and downloads)
app.UseStaticFiles();

// Enforce HTTPS and configure routing
app.UseHttpsRedirection();
app.UseRouting();

// Apply CORS policy
app.UseCors("AllowFrontend");

// Authentication must come BEFORE Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

#endregion

#region ----------------- Ensure Uploads Directory Exists -----------------

// Ensure /uploads folder exists for storing file attachments
var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

#endregion

// Run the web application
app.Run();
