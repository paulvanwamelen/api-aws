using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used by the authenticated mutator for handling console commands
    public class WithdrawConsoleInputType : InputObjectGraphType
    {
        public WithdrawConsoleInputType()
        {
            Name = "WithdrawConsoleInput";
            Description = "The input required when withdrawing a console command";
            Field<NonNullGraphType<StringGraphType>>("id", description: "The command's unique ID number");
        }
    }

    public class WithdrawConsoleDTO
    {
        public string id {get; set;}
    }
}
