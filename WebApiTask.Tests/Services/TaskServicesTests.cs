using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using ApplicationLayer.Services.TaskServices;
using ApplicationLayer.Services.Notifications;
using DomainLayer.Models;
using InfrastuctureLayer;
using InfrastuctureLayer.Repositorio.Commons;
using Xunit;


namespace WebApiTask.Tests.Services
{
    public class TaskServicesTests
    {

        private TaskServices CreateService(WebApiTaskContext ctx, Mock<ITaskNotificationService>? notifierMock = null)
        {
            var repo = new TaskRepository(ctx);
            var logger = Mock.Of<ILogger<TaskServices>>();
            notifierMock ??= new Mock<ITaskNotificationService>();
            return new TaskServices(repo, logger, notifierMock.Object);
        }

        private WebApiTaskContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<WebApiTaskContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new WebApiTaskContext(options);
        }

        [Fact]
        public async Task GetTaskByIdAllAsync_InvalidId_ReturnsError()
        {
            await using var ctx = CreateContext();
            var svc = CreateService(ctx);
            var r = await svc.GetTaskByIdAllAsync(0);
            Assert.True(r.Errors.Count > 0);
        }

        [Fact]
        public async Task FiltrarTareasPorEstado_WithEmptyState_ReturnsError()
        {
            await using var ctx = CreateContext();
            var svc = CreateService(ctx);
            var r = await svc.FiltrarTareasPorEstado("");
            Assert.True(r.Errors.Count > 0);
        }

        [Fact]
        public async Task AddTaskAllAsync_ValidTask_SucceedsAndNotifies()
        {
            await using var ctx = CreateContext();
            var notifier = new Mock<ITaskNotificationService>();
            var svc = CreateService(ctx, notifier);
            var tarea = new Tareas { Description = "Test", Status = "Pendiente", DueDate = DateTime.Now.AddDays(1) };
            var r = await svc.AddTaskAllAsync(tarea);
            Assert.True(r.Successful);
            notifier.Verify(n => n.NotifyTaskCreatedAsync(It.IsAny<Tareas>()), Times.Once);
        }

        [Fact]
        public async Task AddTaskAllAsync_InvalidTask_Fails()
        {
            await using var ctx = CreateContext();
            var svc = CreateService(ctx);
            var tarea = new Tareas { Description = "Bad", Status = "", DueDate = DateTime.Now.AddDays(-1) };
            var r = await svc.AddTaskAllAsync(tarea);
            Assert.False(r.Successful);
            Assert.True(r.Errors.Count > 0);
        }
        [Fact]
        public async Task UpdateTaskAllAsync_ValidTask_Succeeds()
        {
            await using var ctx = CreateContext();
            var svc = CreateService(ctx);
            var tarea = new Tareas { Description = "U", Status = "Pendiente", DueDate = DateTime.Now.AddDays(1) };
            await svc.AddTaskAllAsync(tarea);
            tarea.Description = "Updated";
            var r = await svc.UpdateTaskAllAsync(tarea);
            Assert.True(r.Successful);
        }


        [Fact]
        public async Task DeleteTaskAllAsync_NonExisting_ReturnsNotSuccessful()
        {
            await using var ctx = CreateContext();
            var svc = CreateService(ctx);
            var r = await svc.DeleteTaskAllAsync(123);
            Assert.False(r.Successful);
        }

        [Fact]
        public async Task GetTaskAllAsync_ReturnsInsertedTasks()
        {
            await using var ctx = CreateContext();
            await ctx.Tarea.AddAsync(new Tareas { Description = "T1", Status = "Pendiente", DueDate = DateTime.Now.AddDays(1) });
            await ctx.SaveChangesAsync();
            var svc = CreateService(ctx);
            var r = await svc.GetTaskAllAsync();
            Assert.True(r.Successful);
            Assert.Single(r.DataList);
        }
    }
}
