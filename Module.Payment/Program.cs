using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapGet("/payment/execute", () =>
{
    var response = new
    {
        success = true,
        message = "Payment successful",
        data = new
        {
            transactionId = Guid.NewGuid().ToString(),
            amount = 150.75,
            currency = "TRY",
            paymentMethod = "Credit Card",
            status = "Completed",
            timestamp = DateTime.UtcNow,
        },
    };

    return Results.Json(response);
}).RequireAuthorization();

app.Use(async (context, next) =>
{
    Console.WriteLine($"Payment Service Çalıştı - Sunucu: {context.Request.Host}");
    await next();
});

app.Run();