using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    //Public, unauthenticated, read-only schema
    public class APSchemaRO : Schema
    {
        public APSchemaRO(MyContext db)
        {
            Query = new APQuery(db);
        }
    }
}
