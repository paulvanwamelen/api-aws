using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class ConsoleVoteType : ObjectGraphType<ConsolesVotes>
    {
        public ConsoleVoteType()
        {
            Field<StringGraphType>("id", resolve: _ => GuidGenerator.HelperBAToString(((ConsolesVotes)_.Source).EntryId), description: "Vote's unique ID number");
            Field<ConsoleType>("command", resolve: _ => ((ConsolesVotes)_.Source).Console, description: "The command this vote applies to");
            Field<UserType>("user", resolve: _ => ((ConsolesVotes)_.Source).VoterNavigation, description: "The user the vote belongs to");
            Field<BooleanGraphType>("vote", resolve: _ => ((ConsolesVotes)_.Source).Vote, description: "The vote itself");
        }
    }
}