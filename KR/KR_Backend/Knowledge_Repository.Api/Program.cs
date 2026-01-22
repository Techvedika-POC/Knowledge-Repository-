using Application.Interfaces;
using Knowledge_Repository.Application.Implementations.Services;
using Knowledge_Repository.Application.Interfaces;
using Knowledge_Repository.Application.Interfaces.Repositories;
using Knowledge_Repository.Application.Interfaces.Services;
using Knowledge_Repository.Application.Services;
using Knowledge_Repository.Application.Services;
using Knowledge_Repository.Infrastructure.Data;
using Knowledge_Repository.Infrastructure.Repositories;
using Knowledge_Repository.Infrastructure.Repositories;
using Knowledge_Repository.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TrainingPlanParser.Services;
using TrainingPlanParser.Services.Enrichment;
using TrainingPlanParser.Services.Evaluation.Core;
using TrainingPlanParser.Services.Evaluation.Hybrid;
using TrainingPlanParser.Services.Evaluation.MLNet;
using TrainingPlanParser.Services.Evaluation.RuleBased;
using TrainingPlanParser.Services.Pipeline;


AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);


IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);

// ===== DATABASE =====
builder.Services.AddDbContext<Knowledge_Repository_dbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ===== REGISTER REPOSITORIES =====
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IEngagementRepository, EngagementRepository>();
builder.Services.AddScoped<IDomainRepository, DomainRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IKnowledgeItemRepository, KnowledgeItemRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped<IDaySpotlightRepository, DaySpotlightRepository>();
builder.Services.AddScoped<IAttachmentRepository, AttachmentRepository>();
builder.Services.AddScoped<IIdeathonRepository, IdeathonRepository>();
builder.Services.AddScoped<IMentorRepository, MentorRepository>();
builder.Services.AddScoped<IEventTeamInsightRepository, EventTeamInsightRepository>();
builder.Services.AddScoped<IKnowledgeTagRepository, KnowledgeTagRepository>();
builder.Services.AddScoped<IKnowledgeVersionRepository, KnowledgeVersionRepository>();
builder.Services.AddScoped<IEventKnowledgeItemRepository, EventKnowledgeItemRepository>();
builder.Services.AddScoped<IKnowledgeTagRepository, KnowledgeTagRepository>();
builder.Services.AddScoped<IKnowledgeVersionRepository, KnowledgeVersionRepository>();
builder.Services.AddScoped<IPasswordResetRepository, PasswordResetRepository>();
builder.Services.AddScoped<IJuryFinalScoreRepository, JuryFinalScoreRepository>();
builder.Services.AddScoped<IJuryChatRepository, JuryChatRepository>();
builder.Services.AddScoped<IEventJuryRepository, EventJuryRepository>();
builder.Services.AddScoped<ICommunicationRepository, CommunicationRepository>();
builder.Services.AddScoped<IApproverRepository, ApproverRepository>();
builder.Services.AddScoped<ILearningPlanRepository, LearningPlanRepository>();
builder.Services.AddScoped<IWeekRepository, WeekRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
builder.Services.AddScoped<IUserProgressRepository, UserProgressRepository>();

// ===== REGISTER SERVICES =====
builder.Services.AddScoped<IUserProgressService, UserProgressService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IEventRegistrationService, EventRegistrationService>();
builder.Services.AddScoped<IEngagementService, EngagementService>();
builder.Services.AddScoped<IDomainService, DomainService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IKnowledgeItemService, KnowledgeItemService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IDaySpotlightService, DaySpotlightService>();
builder.Services.AddScoped<IFreshPickService, FreshPickService>();
builder.Services.AddScoped<IGlobalSearchService, GlobalSearchService>();
builder.Services.AddScoped<ITopicHighlightService, TopicHighlightService>();
builder.Services.AddScoped<ITrendingService, TrendingService>();
builder.Services.AddScoped<IApproverService, ApproverService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IIdeathonService, IdeathonService>();
builder.Services.AddScoped<IMentorService, MentorService>();
builder.Services.AddScoped<IEventTeamInsightService, EventTeamInsightService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddTransient<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IJuryFinalScoreService,JuryFinalScoreService>();
builder.Services.AddScoped<IJuryChatService, JuryChatService>();
builder.Services.AddScoped<IJuryPanelService, JuryPanelService>();
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserProgressAggregateService, UserProgressAggregateService>();
builder.Services.AddScoped<ILearningPlanService, LearningPlanService>();
builder.Services.AddScoped<IWeekService, WeekService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IManagerRepository, ManagerRepository>();
builder.Services.AddScoped<IManagerService, ManagerService>();

// ENRICHMENT
builder.Services.AddScoped<TrainingPlanEnrichmentProcessor>();
builder.Services.AddScoped<TrainingPlanEnrichmentProcessor>();
builder.Services.AddScoped<MultiPassEnrichmentOrchestrator>();

// EVALUATORS
builder.Services.AddScoped<RuleBasedEvaluator>();
builder.Services.AddScoped<MLNetEvaluator>();
builder.Services.AddScoped<HybridEvaluator>();

// ENRICHMENT

builder.Services.AddScoped<ITrainingPlanIngestionService, TrainingPlanIngestionService>();
builder.Services.AddScoped<ITrainingPlanMappingService, TrainingPlanMappingService>();
builder.Services.AddScoped<ITrainingPlanRepository, TrainingPlanRepository>();

builder.Services.AddScoped<IEnrichmentLlmService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["OpenAI:ApiKey"];

    if (string.IsNullOrWhiteSpace(apiKey))
        throw new Exception("OpenAI API Key is missing for enrichment");

    return new Gpt4oMiniEnrichmentService(apiKey);
});
// ===== EVALUATION STRATEGIES =====

// Rule-based evaluator 
builder.Services.AddScoped<RuleBasedEvaluator>();

// ML.NET evaluator
builder.Services.AddScoped<MLNetEvaluator>();

// Hybrid evaluator (FINAL decision engine)
builder.Services.AddScoped<IEvaluationStrategy>(sp =>
{
    var rules = sp.GetRequiredService<RuleBasedEvaluator>();
    var ml = sp.GetRequiredService<MLNetEvaluator>();

    return new HybridEvaluator(rules, ml);
});


// ===== TRAINING PLAN PARSER PIPELINE =====
builder.Services.AddScoped<DocxParser>();

builder.Services.AddScoped<ILlmService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["Groq:ApiKey"];

    if (string.IsNullOrWhiteSpace(apiKey))
        throw new Exception("Groq API Key is missing");

    return new GroqLlmService(apiKey);
});

builder.Services.AddScoped<RuleBasedEvaluator>();

builder.Services.AddScoped<TrainingPlanProcessingPipeline>();

// ===== CORS =====
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ===== CONTROLLERS =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// ===== SWAGGER =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Knowledge Repository API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer <token>'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ===== JWT AUTH =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                Console.WriteLine("Received token: " + context.Token);
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("JWT validation failed: " + context.Exception);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
var uploadsPhysicalPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads");
Directory.CreateDirectory(uploadsPhysicalPath);
builder.Services.AddScoped<IFileStorageService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<FileStorageService>>();
    return new FileStorageService(uploadsPhysicalPath, logger);
});


// ===== BUILD APP =====
var app = builder.Build();
app.UseStaticFiles();



// ===== SWAGGER UI =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Knowledge Repository API v1");
        c.RoutePrefix = string.Empty;
    });
}

// ===== MIDDLEWARE =====
app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();

// ===== MAP CONTROLLERS =====
app.MapControllers();
app.Run();
