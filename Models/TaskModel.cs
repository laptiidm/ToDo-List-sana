using System;
using System.ComponentModel.DataAnnotations;

namespace Todo_List_3.Models
{
	public class TaskModel
	{
		public int TaskId { get; set; }

		//[Required(ErrorMessage = "Опис задачі обов’язковий")]
		//[StringLength(1000, ErrorMessage = "Опис не може бути довшим за 200 символів")]
		public string? Description { get; set; }

		[DataType(DataType.Date)]
		[Display(Name = "Дата завершення")]
		public DateTime? DueDate { get; set; }

		public int? CategoryId { get; set; }
		public bool IsDone { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? CompletedAt { get; set; }
		public string? CategoryName { get; set; }
	}
}
