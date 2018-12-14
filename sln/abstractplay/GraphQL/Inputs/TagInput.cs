using GraphQL.Types;

namespace abstractplay.GraphQL
{
    public class TagInputType : InputObjectGraphType
    {
        public TagInputType()
        {
            Name = "TagInput";
            Description = "The individual input required when updating game tags";
            Field<NonNullGraphType<StringGraphType>>("game", description: "The ID of the game being tagged");
            Field<NonNullGraphType<ListGraphType<StringGraphType>>>("tags", description: "The tags assigned to this game");
        }
    }

    public class TagDTO
    {
        public string game {get; set;}
        public string[] tags {get; set;}
    }
}
