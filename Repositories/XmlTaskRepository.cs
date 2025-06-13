using System.Xml.Serialization;
using Todo_List_3.Models;

namespace Todo_List_3.Repositories
{
	public class XmlTaskRepository : ITaskRepository
	{
		private readonly string _xmlFilePath;

		public XmlTaskRepository(string? xmlFilePath)
		{
			_xmlFilePath = xmlFilePath ?? throw new ArgumentNullException(nameof(xmlFilePath));
		}

		public List<TaskModel> GetActiveTasks()
		{
			try
			{
				var serializer = new XmlSerializer(typeof(XmlTasksWrapper));

				using var stream = new FileStream(_xmlFilePath, FileMode.Open);
				var taskListWrapper = (XmlTasksWrapper)serializer.Deserialize(stream)!;

				var activeTasks = taskListWrapper.Tasks ?? new(); // simplified
				return activeTasks.Where(task => !task.IsDone).ToList();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[ERROR] Неможливо зчитати XML: {ex.Message}");
				return new(); // simplified
			}
		}

		public List<TaskModel> GetCompletedTasks()
		{
			try
			{
				var serializer = new XmlSerializer(typeof(XmlTasksWrapper));

				using var stream = new FileStream(_xmlFilePath, FileMode.Open);
				var taskListWrapper = (XmlTasksWrapper)serializer.Deserialize(stream)!;

				var completedTasks = taskListWrapper.Tasks ?? []; // simplified   
				return completedTasks.Where(task => task.IsDone).OrderByDescending(task => task.CompletedAt).ToList();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[ERROR] Неможливо зчитати XML: {ex.Message}");
				return []; // simplified  
			}
		}


		public void AddTask(TaskModel task)
		{
			try
			{
				var serializer = new XmlSerializer(typeof(XmlTasksWrapper));
				XmlTasksWrapper taskListWrapper;

				// Зчитуємо існуючі задачі
				if (File.Exists(_xmlFilePath))
				{
					using (var stream = new FileStream(_xmlFilePath, FileMode.Open))
						{
							taskListWrapper = (XmlTasksWrapper)serializer.Deserialize(stream)!;
						}
				}
				else
				{
					taskListWrapper = new XmlTasksWrapper
					{
						Tasks = new List<TaskModel>()
					};
				}

				// set TaskId — найпростіший спосіб: найбільший +1
				int maxId = taskListWrapper.Tasks?.Max(t => t.TaskId) ?? 0;
				task.TaskId = maxId + 1;

				// Ініціалізуємо дати
				task.CreatedAt = DateTime.Now;
				task.IsDone = false;

				// Додаємо задачу
				taskListWrapper.Tasks!.Add(task);

				// Записуємо назад у файл
				using (var stream = new FileStream(_xmlFilePath, FileMode.Create))
				{
					serializer.Serialize(stream, taskListWrapper);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[ERROR] Неможливо додати задачу: {ex.Message}");
			}
		}


		public void MarkTaskAsDone(int taskId)
		{
			try
			{
				var serializer = new XmlSerializer(typeof(XmlTasksWrapper));

				XmlTasksWrapper taskListWrapper;

				if (File.Exists(_xmlFilePath))
				{
					using (var stream = new FileStream(_xmlFilePath, FileMode.Open))
					{
						taskListWrapper = (XmlTasksWrapper)serializer.Deserialize(stream)!;
					}
				}
				else
				{
					Console.WriteLine("[WARN] XML-файл не знайдено.");
					return;
				}

				// Знаходимо задачу за ID
				var task = taskListWrapper.Tasks?.FirstOrDefault(t => t.TaskId == taskId);

				if (task != null)
				{
					task.IsDone = true;
					task.CompletedAt = DateTime.Now;

					// Перезаписуємо файл з оновленими даними
					using (var stream = new FileStream(_xmlFilePath, FileMode.Create))
					{
						serializer.Serialize(stream, taskListWrapper);
					}
				}
				else
				{
					Console.WriteLine($"[INFO] Задача з ID {taskId} не знайдена.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[ERROR] Помилка при позначенні задачі як виконаної: {ex.Message}");
			}
		}


	}
}
