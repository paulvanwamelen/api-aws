using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    //Authenticated read-only schema (GET requests)
    public class APSchemaROAuth : Schema
    {
        public APSchemaROAuth(MyContext db)
        {
            Query = new APQueryAuth(db);
        }
    }
}
