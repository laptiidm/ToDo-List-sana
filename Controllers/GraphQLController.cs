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

			// *** ОДИН ІЗ КЛЮЧОВИХ ВІДСУТНІХ БЛОКІВ КОДУ ***
			// 1. Отримання HTTP-заголовка "X-Storage-Type"
			string? storageTypeHeader = HttpContext.Request.Headers["X-Storage-Type"].FirstOrDefault();

			// 2. Створення UserContext для передачі даних у GraphQL-виконавець
			var userContext = new Dictionary<string, object?>
			{
				{ "StorageTypeHeader", storageTypeHeader } // Передаємо значення заголовка
                // Можете додати інші дані, які вам потрібні в UserContext
            };


			// Convert Dictionary<string, object> to GraphQL.Inputs
			var variables = query.Variables != null
				? new Inputs(query.Variables.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value))
				: Inputs.Empty;

			var result = await _executer.ExecuteAsync(options =>
			{
				options.Schema = _schema;
				options.Query = query.Query;
				options.OperationName = query.OperationName;
				options.Variables = variables;
				// *** ДРУГИЙ КЛЮЧОВИЙ ВІДСУТНІЙ РЯДОК ***
				options.UserContext = userContext; // Передаємо створений UserContext
				options.RequestServices = HttpContext.RequestServices;
				options.ThrowOnUnhandledException = true;
			});

			// ПОРАДА: Краща обробка помилок та відповіді
			if (result.Errors?.Any() == true)
			{
				// Якщо є помилки, можливо, ви захочете повернути статус BadRequest (400)
				// і серіалізувати результат, який буде містити поле "errors".
				return BadRequest(Content(_serializer.Serialize(result), "application/json"));
			}

			var json = _serializer.Serialize(result);
			return Content(json, "application/json");
		}
	}
}