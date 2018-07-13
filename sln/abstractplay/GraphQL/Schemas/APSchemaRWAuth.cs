using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    //Authenticated read/write schema (POST requests)
    public class APSchemaRWAuth : Schema
    {
        public APSchemaRWAuth(MyContext db)
        {
            Query = new APQueryAuth(db);
            Mutation = new APMutatorAuth(db);
        }
    }
}
