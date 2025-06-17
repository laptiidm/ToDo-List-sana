// Todo_List_3.Repositories/DbTaskRepository.cs
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Todo_List_3.Models;
using Todo_List_3.Services;
using System.Threading.Tasks; // ДОДАЙТЕ ЦЕЙ USING

namespace Todo_List_3.Repositories
{
	public class DbTaskRepository : ITaskRepository
	{
		private readonly string _connectionString;

		// ЗМІНЕНО: Конструктор приймає IDbRepositorySettingsProvider
		public DbTaskRepository(IDatabaseRepositorySettingsProvider settingsProvider)
		{
			_connectionString = settingsProvider.GetConnectionString();
		}
        // ...

		public async Task<IEnumerable<TaskModel>> GetActiveTasks()
		{
			var tasks = new List<TaskModel>();
			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand("SELECT * FROM tasks WHERE is_done = 0", connection);
				await connection.OpenAsync(); // Асинхронне відкриття з'єднання
				using (var reader = await command.ExecuteReaderAsync()) // Асинхронне виконання запиту
				{
					while (await reader.ReadAsync()) // Асинхронне читання записів
					{
						tasks.Add(ReadTask(reader));
					}
				}
			}
			return tasks;
		}

		public async Task<IEnumerable<TaskModel>> GetCompletedTasks()
		{
			var tasks = new List<TaskModel>();
			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand("SELECT * FROM tasks WHERE is_done = 1", connection);
				await connection.OpenAsync();
				using (var reader = await command.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						tasks.Add(ReadTask(reader));
					}
				}
			}
			return tasks;
		}

		public async Task AddTask(TaskModel task)
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

				await connection.OpenAsync(); // Асинхронне відкриття
				await command.ExecuteNonQueryAsync(); // Асинхронне виконання
			}
		}

		public async Task MarkTaskAsDone(int taskId)
		{
			using (var connection = new SqlConnection(_connectionString))
			{
				var command = new SqlCommand(
					@"UPDATE tasks
					  SET is_done = 1, completed_at = @completed_at
					  WHERE task_id = @task_id", connection);

				command.Parameters.AddWithValue("@task_id", taskId);
				command.Parameters.AddWithValue("@completed_at", DateTime.Now);

				await connection.OpenAsync();
				await command.ExecuteNonQueryAsync();
			}
		}

		private TaskModel ReadTask(SqlDataReader reader)
		{
			// Цей допоміжний метод залишається синхронним, оскільки він просто читає дані з вже заповненого reader.
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