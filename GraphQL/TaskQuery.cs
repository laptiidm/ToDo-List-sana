using GraphQL.Types;
using Todo_List_3.Enums;
using Todo_List_3.Services; // де у тебе IStorageSelectionService
using GraphQL.Resolvers;
using GraphQL;
using Todo_List_3.GraphQL;


namespace Todo_List_3.GraphQL
{
	public class TaskQuery : ObjectGraphType
	{
		private readonly IStorageSelectionService _storageSelectionService;

		public TaskQuery(IStorageSelectionService storageSelectionService)
		{
			_storageSelectionService = storageSelectionService;

			AddField(new FieldType
			{
				Name = "activeTasks",
				Type = typeof(ListGraphType<TaskType>),
				Arguments = new QueryArguments(
					new QueryArgument<NonNullGraphType<GraphQLStorageType>> { Name = "storageType" }
				),
				Resolver = new FuncFieldResolver<object>(context =>
				{
					var storageType = context.GetArgument<StorageType>("storageType");
					var repo = _storageSelectionService.GetRepositoryByStorageType(storageType);
					return repo.GetActiveTasks();
				})
			});

			AddField(new FieldType
			{
				Name = "completedTasks",
				Type = typeof(ListGraphType<TaskType>),
				Arguments = new QueryArguments(
					new QueryArgument<NonNullGraphType<GraphQLStorageType>> { Name = "storageType" }
				),
				Resolver = new FuncFieldResolver<object>(context =>
				{
					var storageType = context.GetArgument<StorageType>("storageType");
					var repo = _storageSelectionService.GetRepositoryByStorageType(storageType);
					return repo.GetCompletedTasks();
				})
			});
		}
	}
}