using System.Linq;
using GraphQL;
using GraphQL.Types;
using GraphQL.NewtonsoftJson;
using Microsoft.AspNetCore.Mvc;
using Todo_List_3.GraphQL;
using Newtonsoft.Json.Linq;
using Todo_List_3.Enums;

namespace Todo_List_3.Controllers
{
	[Route("graphql")]
	[ApiController]
	public class GraphQLController : ControllerBase
	{
		private readonly ISchema _schema;
		private readonly IDocumentExecuter _executer;
		private readonly IGraphQLTextSerializer _serializer;

		public GraphQLController(
			ISchema schema,
			IDocumentExecuter executer,
			IGraphQLTextSerializer serializer)
		{
			_schema = schema;
			_executer = executer;
			_serializer = serializer;
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] GraphQLQuery query)
		{
			if (query == null || string.IsNullOrEmpty(query.Query))
				return BadRequest("GraphQL query is missing.");

			var variables = query.Variables;
			Inputs? inputs = null;

			if (variables != null && variables.TryGetValue("storageType", out var raw))
			{
				// Преобразуємо строкове значення enum у StorageType  
				var enumValue = Enum.Parse<StorageType>(raw.ToString()!);
				inputs = new Inputs(new Dictionary<string, object?> { { "storageType", enumValue } });
			}

			var result = await _executer.ExecuteAsync(options =>
			{
				options.Schema = _schema;
				options.Query = query.Query;
				options.OperationName = query.OperationName;
				options.Variables = inputs;
				options.RequestServices = HttpContext.RequestServices;
				options.ThrowOnUnhandledException = true; // Fix: Use 'ThrowOnUnhandledException' instead of 'ExposeExceptions'  
			});

			var json = _serializer.Serialize(result);
			return Content(json, "application/json");
		}
	}
}
