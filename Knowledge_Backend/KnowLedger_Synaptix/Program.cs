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

// ----------------- Database Context -----------------
builder.Services.AddDbContext<Knowledge_Repository_dbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ----------------- Application Services -----------------
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

// ----------------- HTTP Client (Embedding + Qdrant) -----------------
builder.Services.AddHttpClient<IEmbeddingService, EmbeddingService>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromMinutes(2));

builder.Services.AddHttpClient("QdrantClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:6333/");
    client.Timeout = TimeSpan.FromMinutes(2);
});

// ----------------- CORS Configuration -----------------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            allowedOrigins ?? new[] {
                "http://localhost:3000",
                "https://knowledge-frontend-n567.onrender.com"
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ----------------- Controllers & JSON -----------------
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// ----------------- JWT Authentication -----------------
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

// ----------------- Swagger -----------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ----------------- Build App -----------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure /uploads folder exists
var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Serve static files and uploads
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// Middleware pipeline
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
