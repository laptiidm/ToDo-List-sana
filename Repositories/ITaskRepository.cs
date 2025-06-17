// Todo_List_3.Repositories/ITaskRepository.cs
using Todo_List_3.Models; // Переконайтеся, що це правильно
using System.Collections.Generic; // Для IEnumerable
using System.Threading.Tasks; // ОБОВ'ЯЗКОВО ДЛЯ Task / Task<T>

namespace Todo_List_3.Repositories
{
	public interface ITaskRepository
	{
		Task<IEnumerable<TaskModel>> GetActiveTasks(); // <--- ОБОВ'ЯЗКОВО Task<IEnumerable<TaskModel>>
		Task<IEnumerable<TaskModel>> GetCompletedTasks(); // <--- ОБОВ'ЯЗКОВО Task<IEnumerable<TaskModel>>
		Task AddTask(TaskModel task); // <--- ОБОВ'ЯЗКОВО Task
		//Task<TaskModel> AddTask(TaskModel task);
		Task MarkTaskAsDone(int taskId); // <--- ОБОВ'ЯЗКОВО Task
	}
}