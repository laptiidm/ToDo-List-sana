using Microsoft.Extensions.Options;
using Todo_List_3.Configurations;

namespace Todo_List_3.Services // Або інший простір імен для провайдерів
{
	public class DbRepositorySettingsProvider : IDatabaseRepositorySettingsProvider
	{
		private readonly StorageOptions _options;

		public DbRepositorySettingsProvider(IOptions<StorageOptions> options)
		{
			_options = options.Value;
		}

		public string GetConnectionString()
		{
			return _options.DBConnectionString ?? throw new InvalidOperationException("Database connection string is not configured in StorageOptions.");
		}
	}
}