
using Todo_List_3.Enums;
using Todo_List_3.Repositories;

namespace Todo_List_3.Services;

public interface IStorageSelectionService
{
	StorageType GetCurrentStorageType();
	void SetCurrentStorageType(StorageType selectedStorageType);
	ITaskRepository GetCurrentRepository();
	ITaskRepository GetRepositoryByStorageType(StorageType storageType);
}
