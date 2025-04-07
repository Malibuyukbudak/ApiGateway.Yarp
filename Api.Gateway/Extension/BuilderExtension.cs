using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.LoadBalancing;

namespace Api.Gateway.Extension;

public static class BuilderExtension
{
    public static WebApplicationBuilder YarpLoadFromMemoryBuilder(this WebApplicationBuilder builder)
    {
        var routes = new[]
        {
            new RouteConfig
            {
                RouteId = "identityRoute",
                ClusterId = "authCluster",
                Match = new RouteMatch { Path = "/identity/{**catch-all}" }
            },
            new RouteConfig
            {
                AuthorizationPolicy = "authPolicy",
                RouteId = "paymentRoute",
                ClusterId = "paymentCluster",
                Match = new RouteMatch { Path = "/payment/{**catch-all}" }
            }
        };

        var clusters = new[]
        {
            new ClusterConfig
            {
                ClusterId = "authCluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "authService", new DestinationConfig { Address = "https://localhost:7276" } }
                }
            },
            new ClusterConfig
            {
                ClusterId = "paymentCluster", Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "Payment1", new DestinationConfig { Address = "https://localhost:7137" } },
                    { "Payment2", new DestinationConfig { Address = "https://localhost:7138" } },
                    { "Payment3", new DestinationConfig { Address = "https://localhost:7139" } }
                },
                LoadBalancingPolicy = LoadBalancingPolicies.RoundRobin,
            }
        };

        builder.Services.AddReverseProxy()
            .LoadFromMemory(routes, clusters);
        return builder;
    }

    public static WebApplicationBuilder YarpLoadFromConfigBuilder(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
        return builder;
    }

    public static WebApplicationBuilder YarpAuthenticationBuilder(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey
                        (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("authPolicy", policy => policy.RequireAuthenticatedUser());
        });

        return builder;
    }

    public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    public static WebApplicationBuilder YarpCacheBuilder(this WebApplicationBuilder builder)
    {
        builder.Services.AddOutputCache(options =>
        {
            options.AddPolicy("cachePolicy", builder =>
                builder.Expire(TimeSpan.FromSeconds(1)));
        });
        return builder;
    }

    public static WebApplicationBuilder YarpRateLimitBuilder(this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("rateLimitPolicy", opt =>
            {
                opt.PermitLimit = 4;
                opt.Window = TimeSpan.FromSeconds(12);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 2;
            });
        });

        return builder;
    }
}