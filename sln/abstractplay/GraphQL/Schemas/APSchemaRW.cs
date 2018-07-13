using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    //Schema for private SNS-mutator
    public class APSchemaRW : Schema
    {
        public APSchemaRW(MyContext db)
        {
            Query = new APQuery(db);
            Mutation = new APMutator(db);
        }
    }
}
