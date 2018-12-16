using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used by the authenticated mutator for accepting/withdrawing from challenges
    public class NewChatInputType : InputObjectGraphType
    {
        public NewChatInputType()
        {
            Name = "NewChatInput";
            Description = "The input required when posting a chat to a game";
            Field<NonNullGraphType<StringGraphType>>("id", description: "The game's ID number");
            Field<NonNullGraphType<StringGraphType>>("message", description: "The message you wish to post.");
        }
    }

    public class NewChatDTO
    {
        public string id {get; set;}
        public string message {get; set;}
    }
}
