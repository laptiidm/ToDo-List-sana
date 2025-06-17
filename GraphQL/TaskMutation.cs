using GraphQL.Types;
using Todo_List_3.Enums;
using Todo_List_3.Models;
using Todo_List_3.Services; // Для IStorageSelectionService
using GraphQL;
using Todo_List_3.GraphQL;
using System.Threading.Tasks; // Додаємо для асинхронних операцій
using System.Linq; // Для Enumerable.Empty

namespace Todo_List_3.GraphQL
{
	public class TaskMutation : ObjectGraphType
	{
		public TaskMutation()
		{
			Name = "Mutation"; // Зазвичай для мутацій Name = "Mutation"

			// --- add Task ---
			Field<TaskType>("addTask") // Визначаємо поле 'addTask'
				.Description("Adds a new task to the selected storage.")
				.Arguments(
					new QueryArgument<NonNullGraphType<TaskInputType>> { Name = "task" }
				)
				.ResolveAsync(async context => // Використовуємо ResolveAsync для асинхронного резолвера
				{
					// Перевірка наявності RequestServices (для надійності)
					if (context.RequestServices == null)
					{
						context.Errors.Add(new ExecutionError("RequestServices is not available."));
						return null;
					}

					var storageSelectionService = context.RequestServices.GetRequiredService<IStorageSelectionService>();

					// Отримуємо storageType з HTTP-заголовка через UserContext
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
						IsDone = false, // Завжди false при додаванні
						CreatedAt = DateTime.Now // Встановлюємо поточний час
					};

					// Очікуємо завершення асинхронної операції додавання
					await repo.AddTask(task);

					// Важливо: Якщо ваш репозиторій генерує TaskId або інші поля (наприклад, CreatedAt),
					// і ви хочете їх повернути клієнту, метод AddTask повинен оновлювати об'єкт 'task'
					// або повертати оновлену модель.
					// Якщо AddTask в ITaskRepository повертає 'Task', то TaskId може бути не оновлений
					// після виклику AddTask.
					// Якщо потрібно повертати TaskId, змініть ITaskRepository.AddTask на Task<TaskModel>
					// і повертайте оновлену модель з репозиторію.
					return task; // Повертаємо об'єкт задачі, який може бути оновлений в репозиторії
				});

			// --- mark Task As Done ---
			Field<BooleanGraphType>("markTaskAsDone") // Визначаємо поле 'markTaskAsDone'
				.Description("Marks a task as done by its ID in the selected storage.")
				.Arguments(
					new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "taskId" }
				)
				.ResolveAsync(async context => // Використовуємо ResolveAsync для асинхронного резолвера
				{
					// Перевірка наявності RequestServices (для надійності)
					if (context.RequestServices == null)
					{
						context.Errors.Add(new ExecutionError("RequestServices is not available."));
						return false;
					}

					var storageSelectionService = context.RequestServices.GetRequiredService<IStorageSelectionService>();

					// Отримуємо storageType з HTTP-заголовка через UserContext
					if (!context.UserContext.TryGetValue("StorageTypeHeader", out object? storageTypeObj) || !(storageTypeObj is string storageTypeHeader) || !Enum.TryParse<StorageType>(storageTypeHeader, true, out var storageType))
					{
						context.Errors.Add(new ExecutionError("HTTP header 'X-Storage-Type' is missing or invalid. Expected 'XML' or 'Database'."));
						return false;
					}

					var taskId = context.GetArgument<int>("taskId");
					var repo = storageSelectionService.GetRepositoryByStorageType(storageType);

					// Очікуємо завершення асинхронної операції
					await repo.MarkTaskAsDone(taskId);
					return true; // Повертаємо true, якщо операція успішна
				});
		}
	}
}