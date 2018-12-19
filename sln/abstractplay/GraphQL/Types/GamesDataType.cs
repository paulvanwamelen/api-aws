using System.Linq;
using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class GamesDataType : ObjectGraphType<GamesData> 
    {
        public GamesDataType()
        {
            Field<StringGraphType>(
                "id", 
                description: "The game's unique ID number", 
                resolve: _ => GuidGenerator.HelperBAToString(((GamesData)_.Source).EntryId)
            );
            Field<GamesMetaType>(
                "type", 
                description: "The type of game this is",
                resolve: _ => ((GamesData)_.Source).GameMeta
            );
            Field(x => x.Closed).Name("closed").Description("Whether the game is over");
            Field(x => x.Alert).Name("alert").Description("Whether the site admin has been called to review the game");
            Field<ListGraphType<StringGraphType>>(
                "variants", 
                description: "The variants in effect for this game", 
                resolve: _ => {
                    var rec = ((GamesData)_.Source).Variants;
                    if (rec != null)
                    {
                        return rec.Split('\n');
                    }
                    else
                    {
                        return null;
                    }
                }
            );
            Field<ListGraphType<GamesDataChatType>>(
                "chats",
                description: "The chats associated with this game, in ascending chronological order",
                resolve: _ => ((GamesData)_.Source).GamesDataChats.OrderBy(x => x.Timestamp).ToArray()
            );
            Field<IntGraphType>(
                "clockStart", 
                description: "The game clock's starting value, in hours", 
                resolve: _ => (int)((GamesData)_.Source).ClockStart
            );
            Field<IntGraphType>(
                "clockInc", 
                description: "The game clock's increment value, in hours", 
                resolve: _ => (int)((GamesData)_.Source).ClockInc
            );
            Field<IntGraphType>(
                "clockMax", 
                description: "The game clock's maximum value, in hours", 
                resolve: _ => (int)((GamesData)_.Source).ClockMax
            );
            Field(x => x.ClockFrozen).Name("clockFrozen").Description("Whether this game clock is currently frozen");
            Field<ListGraphType<GamesDataClockType>>(
                "banks",
                description: "The clock banks associated with this game",
                resolve: _ => ((GamesData)_.Source).GamesDataClocks.ToArray()
            );
            Field<ListGraphType<UserType>>(
                "players",
                description: "The users playing this game",
                resolve: _ => ((GamesData)_.Source).GamesDataPlayers.Select(x => (Owners)x.Owner).ToArray()
            );
            Field<ListGraphType<GamesDataStateType>>(
                "states",
                description: "This game's states in ascending chronological order",
                resolve: _ => ((GamesData)_.Source).GamesDataStates.OrderBy(x => x.Timestamp).ToArray()
            );
            Field<GamesDataStateType>(
                "lastState",
                description: "This game's most recent state",
                resolve: _ => ((GamesData)_.Source).GamesDataStates.Last()
            );
            Field<ListGraphType<UserType>>(
                "whoseTurn",
                description: "The list of players who can currently move in this game",
                resolve: _ => ((GamesData)_.Source).GamesDataWhoseturn.Select(x => (Owners)x.Owner).ToArray()
            );
        }
    }
}