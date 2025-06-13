using GraphQL; // Додаємо, якщо немає
using GraphQL.Types;
using GraphQL.NewtonsoftJson;
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
		private readonly IDocumentSerializer _serializer;

		public GraphQLController(
			ISchema schema,
			IDocumentExecuter executer,
			IDocumentSerializer serializer)
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

			var result = await _executer.ExecuteAsync(options =>
			{
				options.Schema = _schema;
				options.Query = query.Query;
				options.OperationName = query.OperationName;
				options.Variables = query.Variables?.ToInputs();
				options.RequestServices = HttpContext.RequestServices;
			});

			var json = _serializer.Serialize(result);
			return Content(json, "application/json");
		}
	}
}