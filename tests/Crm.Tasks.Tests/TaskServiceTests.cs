using Crm.Shared.Domain;
using Crm.Tasks.Application.DTOs;
using Crm.Tasks.Application.Interfaces;
using Crm.Tasks.Application.Services;
using Crm.Tasks.Domain.Entities;
using Crm.Tasks.Domain.Enums;
using Crm.Tasks.Domain.Interfaces;
using Xunit;
using TaskStatus = Crm.Tasks.Domain.Enums.TaskStatus;

namespace Crm.Tasks.Tests;

public sealed class TaskServiceTests
{
    private static readonly Guid CreatedBy = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Assignee = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid OtherUser = new("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task CreateAsync_ShouldCreateTaskAndReturnDto()
    {
        var taskRepo = new InMemoryTaskRepository();
        var subTaskRepo = new InMemorySubTaskRepository();
        var dependencyRepo = new InMemoryTaskDependencyRepository();
        var uow = new FakeUnitOfWork();
        var service = new TaskService(taskRepo, subTaskRepo, dependencyRepo, uow);

        var dto = new CreateTaskDto("Task A", "Desc", DateTime.UtcNow.AddDays(1), (int)(TaskPriority)3, Assignee);

        var result = await service.CreateAsync(dto, CreatedBy, default);

        Assert.Equal("Task A", result.Title);
        Assert.Equal((TaskPriority)3, Enum.Parse<TaskPriority>(result.Priority));
        Assert.NotEqual(string.Empty, result.Status);
        Assert.True(uow.SaveCount > 0);
        Assert.Single(await taskRepo.GetAllAsync());
    }

    [Fact]
    public async Task SetStatusAsync_ShouldPersistAndReturnNewStatus()
    {
        var taskRepo = new InMemoryTaskRepository();
        var subTaskRepo = new InMemorySubTaskRepository();
        var dependencyRepo = new InMemoryTaskDependencyRepository();
        var uow = new FakeUnitOfWork();
        var service = new TaskService(taskRepo, subTaskRepo, dependencyRepo, uow);

        var task = TaskItem.Create("Task A", null, null, (TaskPriority)1, Assignee, CreatedBy);
        taskRepo.AddInitial(task);
        await taskRepo.AddAsync(task);

        var result = await service.SetStatusAsync(task.Id, new SetTaskStatusDto((int)TaskStatus.ВРаботе), default);

        Assert.Equal(((TaskStatus)2).ToString(), result.Status);
        Assert.Equal(task.Id, result.Id);
        Assert.Equal((TaskStatus)2, task.Status);
    }

    [Fact]
    public async Task AddSubTaskAsync_ShouldPersistSubTask()
    {
        var taskRepo = new InMemoryTaskRepository();
        var subTaskRepo = new InMemorySubTaskRepository();
        var dependencyRepo = new InMemoryTaskDependencyRepository();
        var uow = new FakeUnitOfWork();
        var service = new TaskService(taskRepo, subTaskRepo, dependencyRepo, uow);

        var parent = TaskItem.Create("Task A", null, null, (TaskPriority)1, Assignee, CreatedBy);
        await taskRepo.AddAsync(parent);

        var result = await service.AddSubTaskAsync(parent.Id, new CreateSubTaskDto("Sub", null, null, (int)(TaskPriority)2, Assignee, 5), OtherUser);

        Assert.Equal(parent.Id, result.ParentTaskId);
        Assert.Equal("Sub", result.Title);
        Assert.Single(await subTaskRepo.GetAllAsync());
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task AddSubTaskAsync_ShouldThrowWhenParentMissing()
    {
        var taskRepo = new InMemoryTaskRepository();
        var subTaskRepo = new InMemorySubTaskRepository();
        var dependencyRepo = new InMemoryTaskDependencyRepository();
        var service = new TaskService(taskRepo, subTaskRepo, dependencyRepo, new FakeUnitOfWork());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.AddSubTaskAsync(Guid.NewGuid(), new CreateSubTaskDto("Sub", null, null, 1, Assignee, 0), OtherUser));
    }

    [Fact]
    public async Task UpdateSubTasksOrderAsync_ShouldNormalizeOrder()
    {
        var taskRepo = new InMemoryTaskRepository();
        var subTaskRepo = new InMemorySubTaskRepository();
        var dependencyRepo = new InMemoryTaskDependencyRepository();
        var uow = new FakeUnitOfWork();
        var service = new TaskService(taskRepo, subTaskRepo, dependencyRepo, uow);

        var parent = TaskItem.Create("Task A", null, null, (TaskPriority)1, Assignee, CreatedBy);
        parent.AddSubTask("One", null, null, (TaskPriority)1, Assignee, CreatedBy, 10);
        parent.AddSubTask("Two", null, null, (TaskPriority)1, Assignee, CreatedBy, 20);
        parent.AddSubTask("Three", null, null, (TaskPriority)1, Assignee, CreatedBy, 30);
        await taskRepo.AddAsync(parent);

        await service.UpdateSubTasksOrderAsync(parent.Id);

        Assert.Equal(0, parent.SubTasks[0].Order);
        Assert.Equal(1, parent.SubTasks[1].Order);
        Assert.Equal(2, parent.SubTasks[2].Order);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task AddDependencyAsync_ShouldCreateDependency()
    {
        var taskRepo = new InMemoryTaskRepository();
        var subTaskRepo = new InMemorySubTaskRepository();
        var dependencyRepo = new InMemoryTaskDependencyRepository();
        var uow = new FakeUnitOfWork();
        var service = new TaskService(taskRepo, subTaskRepo, dependencyRepo, uow);

        var successor = TaskItem.Create("Task A", null, null, (TaskPriority)1, Assignee, CreatedBy);
        var predecessor = TaskItem.Create("Task B", null, null, (TaskPriority)2, Assignee, CreatedBy);
        await taskRepo.AddAsync(successor);
        await taskRepo.AddAsync(predecessor);

        var result = await service.AddDependencyAsync(successor.Id, new CreateTaskDependencyDto(predecessor.Id, (int)DependencyType.StartToStart));

        Assert.Equal(predecessor.Id, result.PredecessorTaskId);
        Assert.Equal(successor.Id, result.SuccessorTaskId);
        Assert.Equal(DependencyType.StartToStart.ToString(), result.Type);
        Assert.Single(dependencyRepo.Items);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task AddDependencyAsync_ShouldThrow_WhenSameTask()
    {
        var taskRepo = new InMemoryTaskRepository();
        var subTaskRepo = new InMemorySubTaskRepository();
        var dependencyRepo = new InMemoryTaskDependencyRepository();
        var service = new TaskService(taskRepo, subTaskRepo, dependencyRepo, new FakeUnitOfWork());
        var task = TaskItem.Create("Task A", null, null, (TaskPriority)1, Assignee, CreatedBy);
        await taskRepo.AddAsync(task);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddDependencyAsync(task.Id, new CreateTaskDependencyDto(task.Id, (int)DependencyType.FinishToFinish)));
    }

    [Fact]
    public async Task AddCommentAsync_ShouldAddToTask()
    {
        var taskRepo = new InMemoryTaskRepository();
        var subTaskRepo = new InMemorySubTaskRepository();
        var dependencyRepo = new InMemoryTaskDependencyRepository();
        var uow = new FakeUnitOfWork();
        var service = new TaskService(taskRepo, subTaskRepo, dependencyRepo, uow);

        var task = TaskItem.Create("Task A", null, null, (TaskPriority)1, Assignee, CreatedBy);
        await taskRepo.AddAsync(task);

        var result = await service.AddCommentAsync(task.Id, new CreateCommentDto("Looks good"), OtherUser);

        Assert.Equal(task.Id, result.TaskId);
        Assert.Equal("Looks good", result.Content);
        Assert.Equal(1, task.Comments.Count);
        Assert.Equal(1, uow.SaveCount);
    }

    [Fact]
    public async Task GetDependenciesAsync_ShouldReturnMappedDtos()
    {
        var taskRepo = new InMemoryTaskRepository();
        var subTaskRepo = new InMemorySubTaskRepository();
        var dependencyRepo = new InMemoryTaskDependencyRepository();
        var service = new TaskService(taskRepo, subTaskRepo, dependencyRepo, new FakeUnitOfWork());
        var task = TaskItem.Create("Task A", null, null, (TaskPriority)1, Assignee, CreatedBy);
        await taskRepo.AddAsync(task);
        var dependency = TaskDependency.Create(Guid.NewGuid(), task.Id, DependencyType.StartToStart);
        dependencyRepo.Items.Add(dependency);

        var dependencies = await service.GetDependenciesAsync(task.Id);

        Assert.Single(dependencies);
        Assert.Equal(dependency.Id, dependencies[0].Id);
        Assert.Equal(DependencyType.StartToStart.ToString(), dependencies[0].Type);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
        public void Dispose() { }
    }

    private sealed class InMemoryTaskRepository : ITaskRepository
    {
        private readonly List<TaskItem> _tasks = [];
        public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            await Task.FromResult(_tasks.FirstOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<TaskItem>)_tasks.ToList());

        public Task<IReadOnlyList<TaskItem>> FindAsync(ISpecification<TaskItem> specification, CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<TaskItem>)_tasks.ToList());

        public Task AddAsync(TaskItem entity, CancellationToken ct = default)
        {
            _tasks.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(TaskItem entity, CancellationToken ct = default) => Task.CompletedTask;

        public Task DeleteAsync(TaskItem entity, CancellationToken ct = default)
        {
            _tasks.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TaskItem>> GetByAssignedUserIdAsync(Guid userId, CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<TaskItem>)_tasks.Where(x => x.AssignedUserId == userId).ToList());

        public Task<IReadOnlyList<TaskItem>> GetByRelatedEntityAsync(RelatedEntityType entityType, Guid entityId, CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<TaskItem>)_tasks.Where(x => x.RelatedEntityType == entityType && x.RelatedEntityId == entityId).ToList());

        public Task<TaskItem?> GetWithCommentsAsync(Guid id, CancellationToken ct = default) =>
            GetByIdAsync(id, ct);

        public Task AddCommentAsync(TaskComment comment, CancellationToken ct = default)
        {
            var task = _tasks.FirstOrDefault(x => x.Id == comment.TaskId);
            task?.AddComment(comment);
            return Task.CompletedTask;
        }

        public void AddInitial(TaskItem task) => _tasks.Add(task);
    }

