using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used by the authenticated mutator for handling new DMs
    public class DeleteDmsInputType : InputObjectGraphType
    {
        public DeleteDmsInputType()
        {
            Name = "DeleteDmsInput";
            Description = "The input required when deleting DMs";
            Field<NonNullGraphType<ListGraphType<StringGraphType>>>("ids", description: "The message ID numbers");
        }
    }

    public class DeleteDmsDTO
    {
        public string[] ids {get; set;}
    }
}
