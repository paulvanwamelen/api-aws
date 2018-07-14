using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class GameStatusType : ObjectGraphType<GamesMetaStatus>
    {
        public GameStatusType()
        {
            Field<StringGraphType>("id", resolve: _ => GuidGenerator.HelperBAToString(((GamesMetaStatus)_.Source).StatusId), description: "Entry's unique ID number");
            Field(x => x.Timestamp).Name("timestamp").Description("The date and time of the  update");
            Field(x => x.IsUp).Name("isUp").Description("Whether the code responded");
            Field(x => x.Message).Name("message").Description("Details about the check");
        }
    }
}