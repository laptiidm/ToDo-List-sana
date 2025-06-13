using System.Xml.Serialization;
using Todo_List_3.Models;

namespace Todo_List_3.Models
{
	[XmlRoot("Tasks")]
	public class XmlTasksWrapper
	{
		[XmlElement("Task")]
		public List<TaskModel>? Tasks { get; set; }
	}
}
