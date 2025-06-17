using Microsoft.Extensions.Options;
using Todo_List_3.Configurations;

namespace Todo_List_3.Services 
{
	public class XmlRepositorySettingsProvider : IXmlRepositorySettingsProvider
	{
		private readonly StorageOptions _options;

		public XmlRepositorySettingsProvider(IOptions<StorageOptions> options)
		{
			_options = options.Value;
		}

		public string GetFilePath()
		{
			return _options.XmlFilePath ?? throw new InvalidOperationException("XML file path is not configured in StorageOptions.");
		}
	}
}