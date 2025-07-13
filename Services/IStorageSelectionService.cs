
using Todo_List_3.Enums;
using Todo_List_3.Repositories;

namespace Todo_List_3.Services;

public interface IStorageSelectionService
{
	StorageType GetCurrentStorageType(); // base
	void SetCurrentStorageType(StorageType selectedStorageType); // base
	ITaskRepository GetCurrentRepository(); // base
	ITaskRepository GetRepositoryByStorageType(StorageType storageType); // GraphQL (uses h)
}