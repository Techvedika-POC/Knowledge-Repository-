using KnowLedger_Synaptix.Models;
using KnowLedger_Synaptix.Services;
using KnowLedger_Synaptix.Services.Implementations;
using KnowLedger_Synaptix.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<Knowledge_Repository_dbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services
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
builder.Services.AddScoped<IActivityLogService , ActivityLogService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:3000", "https://knowledge-frontend-n567.onrender.com") 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Controllers
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

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

// Authorization
builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();      // Authentication BEFORE authorizatione
app.UseAuthorization();

app.MapControllers();
app.Run();
