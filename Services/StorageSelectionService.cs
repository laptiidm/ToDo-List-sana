using Todo_List_3.Enums;
using Microsoft.AspNetCore.Http;
using System;
using Todo_List_3.Configurations;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Todo_List_3.Repositories;

namespace Todo_List_3.Services
{
	public class StorageSelectionService : IStorageSelectionService
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly StorageOptions _storageOptions;
		private const string StorageTypeSessionKey = "CurrentStorageType";

		public StorageSelectionService(
			IHttpContextAccessor httpContextAccessor,
			IOptions<StorageOptions> options)
		{
			_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			_storageOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}


		public StorageType GetCurrentStorageType()
		{
			var session = _httpContextAccessor?.HttpContext?.Session;
			if (session == null)
			{
				return _storageOptions?.DefaultStorageType ?? throw new InvalidOperationException("DefaultStorageType is not configured {id=1}.");
			}

			var storedTypeString = session.GetString(StorageTypeSessionKey);
			if (Enum.TryParse<StorageType>(storedTypeString, out var storedType))
			{
				return storedType;
			}
			else
			{
				return _storageOptions?.DefaultStorageType ?? throw new InvalidOperationException("DefaultStorageType is not configured {id=2}.");
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

		// динамічно створюємо репозиторій так само, як у Program.cs
		public ITaskRepository GetCurrentRepository()
		{
			var currentStorage = GetCurrentStorageType();

			return currentStorage switch
			{
				StorageType.Xml => new XmlTaskRepository(_storageOptions.XmlFilePath ?? throw new InvalidOperationException("XmlFilePath is not configured.")),
				StorageType.Database => new DbTaskRepository(_storageOptions.DBConnectionString ?? throw new InvalidOperationException("DBConnectionString is not configured.")),
				_ => throw new NotSupportedException($"Storage type '{currentStorage}' is not supported."),
			};
		}

		public ITaskRepository GetRepositoryByStorageType(StorageType storageType)
		{
			return storageType switch
			{
				StorageType.Xml => new XmlTaskRepository(_storageOptions.XmlFilePath ?? throw new InvalidOperationException("XmlFilePath is not configured.")),
				StorageType.Database => new DbTaskRepository(_storageOptions.DBConnectionString ?? throw new InvalidOperationException("DBConnectionString is not configured.")),
				_ => throw new NotSupportedException($"Storage type '{storageType}' is not supported."),
			};
		}

	}

}
