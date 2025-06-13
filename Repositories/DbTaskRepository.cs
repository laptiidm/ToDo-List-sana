using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Todo_List_3.Models;

namespace Todo_List_3.Repositories
{
	public class DbTaskRepository : ITaskRepository
	{
		private readonly string _connectionString;

		public DbTaskRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public List<TaskModel> GetActiveTasks()
		{
			var tasks = new List<TaskModel>();
			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand("SELECT * FROM tasks WHERE is_done = 0", connection);
				connection.Open();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						tasks.Add(ReadTask(reader));
					}
				}
			}
			return tasks;
		}

		public List<TaskModel> GetCompletedTasks()
		{
			var tasks = new List<TaskModel>();
			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand("SELECT * FROM tasks WHERE is_done = 1", connection);
				connection.Open();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						tasks.Add(ReadTask(reader));
					}
				}
			}
			return tasks;
		}

		public void AddTask(TaskModel task)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand(
					@"INSERT INTO tasks (description, due_date, category_id, is_done, created_at, completed_at)
                      VALUES (@description, @due_date, @category_id, @is_done, @created_at, @completed_at)", connection);

				command.Parameters.AddWithValue("@description", task.Description ?? (object)DBNull.Value);
				command.Parameters.AddWithValue("@due_date", task.DueDate ?? (object)DBNull.Value);
				command.Parameters.AddWithValue("@category_id", task.CategoryId ?? (object)DBNull.Value);
				command.Parameters.AddWithValue("@is_done", task.IsDone);
				command.Parameters.AddWithValue("@created_at", task.CreatedAt ?? DateTime.Now);
				command.Parameters.AddWithValue("@completed_at", task.CompletedAt ?? (object)DBNull.Value);

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public void MarkTaskAsDone(int taskId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand(
					@"UPDATE tasks 
                      SET is_done = 1, completed_at = @completed_at 
                      WHERE task_id = @task_id", connection);

				command.Parameters.AddWithValue("@task_id", taskId);
				command.Parameters.AddWithValue("@completed_at", DateTime.Now);

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		private TaskModel ReadTask(SqlDataReader reader)
		{
			return new TaskModel
			{
				TaskId = (int)reader["task_id"],
				Description = reader["description"]?.ToString(),
				DueDate = reader["due_date"] as DateTime?,
				CategoryId = reader["category_id"] as int?,
				IsDone = (bool)reader["is_done"],
				CreatedAt = reader["created_at"] as DateTime?,
				CompletedAt = reader["completed_at"] as DateTime?
			};
		}
	}
}





