using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used by the authenticated mutator for handling console commands
    public class NewConsoleInputType : InputObjectGraphType
    {
        public NewConsoleInputType()
        {
            Name = "NewConsoleInput";
            Description = "The input required when issuing new console commands";
            Field<NonNullGraphType<StringGraphType>>("id", description: "The game's ID number");
            Field<NonNullGraphType<IntGraphType>>("command", description: "The command you wish to issue: `0` offers a draw, `1` freezes the clocks, `2` thaws them");
        }
    }

    public class NewConsoleDTO
    {
        public string id {get; set;}
        public ushort command {get; set;}
    }
}
