using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{

    public enum ConsoleCommands : ushort { DRAW, FREEZE, THAW };

    public class ConsoleType : ObjectGraphType<Consoles>
    {
        public ConsoleType()
        {
            Field<StringGraphType>("id", resolve: _ => GuidGenerator.HelperBAToString(((Consoles)_.Source).EntryId), description: "Console entry's unique ID number");
            Field<GamesDataType>("game", resolve: _ => ((Consoles)_.Source).Game, description: "The game the console command applies to");
            Field<UserType>("user", resolve: _ => ((Consoles)_.Source).Owner, description: "The user who issued the command");
            Field<DateTimeGraphType>("date", resolve: _ => ((Consoles)_.Source).Timestamp, description: "The date the command was issued");
            Field<ListGraphType<ConsoleVoteType>>("votes", resolve: _ => ((Consoles)_.Source).ConsolesVotes, description: "All submitted votes");
            Field<StringGraphType>(
                "command", 
                description: "The command that was actually issued",
                resolve: _ =>
                {
                    string ret;
                    switch ((ConsoleCommands)((Consoles)_.Source).Command)
                    {
                        case ConsoleCommands.DRAW:
                            ret = "Draw?";
                            break;
                        case ConsoleCommands.FREEZE:
                            ret = "Freeze Clocks";
                            break;
                        case ConsoleCommands.THAW:
                            ret = "Thaw Clocks";
                            break;
                        default:
                            ret = "UNKNOWN! (Should never happen)";
                            break;
                    }
                    return ret;
                }
            );
        }
    }
}