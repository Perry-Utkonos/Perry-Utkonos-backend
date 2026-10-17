// Host/Program.cs
var builder = WebApplication.CreateBuilder(args);

// 1. Register MediatR (Scans all modules for handlers)
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// 2. Register the Queue as a Singleton (shared between API and Worker)
builder.Services.AddSingleton<TaskQueue>();

// 3. Register the Schedule (The Heart) as a Hosted Service
builder.Services.AddHostedService<ScheduleWorker>();

var app = builder.Build();

// 4. Map the Gateway Endpoints
app.MapScrapeEndpoints();

app.Run();
