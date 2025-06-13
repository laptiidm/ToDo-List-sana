using Todo_List_3.Enums;

namespace Todo_List_3.Configurations
{
	public class StorageOptions
	{
		public string? XmlFilePath { get; set; }
		public string? DBConnectionString { get; set; }
		public StorageType DefaultStorageType { get; set; } = StorageType.Database;
	}
}
