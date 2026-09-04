using Microsoft.OpenApi.Models;

namespace CPG.Api.Infrastructure;

/// <summary>OpenAPI / Swagger generation with a JWT bearer security scheme (SPEC.md section 1).</summary>
public static class SwaggerExtensions
{
    public static IServiceCollection AddCpgSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CPG Enterprises Logistics Platform API",
                Version = "v1",
                Description = "Clean Architecture API for CPG Enterprises of Orlando (SPEC.md).",
            });

            var scheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            };

            options.AddSecurityDefinition("Bearer", scheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = [] });
        });

        return services;
    }
}
