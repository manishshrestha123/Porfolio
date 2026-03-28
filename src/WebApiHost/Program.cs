using Application.Abstractions;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Add services for Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Enable Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // Makes Swagger UI available at root URL
    });
}

app.UseHttpsRedirection();
app.UseCors();

app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/api/contact", async (
        ContactFormRequest dto,
        PortfolioDbContext db,
        IContactSmtpNotifier smtpNotifier,
        HttpContext http,
        CancellationToken cancellationToken) =>
    {
        if (string.IsNullOrWhiteSpace(dto.Name)
            || string.IsNullOrWhiteSpace(dto.Email)
            || string.IsNullOrWhiteSpace(dto.Message))
        {
            return Results.BadRequest(new { error = "Name, email, and message are required." });
        }

        var entity = new ContactMessage
        {
            Id = Guid.NewGuid(),
            VisitorName = dto.Name.Trim(),
            VisitorEmail = dto.Email.Trim(),
            Subject = string.IsNullOrWhiteSpace(dto.Subject) ? null : dto.Subject.Trim(),
            MessageBody = dto.Message.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            SubmitterIpAddress = http.Connection.RemoteIpAddress?.ToString(),
            AdminNotificationStatus = EmailNotificationStatus.Pending,
        };

        db.ContactMessages.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        var send = await smtpNotifier.SendAsync(entity, cancellationToken);
        entity.AdminNotificationCompletedAtUtc = DateTime.UtcNow;
        if (send.Sent)
        {
            entity.AdminNotificationStatus = EmailNotificationStatus.Sent;
            entity.AdminNotificationError = null;
        }
        else
        {
            entity.AdminNotificationStatus = EmailNotificationStatus.Failed;
            entity.AdminNotificationError = send.ErrorMessage;
        }

        await db.SaveChangesAsync(cancellationToken);

        var emailStatus = send.Sent ? "sent" : "failed";
        return Results.Created($"/api/contact/{entity.Id}", new { id = entity.Id, emailNotification = emailStatus });
    })
    .WithName("PostContact")
    .WithTags("Contact");

// Automatically open browser to Swagger UI
if (app.Environment.IsDevelopment())
{
    var url = "https://localhost:7015/swagger/index.html";
    _ = Task.Run(() =>
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch { /* ignore if fails */ }
    });
}

app.Run();

record ContactFormRequest(string Name, string Email, string? Subject, string Message);

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
