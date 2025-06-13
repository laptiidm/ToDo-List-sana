using GraphQL.Types;
using Todo_List_3.Models;

namespace Todo_List_3.GraphQL
{
	public class TaskInputType : InputObjectGraphType<TaskModel>
	{
		public TaskInputType()
		{
			Name = "TaskInput";
			Description = "Вхідні дані для створення задачі";

			Field(x => x.Description);
			Field(x => x.DueDate, nullable: true);
			Field(x => x.CategoryId, nullable: true);
		}
	}
}

