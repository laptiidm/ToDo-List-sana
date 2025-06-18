using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Todo_List_3.GraphQL;

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

			string? storageTypeHeader = HttpContext.Request.Headers["X-Storage-Type"].FirstOrDefault();

			var userContext = new Dictionary<string, object?>
		   {
			   { "StorageTypeHeader", storageTypeHeader }
		   };

			Inputs variables = Inputs.Empty;
			if (query.Variables != null)
			{
				string? variablesJson = _serializer.Serialize(query.Variables);

				// Fix for CS8600: Ensure variablesJson is not null before deserialization  
				if (!string.IsNullOrEmpty(variablesJson))
				{
					variables = _serializer.Deserialize<Inputs>(variablesJson) ?? Inputs.Empty;
				}
			}

			var result = await _executer.ExecuteAsync(options =>
			{
				options.Schema = _schema;
				options.Query = query.Query;
				options.OperationName = query.OperationName;
				options.Variables = variables;
				options.UserContext = userContext;
				options.RequestServices = HttpContext.RequestServices;
				options.ThrowOnUnhandledException = true;
			});

			if (result.Errors?.Any() == true)
			{
				return BadRequest(Content(_serializer.Serialize(result), "application/json"));
			}

			var json = _serializer.Serialize(result);
			return Content(json, "application/json");
		}
	}
}