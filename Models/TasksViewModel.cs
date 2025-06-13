using System.Collections.Generic;
using Todo_List_3.Enums;

namespace Todo_List_3.Models
{
	public class TasksViewModel
	{
		public List<TaskModel> ActiveTasks { get; set; } = new List<TaskModel>();
		public List<TaskModel> CompletedTasks { get; set; } = new List<TaskModel>();
		public TaskModel? NewTask { get; set; }

		public StorageType CurrentStorage { get; set; } // 🔥 потрібне для в’ю
	}
}

