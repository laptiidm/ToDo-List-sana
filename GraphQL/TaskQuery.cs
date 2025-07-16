using GraphQL.Types;
using Todo_List_3.Enums;
using Todo_List_3.Services; 
using GraphQL; 
using Todo_List_3.Models; 

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
					if (context.RequestServices == null) // make sure we can use the DI container
					{
						context.Errors.Add(new ExecutionError("RequestServices is not available."));
						return Enumerable.Empty<TaskModel>();
					}

					var storageSelectionService = context.RequestServices.GetRequiredService<IStorageSelectionService>();

					if (context.UserContext.TryGetValue("StorageType", out object? storageTypeObj) && storageTypeObj is string storageTypeHeader) // ensure that the  header is passed as a string to the user context
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
				.ResolveAsync(async context => // make sure we can use the DI container
				{
					if (context.RequestServices == null)
					{
						context.Errors.Add(new ExecutionError("RequestServices is not available."));
						return Enumerable.Empty<TaskModel>();
					}

					var storageSelectionService = context.RequestServices.GetRequiredService<IStorageSelectionService>();

					if (context.UserContext.TryGetValue("StorageType", out object? storageTypeObj) && storageTypeObj is string storageTypeHeader)
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

// TaskQuery defines the GraphQL query operations for retrieving task data

