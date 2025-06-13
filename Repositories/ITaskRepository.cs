using Todo_List_3.Models;

namespace Todo_List_3.Repositories

{
	public interface ITaskRepository
	{
		List<TaskModel> GetActiveTasks();
		List<TaskModel> GetCompletedTasks();
		void AddTask(TaskModel task);
		void MarkTaskAsDone(int taskId);
	}
}
