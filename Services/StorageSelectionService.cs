using Todo_List_3.Enums;
using Todo_List_3.Configurations;
using Microsoft.Extensions.Options;
using Todo_List_3.Repositories;

namespace Todo_List_3.Services
{
	public class StorageSelectionService : IStorageSelectionService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly StorageOptions _storageOptions;
		private readonly IServiceProvider _serviceProvider; // for getting repo`s
		private const string StorageTypeSessionKey = "CurrentStorageType";

		public StorageSelectionService(
			IHttpContextAccessor httpContextAccessor,
			IOptions<StorageOptions> options,
			IServiceProvider serviceProvider)
		{
			_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			_storageOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public StorageType GetCurrentStorageType()
		{
			// get the current storage type from session
			var session = _httpContextAccessor?.HttpContext?.Session;
			if (session == null)
			{
				return _storageOptions.DefaultStorageType; 
			}

			var storedTypeString = session.GetString(StorageTypeSessionKey);
			if (Enum.TryParse<StorageType>(storedTypeString, out var storedType))
			{
				return storedType;
			}
			else
			{
				return _storageOptions.DefaultStorageType;
			}
		}

		public void SetCurrentStorageType(StorageType storageType)
		{
			var session = _httpContextAccessor?.HttpContext?.Session;
			if (session == null)
			{
				throw new InvalidOperationException("Session is not available.");
			}
			session.SetString(StorageTypeSessionKey, storageType.ToString());
		}

		// get a repo based on the current storage
		public ITaskRepository GetCurrentRepository()
		{
			var currentStorage = GetCurrentStorageType();
			return GetRepositoryFromProvider(currentStorage);
		}
		// for GraphQL resolvers
		public ITaskRepository GetRepositoryByStorageType(StorageType storageType)
		{
			return GetRepositoryFromProvider(storageType);
		}

		// auxiliary method
		private ITaskRepository GetRepositoryFromProvider(StorageType storageType)
		{
			return storageType switch
			{
				StorageType.Xml => _serviceProvider.GetRequiredService<XmlTaskRepository>(), // Отримуємо з DI
				StorageType.Database => _serviceProvider.GetRequiredService<DbTaskRepository>(), // Отримуємо з DI
				_ => throw new NotSupportedException($"Storage type '{storageType}' is not supported."),
			};
		}
	}
}