    private sealed class InMemorySubTaskRepository : ISubTaskRepository
    {
        private readonly List<SubTask> _subtasks = [];
        public Task<SubTask?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(_subtasks.FirstOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<SubTask>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<SubTask>)_subtasks.ToList());

        public Task<IReadOnlyList<SubTask>> FindAsync(ISpecification<SubTask> specification, CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<SubTask>)_subtasks.ToList());

        public Task AddAsync(SubTask entity, CancellationToken ct = default)
        {
            _subtasks.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(SubTask entity, CancellationToken ct = default) => Task.CompletedTask;

        public Task DeleteAsync(SubTask entity, CancellationToken ct = default)
        {
            _subtasks.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<SubTask>> GetByParentTaskIdAsync(Guid parentTaskId, CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<SubTask>)_subtasks.Where(x => x.ParentTaskId == parentTaskId).ToList());
    }

    private sealed class InMemoryTaskDependencyRepository : ITaskDependencyRepository
    {
        public readonly List<TaskDependency> Items = [];
        public Task<TaskDependency?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

        public Task<IReadOnlyList<TaskDependency>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<TaskDependency>)Items.ToList());

        public Task<IReadOnlyList<TaskDependency>> FindAsync(ISpecification<TaskDependency> specification, CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<TaskDependency>)Items.ToList());

        public Task AddAsync(TaskDependency entity, CancellationToken ct = default)
        {
            Items.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(TaskDependency entity, CancellationToken ct = default) => Task.CompletedTask;

        public Task DeleteAsync(TaskDependency entity, CancellationToken ct = default)
        {
            Items.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TaskDependency>> GetByTaskIdAsync(Guid taskId, CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<TaskDependency>)Items.Where(x => x.SuccessorTaskId == taskId).ToList());

        public Task<IReadOnlyList<TaskDependency>> GetPredecessorsAsync(Guid taskId, CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<TaskDependency>)Items.Where(x => x.SuccessorTaskId == taskId).ToList());

        public Task<IReadOnlyList<TaskDependency>> GetSuccessorsAsync(Guid taskId, CancellationToken ct = default) =>
            Task.FromResult((IReadOnlyList<TaskDependency>)Items.Where(x => x.PredecessorTaskId == taskId).ToList());
    }
}
