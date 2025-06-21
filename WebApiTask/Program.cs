using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
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
using ApplicationLayer.Services.Auth;
using ApplicationLayer.Services.Notifications;
using WebApiTask.Services;
using Microsoft.AspNetCore.SignalR;
using WebApiTask.Hubs;


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDbContext<WebApiTaskContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("TaskManagerDB")));

builder.Services.AddScoped<ICommonsProcess<Tareas>, TaskRepository>();
builder.Services.AddSignalR();

builder.Services.AddScoped<ITaskNotificationService, SignalRTaskNotificationService>();
builder.Services.AddScoped<ITaskServices>(sp =>
{
    var svc = ActivatorUtilities.CreateInstance<TaskServices>(sp);
    svc.OnTaskCreated = sp.GetService<Action<Tareas>>();
    return svc;
});
builder.Services.AddSingleton<IReactiveTaskQueueService, ReactiveTaskQueueService>();
builder.Services.AddScoped<ITareaFactory, TareaFactory>();

builder.Services.AddScoped<IJwtAuthService, JwtAuthService>();

var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

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
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"]
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/taskhub"))
                {
                    context.Token = accessToken;     
                }
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese 'Bearer {token}'"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


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

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TaskHub>("/taskhub");
app.Run();
