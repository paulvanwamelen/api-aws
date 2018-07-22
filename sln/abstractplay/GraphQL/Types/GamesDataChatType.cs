using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class GamesDataChatType : ObjectGraphType<GamesDataChats> 
    {
        public GamesDataChatType()
        {
            Field<StringGraphType>("id", description: "This entry's unique ID number", resolve: _ => GuidGenerator.HelperBAToString(((GamesDataChats)_.Source).ChatId));
            Field<GamesDataType>(
                "game",
                description: "The game in which this chat was posted",
                resolve: _ => ((GamesDataChats)_.Source).Game
            );
            Field<UserType>(
                "user",
                description: "The user who posted the chat",
                resolve: _ => ((GamesDataChats)_.Source).Owner
            );
            Field(x => x.Timestamp).Name("timestamp").Description("The date and time the chat was posted");
            Field(x => x.Message).Name("message").Description("The text of the chat message");
        }
    }
}
