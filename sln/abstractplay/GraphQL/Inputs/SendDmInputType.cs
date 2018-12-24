using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used by the authenticated mutator for handling new DMs
    public class SendDmInputType : InputObjectGraphType
    {
        public SendDmInputType()
        {
            Name = "SendDmInput";
            Description = "The input required when sending a DM";
            Field<NonNullGraphType<StringGraphType>>("recipient", description: "The recipient's ID number");
            Field<NonNullGraphType<StringGraphType>>("message", description: "Markdown-encoded message; maximum 65,535 characters");
        }
    }

    public class SendDmDTO
    {
        public string recipient {get; set;}
        public string message {get; set;}
    }
}
