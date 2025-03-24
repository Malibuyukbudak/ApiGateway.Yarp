using Yarp.ReverseProxy.Configuration;

namespace Api.Gateway.Extension;

public static class BuilderExtension
{
    public static WebApplicationBuilder LoadFromMemory(this WebApplicationBuilder builder)
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
                ClusterId = "paymentCluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "paymentService", new DestinationConfig { Address = "https://localhost:7137" } }
                }
            }
        };

        builder.Services.AddReverseProxy()
            .LoadFromMemory(routes, clusters);
        return builder;
    }
    
    public static WebApplicationBuilder LoadFromConfig(this WebApplicationBuilder builder)
    {
         builder.Services
            .AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
         return builder;
    }
}