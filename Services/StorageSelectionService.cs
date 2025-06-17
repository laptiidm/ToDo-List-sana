using Todo_List_3.Enums;
using Microsoft.AspNetCore.Http;
using System;
using Todo_List_3.Configurations;
using Microsoft.Extensions.Options;
using Todo_List_3.Repositories;
using Microsoft.Extensions.DependencyInjection; // Додаємо цей using для GetRequiredService

namespace Todo_List_3.Services
{
	public class StorageSelectionService : IStorageSelectionService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly StorageOptions _storageOptions;
		private readonly IServiceProvider _serviceProvider; // ТЕПЕР ВІН ВИКОРИСТОВУЄТЬСЯ ДЛЯ ОТРИМАННЯ РЕПОЗИТОРІЇВ
		private const string StorageTypeSessionKey = "CurrentStorageType";

		public StorageSelectionService(
			IHttpContextAccessor httpContextAccessor,
			IOptions<StorageOptions> options,
			IServiceProvider serviceProvider) // Інжектуємо IServiceProvider
		{
			_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			_storageOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public StorageType GetCurrentStorageType()
		{
			// Цей метод все ще працює з сесією для визначення поточного типу сховища
			// (використовується для дефолтного значення або вибору користувача через веб-інтерфейс).
			var session = _httpContextAccessor?.HttpContext?.Session;
			if (session == null)
			{
				return _storageOptions.DefaultStorageType; // Або інша логіка для відсутності сесії
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

		// Цей метод повертає репозиторій на основі поточного типу сховища з сесії
		public ITaskRepository GetCurrentRepository()
		{
			var currentStorage = GetCurrentStorageType();
			return GetRepositoryFromProvider(currentStorage);
		}

		// Цей метод повертає репозиторій на основі типу сховища, переданого як аргумент
		// Він буде використовуватися в GraphQL резолверах
		public ITaskRepository GetRepositoryByStorageType(StorageType storageType)
		{
			return GetRepositoryFromProvider(storageType);
		}

		// ДОПОМІЖНИЙ МЕТОД: отримує репозиторій з DI-контейнера
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