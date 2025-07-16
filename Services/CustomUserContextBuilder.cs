using GraphQL.Server.Transports.AspNetCore;
using Microsoft.AspNetCore.Http;

namespace Todo_List_3.Services
{
	public class CustomUserContextBuilder : IUserContextBuilder
	{
		public ValueTask<IDictionary<string, object?>?> BuildUserContextAsync(HttpContext context, object? payload)
		{
			var storageType = context.Request.Headers["X-Storage-Type"].FirstOrDefault();

			//Console.WriteLine($"CustomUserContextBuilder: StorageType from header: {storageType}");

			IDictionary<string, object?> result = new Dictionary<string, object?>
			{
				["StorageType"] = storageType
			};

			return ValueTask.FromResult<IDictionary<string, object?>?>(result);
		}
	}
}


