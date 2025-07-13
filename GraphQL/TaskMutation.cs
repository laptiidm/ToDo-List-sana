using GraphQL.Types;
using Todo_List_3.Enums;
using Todo_List_3.Models;
using Todo_List_3.Services; 
using GraphQL;

namespace Todo_List_3.GraphQL
{
	public class TaskMutation : ObjectGraphType
	{
		public TaskMutation()
		{
			Name = "Mutation"; 

			// --- add Task ---
			Field<TaskType>("addTask") 
				.Description("Adds a new task to the selected storage.")
				.Arguments(
					new QueryArgument<NonNullGraphType<TaskInputType>> { Name = "task" }
				)
				.ResolveAsync(async context => 
				{
					if (context.RequestServices == null)
					{
						context.Errors.Add(new ExecutionError("RequestServices is not available."));
						return null;
					}

					var storageSelectionService = context.RequestServices.GetRequiredService<IStorageSelectionService>();

					// try to get header
					if (!context.UserContext.TryGetValue("StorageTypeHeader", out object? storageTypeObj) || !(storageTypeObj is string storageTypeHeader) || !Enum.TryParse<StorageType>(storageTypeHeader, true, out var storageType))
					{
						context.Errors.Add(new ExecutionError("HTTP header 'X-Storage-Type' is missing or invalid. Expected 'XML' or 'Database'."));
						return null;
					}

					var taskInput = context.GetArgument<TaskModel>("task");
					var repo = storageSelectionService.GetRepositoryByStorageType(storageType);

					var task = new TaskModel
					{
						Description = taskInput.Description,
						DueDate = taskInput.DueDate,
						CategoryId = taskInput.CategoryId,
						IsDone = false, 
						CreatedAt = DateTime.Now 
					};

					await repo.AddTask(task);

					return task; // it might make sense to return bool instead of task
				});

			// --- mark Task As Done ---
			Field<BooleanGraphType>("markTaskAsDone") 
				.Description("Marks a task as done by its ID in the selected storage.")
				.Arguments(
					new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "taskId" }
				)
				.ResolveAsync(async context => 
				{
					if (context.RequestServices == null)
					{
						context.Errors.Add(new ExecutionError("RequestServices is not available."));
						return false;
					}

					var storageSelectionService = context.RequestServices.GetRequiredService<IStorageSelectionService>();

					if (!context.UserContext.TryGetValue("StorageTypeHeader", out object? storageTypeObj) || !(storageTypeObj is string storageTypeHeader) || !Enum.TryParse<StorageType>(storageTypeHeader, true, out var storageType))
					{
						context.Errors.Add(new ExecutionError("HTTP header 'X-Storage-Type' is missing or invalid. Expected 'XML' or 'Database'."));
						return false;
					}

					var taskId = context.GetArgument<int>("taskId");
					var repo = storageSelectionService.GetRepositoryByStorageType(storageType);

					await repo.MarkTaskAsDone(taskId);
					return true; 
				});
		}
	}
}