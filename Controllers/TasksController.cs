using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Todo_List_3.Configurations;
using Todo_List_3.Enums;
using Todo_List_3.Models;
using Todo_List_3.Services;

namespace Todo_List_3.Controllers
{
	public class TasksController : Controller
	{
		private readonly IStorageSelectionService _storageSelectionService;
		private readonly StorageOptions _storageOptions;

		public TasksController(
			IStorageSelectionService storageSelectionService,
			IOptions<StorageOptions> storageOptions)
		{
			_storageSelectionService = storageSelectionService;
			_storageOptions = storageOptions.Value;
		}

		[HttpGet]
		public IActionResult Index()
		{
			var repo = _storageSelectionService.GetCurrentRepository();
			var activeTasks = repo?.GetActiveTasks() ?? new List<TaskModel>();
			var completedTasks = repo?.GetCompletedTasks() ?? new List<TaskModel>();
			var currentStorage = _storageSelectionService.GetCurrentStorageType();

			var model = new TasksViewModel
			{
				ActiveTasks = activeTasks,
				CompletedTasks = completedTasks,
				CurrentStorage = currentStorage,
				NewTask = new TaskModel() // для форми, необов'язково, але корисно
			};

			return View(model);
		}


		[HttpPost]
		public IActionResult Add(TaskModel newTask)
		{
			if (!ModelState.IsValid)
			{
				// Якщо форма була некоректно заповнена — повертаємо на Index з поточними задачами
				var repo = _storageSelectionService.GetCurrentRepository();

				var model = new TasksViewModel
				{
					ActiveTasks = repo?.GetActiveTasks() ?? new(),
					CompletedTasks = repo?.GetCompletedTasks() ?? new(),
					CurrentStorage = _storageSelectionService.GetCurrentStorageType(),
					NewTask = newTask
				};

				return View("Index", model);
			}

			var currentRepo = _storageSelectionService.GetCurrentRepository();
			currentRepo?.AddTask(newTask);

			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult MarkAsDone(int taskId)
		{
			var repo = _storageSelectionService.GetCurrentRepository();
			repo?.MarkTaskAsDone(taskId);

			return RedirectToAction("Index");
		}


		[HttpPost]
		public IActionResult SelectStorage(StorageType type)
		{
			if (_storageSelectionService != null)
			{
				_storageSelectionService.SetCurrentStorageType(type);
			}
			return RedirectToAction("Index", "Tasks");
		}

	}
}

