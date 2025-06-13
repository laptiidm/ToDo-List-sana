using GraphQL.Types;
using Todo_List_3.Models;

namespace Todo_List_3.GraphQL
{
	public class TaskType : ObjectGraphType<TaskModel> // ObjectGraphType, який працює з типом TaskModel
	{
		public TaskType()
		{
			Name = "Task"; //ім'я типу

			Field(x => x.TaskId).Description("Ідентифікатор задачі"); // Field() метод-реєстратор
			Field(x => x.Description, nullable: true).Description("Опис задачі"); // Візьми об’єкт x (який є екземпляром TaskModel) і поверни його властивість Description
			Field(x => x.DueDate, nullable: true).Description("Дедлайн");
			Field(x => x.CategoryId, nullable: true).Description("ID категорії");
			Field(x => x.IsDone).Description("Чи виконано задачу");
			Field(x => x.CreatedAt, nullable: true).Description("Дата створення");
			Field(x => x.CompletedAt, nullable: true).Description("Дата виконання");
			Field(x => x.CategoryName, nullable: true).Description("Назва категорії");
		}
	}
}

