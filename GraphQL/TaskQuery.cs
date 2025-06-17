using GraphQL.Types;
using Todo_List_3.Enums;
using Todo_List_3.Services; // Для IStorageSelectionService
using GraphQL; // Для IResolveFieldContext та ExecutionError
using Todo_List_3.Repositories; // Для ITaskRepository
using System.Threading.Tasks; // Для Task<IEnumerable<T>>
using System.Linq; // Для Enumerable.Empty
using Todo_List_3.Models; // Для TaskModel

namespace Todo_List_3.GraphQL
{
	public class TaskQuery : ObjectGraphType
	{
		public TaskQuery()
		{
			Name = "Query";

			Field<ListGraphType<TaskType>>("activeTasks")
				.Description("Gets all active tasks based on the storage type specified in the HTTP header.")
				.ResolveAsync(async context =>
				{
					// Fix: Add null check for RequestServices
					if (context.RequestServices == null)
					{
						context.Errors.Add(new ExecutionError("RequestServices is not available."));
						return Enumerable.Empty<TaskModel>();
					}

					var storageSelectionService = context.RequestServices.GetRequiredService<IStorageSelectionService>();

					if (context.UserContext.TryGetValue("StorageTypeHeader", out object? storageTypeObj) && storageTypeObj is string storageTypeHeader)
					{
						if (Enum.TryParse<StorageType>(storageTypeHeader, true, out var storageType))
						{
							var repo = storageSelectionService.GetRepositoryByStorageType(storageType);
							return await repo.GetActiveTasks();
						}
					}

					context.Errors.Add(new ExecutionError("HTTP header 'X-Storage-Type' is missing or invalid. Expected 'XML' or 'Database'."));
					return Enumerable.Empty<TaskModel>();
				});

			Field<ListGraphType<TaskType>>("completedTasks")
				.Description("Gets all completed tasks based on the storage type specified in the HTTP header.")
				.ResolveAsync(async context =>
				{
					// Fix: Add null check for RequestServices
					if (context.RequestServices == null)
					{
						context.Errors.Add(new ExecutionError("RequestServices is not available."));
						return Enumerable.Empty<TaskModel>();
					}

					var storageSelectionService = context.RequestServices.GetRequiredService<IStorageSelectionService>();

					if (context.UserContext.TryGetValue("StorageTypeHeader", out object? storageTypeObj) && storageTypeObj is string storageTypeHeader)
					{
						if (Enum.TryParse<StorageType>(storageTypeHeader, true, out var storageType))
						{
							var repo = storageSelectionService.GetRepositoryByStorageType(storageType);
							return await repo.GetCompletedTasks();
						}
					}

					context.Errors.Add(new ExecutionError("HTTP header 'X-Storage-Type' is missing or invalid. Expected 'XML' or 'Database'."));
					return Enumerable.Empty<TaskModel>();
				});
		}
	}
}