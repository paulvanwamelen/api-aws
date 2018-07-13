using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class MeType : UserType
    {
        public MeType(): base() 
        {
            Field(x => x.Anonymous).Name("anonymous").Description("Whether the account is flagged as anonymous");
        }
    }
}