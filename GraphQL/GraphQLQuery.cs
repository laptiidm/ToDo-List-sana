namespace Todo_List_3.GraphQL
{
	public class GraphQLQuery
	{
		public string? Query { get; set; }
		public string? OperationName { get; set; }
		public Dictionary<string, object>? Variables { get; set; }
	}
}
