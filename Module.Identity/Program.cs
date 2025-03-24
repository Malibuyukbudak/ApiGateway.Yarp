var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("identity/login", () =>
{
    var response = new
    {
        success = true,
        data = new
        {
            userId = 12345,
            username = "testuser",
            email = "testuser@example.com",
            token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
            expiresIn = 3600
        }
    };
    
    return Results.Json(response);
});

app.Run();
