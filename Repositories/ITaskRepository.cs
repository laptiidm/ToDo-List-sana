using Todo_List_3.Models; 

namespace Todo_List_3.Repositories
{
	public interface ITaskRepository
	{
		Task<IEnumerable<TaskModel>> GetActiveTasks(); 
		Task<IEnumerable<TaskModel>> GetCompletedTasks(); 
		Task AddTask(TaskModel task); 
		Task MarkTaskAsDone(int taskId); 
	}
}