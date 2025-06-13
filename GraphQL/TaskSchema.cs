using GraphQL.Types;


namespace Todo_List_3.GraphQL
{
	public class TaskSchema : Schema
	{
		public TaskSchema(IServiceProvider provider) : base(provider)
		{
			Query = provider.GetRequiredService<TaskQuery>();
			Mutation = provider.GetRequiredService<TaskMutation>();
		}
	}
}

