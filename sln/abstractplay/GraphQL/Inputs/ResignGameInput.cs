using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used by the authenticated mutator for handling resignations
    public class ResignGameInputType : InputObjectGraphType
    {
        public ResignGameInputType()
        {
            Name = "ResignGameInput";
            Description = "The input required when resigning from games";
            Field<NonNullGraphType<StringGraphType>>("id", description: "The game's ID number");
            Field<NonNullGraphType<BooleanGraphType>>("confirmed", description: "To proceed, must be true. This action cannot be undone.");
        }
    }

    public class ResignGameDTO
    {
        public string id {get; set;}
        public bool confirmed {get; set;}
    }
}
