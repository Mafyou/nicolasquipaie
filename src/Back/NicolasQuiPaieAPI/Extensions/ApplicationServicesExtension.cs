namespace NicolasQuiPaieAPI.Extensions;

public static class ApplicationServicesExtension
{
    /// <summary>
    /// Adds all necessary services for the NicolasQuiPaieAPI application
    /// </summary>
    public static void AddNicolasQuiPaieServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment env)
    {
        // Add services to the container
        services.AddEndpointsApiExplorer();
        // Configuration Swagger - ONLY in Development
        if (env.IsDevelopment())
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Nicolas Qui Paie API",
                    Version = "v1",
                    Description = "API pour la plateforme de démocratie participative Nicolas Qui Paie - Mode Développement",
                    Contact = new OpenApiContact
                    {
                        Name = "Nicolas Qui Paie Team",
                        Email = "contact@MafyouIT.tech"
                    }
                });

                // Configuration JWT pour Swagger (inchangée)
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // SecurityRequirement mise à jour pour .NET 10
                c.AddSecurityRequirement(document =>
                {
                    return new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecuritySchemeReference("Bearer", document), []
                        }
                    };
                });
            });

            // Configuration CORS
            services.AddCors(options =>
        {
            options.AddPolicy("AllowBlazorClient", policy =>
            {
                policy.WithOrigins(
                        "https://localhost:7084", // NicolasQuiPaieWeb HTTPS
                        "http://localhost:5207",  // NicolasQuiPaieWeb HTTP
                        "https://localhost:5001",
                        "http://localhost:5000",
                        "https://localhost:7398", // API dans appsettings
                        "https://localhost:7051",  // API HTTPS
                        "https://happy-ocean-06624de03.2.azurestaticapps.net", // NicolasQuiPaieWeb Azure Static Web App
                        "https://*.azurestaticapps.net",
                        "https://nicolasquipaie.eu"
                      )
                      .SetIsOriginAllowedToAllowWildcardSubdomains() // Allow subdomains
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

            // In development, also allow any localhost
            if (env.IsDevelopment())
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("DevelopmentCors", policy =>
                    {
                        policy.SetIsOriginAllowed(origin =>
                        {
                            if (string.IsNullOrWhiteSpace(origin)) return false;

                            // Allow any localhost origin in development
                            return origin.StartsWith("http://localhost:") || origin.StartsWith("https://localhost:");
                        })
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    });
                });
            }

            // Configuration Entity Framework
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            // 🎯 Use extension methods to configure authentication and authorization
            services.AddNicolasQuiPaieJwtAuthentication(configuration);
            services.AddNicolasQuiPaieAuthorization();

            // Configuration FluentValidation
            services.AddValidatorsFromAssemblyContaining<CreateProposalDtoValidator>();

            // Injection des dépendances - Repositories
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProposalRepository, ProposalRepository>();
            services.AddScoped<IVoteRepository, VoteRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IApiLogRepository, ApiLogRepository>();

            // Injection des dépendances - Services
            services.AddScoped<IProposalService, ProposalService>();
            services.AddScoped<IVotingService, VotingService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
        }
    }
}