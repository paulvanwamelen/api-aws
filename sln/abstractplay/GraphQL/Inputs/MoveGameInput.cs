using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used by the authenticated mutator for handling submitted moves
    public class MoveGameInputType : InputObjectGraphType
    {
        public MoveGameInputType()
        {
            Name = "MoveGameInput";
            Description = "The input required when submitting move requests";
            Field<NonNullGraphType<StringGraphType>>("id", description: "The game's ID number");
            Field<NonNullGraphType<StringGraphType>>("move", description: "The move itself, using whatever notation the game supports");
        }
    }

    public class MoveGameDTO
    {
        public string id {get; set;}
        public string move {get; set;}
    }
}
