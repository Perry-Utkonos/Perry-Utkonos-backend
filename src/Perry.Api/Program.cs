using Perry.Api.Chat;
using MediatR;
using Perry.Api.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

var localChatEnabled = builder.Configuration.GetValue<bool>("Chat:Local:Enabled");
if (localChatEnabled)
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("ChatLocal разрешён только в Development");
    builder.Configuration.AddJsonFile("appsettings.ChatLocal.json", optional: false, reloadOnChange: false)
        .AddEnvironmentVariables().AddCommandLine(args);
}

builder.WebHost.UseUrls("http://localhost:7000");

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddSingleton<TaskQueue>();
builder.Services.AddHostedService<ScheduleWorker>();

var chatEnabled = builder.Configuration.GetValue<bool>("Chat:Enabled");
if (chatEnabled)
    builder.Services.AddChatModule(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Perry.Api v1");
});


if (chatEnabled)
    app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
if (chatEnabled)
    app.MapChatModule();
if (chatEnabled && localChatEnabled)
    app.MapLocalChat();

app.Run();
