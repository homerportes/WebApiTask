using ApplicationLayer.Factories;
using ApplicationLayer.Services.TaskServices;
using DomainLayer.Models;
using InfrastuctureLayer;
using InfrastuctureLayer.Repositorio.Commons;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WebApiTaskContext>(options =>
{

    options.UseSqlServer(builder.Configuration.GetConnectionString("TaskManagerDB"));

});


builder.Services.AddScoped<ICommonsProcess<Tareas>, TaskRepository>();
builder.Services.AddScoped<TaskServices>();
builder.Services.AddScoped<ITareaFactory, TareaFactory>();

builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WebApiTaskContext>();
    context.Database.Migrate();
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
