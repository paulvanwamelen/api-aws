using GraphQL.Types;

using abstractplay.DB;
namespace abstractplay.GraphQL
{
    public class RankType : ObjectGraphType<GamesMetaRanks>
    {
        public RankType()
        {
            Field<StringGraphType>("id", resolve: _ => GuidGenerator.HelperBAToString(((GamesMetaRanks)_.Source).EntryId), description: "Rank entry's unique ID number");
            Field<IntGraphType>("rank", resolve: _=> ((GamesMetaRanks)_.Source).Rank, description: "The rank itself");
            Field<GamesMetaType>("game", resolve: _ => ((GamesMetaRanks)_.Source).Game, description: "The game the rank applies to");
            Field<UserType>("user", resolve: _ => ((GamesMetaRanks)_.Source).Owner, description: "The user who applied the rank");
        }
    }
}