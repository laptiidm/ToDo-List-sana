using GraphQL.Types;
using Todo_List_3.Enums;
using Todo_List_3.Models;
using Todo_List_3.Services;
using GraphQL;
using Todo_List_3.GraphQL;
using GraphQL.Resolvers;

namespace Todo_List_3.GraphQL
{
	public class TaskMutation : ObjectGraphType
	{
		private readonly IStorageSelectionService _storageSelectionService;

		public TaskMutation(IStorageSelectionService storageSelectionService)
		{
			_storageSelectionService = storageSelectionService;

			AddField(new FieldType
			{
				Name = "addTask",
				Type = typeof(TaskType),
				Arguments = new QueryArguments(
					new QueryArgument<NonNullGraphType<TaskInputType>> { Name = "task" },
					new QueryArgument<NonNullGraphType<GraphQLStorageType>> { Name = "storageType" }
				),
				Resolver = new FuncFieldResolver<object>(context =>
				{
					var taskInput = context.GetArgument<TaskModel>("task");
					var storageType = context.GetArgument<StorageType>("storageType");

					var repo = _storageSelectionService.GetRepositoryByStorageType(storageType);

					var task = new TaskModel
					{
						Description = taskInput.Description,
						DueDate = taskInput.DueDate,
						CategoryId = taskInput.CategoryId,
						IsDone = false,
						CreatedAt = DateTime.Now
					};

					repo.AddTask(task);
					return task;
				})
			});

			AddField(new FieldType
			{
				Name = "markTaskAsDone",
				Type = typeof(BooleanGraphType),
				Arguments = new QueryArguments(
					new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "taskId" },
					new QueryArgument<NonNullGraphType<GraphQLStorageType>> { Name = "storageType" }
				),
				Resolver = new FuncFieldResolver<object>(context =>
				{
					var taskId = context.GetArgument<int>("taskId");
					var storageType = context.GetArgument<StorageType>("storageType");

					var repo = _storageSelectionService.GetRepositoryByStorageType(storageType);
					repo.MarkTaskAsDone(taskId);
					return true;
				})
			});
		}
	}
}