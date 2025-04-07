using Api.Gateway.Extension;

var builder = WebApplication.CreateBuilder(args);

//builder.YarpLoadFromMemoryBuilder();
builder.YarpLoadFromConfigBuilder();
builder.YarpAuthenticationBuilder();
builder.YarpCacheBuilder();
builder.YarpRateLimitBuilder();

var app = builder.Build();

app.UseCustomAuthentication();
app.MapReverseProxy();
app.UseOutputCache();
app.UseHttpsRedirection();
app.UseRateLimiter();
app.Run();