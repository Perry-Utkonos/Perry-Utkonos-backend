using MediatR;
using Scrapper.Contracts; // 👈 Указываем конкретный namespace (или Scrapper.Application, если нужен он)
using Perry.Api.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:7000");

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);

    cfg.RegisterServicesFromAssembly(typeof(Scrapper.Application.UpdateDailyHoroscopesHandler).Assembly);
});


builder.Services.AddSingleton<UserSessionBuffer>();

builder.Services.AddHostedService<SessionCleanupWorker>();

builder.Services.AddSingleton<TaskQueue>();
builder.Services.AddHostedService<ScheduleWorker>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Perry.Api v1");
});

app.UseAuthorization();
app.MapControllers();

app.Run();
