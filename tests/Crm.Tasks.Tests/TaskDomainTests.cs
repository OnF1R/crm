using Crm.Tasks.Domain.Entities;
using Crm.Tasks.Domain.Enums;
using Xunit;
using TaskStatus = Crm.Tasks.Domain.Enums.TaskStatus;

namespace Crm.Tasks.Tests;

public sealed class TaskDomainTests
{
    private static readonly Guid CreatedBy = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AssignedUserId = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SubTaskAuthor = new("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void Create_ShouldInitializeDefaultValues()
    {
        var dueDate = DateTime.UtcNow.AddDays(2);
        var task = TaskItem.Create("Task A", "Description", dueDate, (TaskPriority)3, AssignedUserId, CreatedBy, null, null);

        Assert.Equal("Task A", task.Title);
        Assert.Equal("Description", task.Description);
        Assert.Equal(dueDate, task.DueDate);
        Assert.Equal((TaskPriority)3, task.Priority);
        Assert.Equal((TaskStatus)1, task.Status);
        Assert.Equal(AssignedUserId, task.AssignedUserId);
        Assert.Equal(CreatedBy, task.CreatedBy);
        Assert.Empty(task.Comments);
        Assert.Empty(task.SubTasks);
        Assert.Empty(task.Dependencies);
    }

    [Fact]
    public void AddComment_ShouldAddToCollection()
    {
        var task = TaskItem.Create("Task A", null, null, (TaskPriority)1, AssignedUserId, CreatedBy);
        var comment = TaskComment.Create(task.Id, "Need update", SubTaskAuthor);
        task.AddComment(comment);

        Assert.Single(task.Comments);
        Assert.Equal(comment.Id, task.Comments[0].Id);
        Assert.Equal("Need update", task.Comments[0].Content);
    }

    [Fact]
    public void AddSubTask_And_UpdateSubTaskOrder_ShouldSetOrderAndLinkParent()
    {
        var task = TaskItem.Create("Task A", null, null, (TaskPriority)1, AssignedUserId, CreatedBy);
        var first = task.AddSubTask("Sub A", null, null, (TaskPriority)2, AssignedUserId, CreatedBy, 10);
        var second = task.AddSubTask("Sub B", null, null, (TaskPriority)1, AssignedUserId, CreatedBy, 20);

        Assert.Equal(task.Id, first.ParentTaskId);
        Assert.Equal(task.Id, second.ParentTaskId);
        Assert.Equal(2, task.SubTasks.Count);
        Assert.Equal((TaskStatus)1, first.Status);
        Assert.Equal((TaskStatus)1, second.Status);

        task.UpdateSubTaskOrder();

        Assert.Equal(0, task.SubTasks[0].Order);
        Assert.Equal(1, task.SubTasks[1].Order);
    }

    [Fact]
    public void RemoveSubTask_ShouldDeleteOnlyMatched()
    {
        var task = TaskItem.Create("Task A", null, null, (TaskPriority)1, AssignedUserId, CreatedBy);
        var keep = task.AddSubTask("Keep", null, null, (TaskPriority)1, AssignedUserId, CreatedBy);
        var remove = task.AddSubTask("Remove", null, null, (TaskPriority)1, AssignedUserId, CreatedBy);

        task.RemoveSubTask(remove.Id);

        Assert.Single(task.SubTasks);
        Assert.Equal(keep.Id, task.SubTasks[0].Id);
    }

    [Fact]
    public void AddDependency_ShouldAddAndRemoveDependency()
    {
        var task = TaskItem.Create("Task A", null, null, (TaskPriority)1, AssignedUserId, CreatedBy);
        var predecessor = Guid.NewGuid();

        var dependency = task.AddDependency(predecessor, DependencyType.StartToStart);
        Assert.Single(task.Dependencies);
        Assert.Equal(predecessor, dependency.PredecessorTaskId);
        Assert.Equal(task.Id, dependency.SuccessorTaskId);
        Assert.Equal(DependencyType.StartToStart, dependency.Type);

        task.RemoveDependency(dependency.Id);
        Assert.Empty(task.Dependencies);
    }

    [Fact]
    public void SetStatus_ShouldUpdate()
    {
        var task = TaskItem.Create("Task A", null, null, (TaskPriority)1, AssignedUserId, CreatedBy);
        task.SetStatus((TaskStatus)4);
        Assert.Equal((TaskStatus)4, task.Status);
    }

    [Fact]
    public void Create_SubTask_ShouldSetDefaultsAndCreatedFields()
    {
        var started = DateTime.UtcNow;
        var subTask = SubTask.Create(Guid.NewGuid(), "SubTask", "Desc", null, (TaskPriority)3, AssignedUserId, CreatedBy, 5);

        Assert.Equal("SubTask", subTask.Title);
        Assert.Equal("Desc", subTask.Description);
        Assert.Equal((TaskPriority)3, subTask.Priority);
        Assert.Equal((TaskStatus)1, subTask.Status);
        Assert.Equal(5, subTask.Order);
        Assert.Equal(CreatedBy, subTask.CreatedBy);
        Assert.InRange(subTask.CreatedAt, started, DateTime.UtcNow);
    }

    [Fact]
    public void SubTask_Update_And_SetStatus_ShouldWork()
    {
        var subTask = SubTask.Create(Guid.NewGuid(), "SubTask", null, null, (TaskPriority)1, AssignedUserId, CreatedBy, 0);
        subTask.Update("Updated", "upd", null, (TaskPriority)4, AssignedUserId, 7);
        subTask.SetStatus((TaskStatus)5);

        Assert.Equal("Updated", subTask.Title);
        Assert.Equal("upd", subTask.Description);
        Assert.Equal((TaskPriority)4, subTask.Priority);
        Assert.Equal(7, subTask.Order);
        Assert.Equal((TaskStatus)5, subTask.Status);
    }
}
