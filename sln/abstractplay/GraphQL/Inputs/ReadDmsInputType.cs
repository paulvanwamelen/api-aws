using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used by the authenticated mutator for handling new DMs
    public class ReadDmsInputType : InputObjectGraphType
    {
        public ReadDmsInputType()
        {
            Name = "ReadDmsInput";
            Description = "The input required when marking DMs as read";
            Field<NonNullGraphType<ListGraphType<StringGraphType>>>("ids", description: "The message ID numbers");
        }
    }

    public class ReadDmsDTO
    {
        public string[] ids {get; set;}
    }
}
