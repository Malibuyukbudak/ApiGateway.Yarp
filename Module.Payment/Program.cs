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
            timestamp = DateTime.UtcNow
        }
    };

    return Results.Json(response);
});

app.Run();