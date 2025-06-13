using GraphQL.Types;

namespace Todo_List_3.Enums
{
	public class GraphQLStorageType : EnumerationGraphType<StorageType>
	{
		public GraphQLStorageType()
		{
			Name = "StorageType";
			Description = "Тип сховища: Xml або Database";
		}
	}
}
