using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DomainLayer.Models;
using InfrastuctureLayer;
using InfrastuctureLayer.Repositorio.Commons;
using ApplicationLayer.Services.TaskServices;
using ApplicationLayer.Services.Reactive;
using ApplicationLayer.Factories;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDbContext<WebApiTaskContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("TaskManagerDB")));

builder.Services.AddScoped<ICommonsProcess<Tareas>, TaskRepository>();
builder.Services.AddScoped<ITaskServices, TaskServices>();
builder.Services.AddSingleton<IReactiveTaskQueueService, ReactiveTaskQueueService>();
builder.Services.AddScoped<ITareaFactory, TareaFactory>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<WebApiTaskContext>();
    ctx.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
