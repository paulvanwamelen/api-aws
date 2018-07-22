using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class GamesDataClockType : ObjectGraphType<GamesDataClocks> 
    {
        public GamesDataClockType()
        {
            Field<GamesDataType>(
                "game",
                description: "The game this bank belongs to",
                resolve: _ => ((GamesDataClocks)_.Source).Game
            );
            Field<UserType>(
                "user",
                description: "The player this bank belongs to",
                resolve: _ => ((GamesDataClocks)_.Source).Owner
            );
            Field<IntGraphType>(
                "bank",
                description: "The amount of time currently banked",
                resolve: _ => (int)((GamesDataClocks)_.Source).Bank
            );
        }
    }
}
