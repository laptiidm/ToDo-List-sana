// Todo_List_3.Repositories/XmlTaskRepository.cs
using System.Xml.Serialization;
using Todo_List_3.Models;
using Todo_List_3.Services;
using System.Threading.Tasks; // ДОДАЙТЕ ЦЕЙ USING
using System.IO;
using System.Linq;
using System.Xml.Linq; // Для Max та Where

namespace Todo_List_3.Repositories
{
	public class XmlTaskRepository : ITaskRepository
	{
		private readonly string _xmlFilePath;
		private readonly object _lockObject = new object();

		// ЗМІНЕНО: Конструктор приймає IXmlRepositorySettingsProvider
		public XmlTaskRepository(IXmlRepositorySettingsProvider settingsProvider)
		{
			_xmlFilePath = settingsProvider.GetFilePath();

			var directory = Path.GetDirectoryName(_xmlFilePath);
			if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			if (!File.Exists(_xmlFilePath))
			{
				new XDocument(new XElement("Tasks")).Save(_xmlFilePath);
			}
		}

		public async Task<IEnumerable<TaskModel>> GetActiveTasks()
		{
			try
			{
				var serializer = new XmlSerializer(typeof(XmlTasksWrapper));
				XmlTasksWrapper taskListWrapper;

				// Асинхронне читання файлу
				if (!File.Exists(_xmlFilePath))
				{
					return Enumerable.Empty<TaskModel>(); // Повертаємо порожній список, якщо файл не існує
				}

				// XmlSerializer не має прямої асинхронної десеріалізації.
				// Обертаємо її в Task.Run, щоб не блокувати потік.
				taskListWrapper = await Task.Run(() =>
				{
					using var stream = new FileStream(_xmlFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
					return (XmlTasksWrapper)serializer.Deserialize(stream)!;
				});

				var activeTasks = taskListWrapper.Tasks ?? new();
				return activeTasks.Where(task => !task.IsDone).ToList();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[ERROR] Неможливо зчитати XML (активні задачі): {ex.Message}");
				return Enumerable.Empty<TaskModel>();
			}
		}

		public async Task<IEnumerable<TaskModel>> GetCompletedTasks()
		{
			try
			{
				var serializer = new XmlSerializer(typeof(XmlTasksWrapper));
				XmlTasksWrapper taskListWrapper;

				if (!File.Exists(_xmlFilePath))
				{
					return Enumerable.Empty<TaskModel>();
				}

				taskListWrapper = await Task.Run(() =>
				{
					using var stream = new FileStream(_xmlFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
					return (XmlTasksWrapper)serializer.Deserialize(stream)!;
				});

				var completedTasks = taskListWrapper.Tasks ?? [];
				return completedTasks.Where(task => task.IsDone).OrderByDescending(task => task.CompletedAt).ToList();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[ERROR] Неможливо зчитати XML (виконані задачі): {ex.Message}");
				return Enumerable.Empty<TaskModel>();
			}
		}

		public async Task AddTask(TaskModel task)
		{
			try
			{
				var serializer = new XmlSerializer(typeof(XmlTasksWrapper));
				XmlTasksWrapper taskListWrapper;

				// Зчитуємо існуючі задачі асинхронно
				if (File.Exists(_xmlFilePath))
				{
					taskListWrapper = await Task.Run(() =>
					{
						using var stream = new FileStream(_xmlFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
						return (XmlTasksWrapper)serializer.Deserialize(stream)!;
					});
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

				// Записуємо назад у файл асинхронно
				await Task.Run(() =>
				{
					using var stream = new FileStream(_xmlFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
					serializer.Serialize(stream, taskListWrapper);
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[ERROR] Неможливо додати задачу: {ex.Message}");
			}
		}

		public async Task MarkTaskAsDone(int taskId)
		{
			try
			{
				var serializer = new XmlSerializer(typeof(XmlTasksWrapper));
				XmlTasksWrapper taskListWrapper;

				if (File.Exists(_xmlFilePath))
				{
					taskListWrapper = await Task.Run(() =>
					{
						using var stream = new FileStream(_xmlFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
						return (XmlTasksWrapper)serializer.Deserialize(stream)!;
					});
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

					// Перезаписуємо файл з оновленими даними асинхронно
					await Task.Run(() =>
					{
						using var stream = new FileStream(_xmlFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
						serializer.Serialize(stream, taskListWrapper);
					});
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