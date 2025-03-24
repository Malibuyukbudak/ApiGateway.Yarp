using Api.Gateway.Extension;

var builder = WebApplication.CreateBuilder(args);


builder.LoadFromMemory();
//builder.LoadFromConfig();

var app = builder.Build();

app.MapReverseProxy();
app.UseHttpsRedirection();
app.Run();