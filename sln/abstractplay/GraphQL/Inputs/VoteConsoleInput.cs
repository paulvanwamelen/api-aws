using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used by the authenticated mutator for handling console commands
    public class VoteConsoleInputType : InputObjectGraphType
    {
        public VoteConsoleInputType()
        {
            Name = "VoteConsoleInput";
            Description = "The input required when voting on a console command";
            Field<NonNullGraphType<StringGraphType>>("id", description: "The command's unique ID number");
            Field<NonNullGraphType<BooleanGraphType>>("vote", description: "Your yes/no vote");
        }
    }

    public class VoteConsoleDTO
    {
        public string id {get; set;}
        public bool vote {get; set;}
    }
}
