using GraphQL.Types;

namespace abstractplay.GraphQL
{
    public class GameStatusInputType : InputObjectGraphType
    {
        public GameStatusInputType()
        {
            Name = "GameStatusInput";
            Description = "The input required when updating a game's status";
            Field<NonNullGraphType<StringGraphType>>("shortcode");
            Field<NonNullGraphType<BooleanGraphType>>("isUp");
            Field<StringGraphType>("message");
        }
    }

    public class GameStatusDTO
    {
        public string shortcode {get; set;}
        public bool isUp {get; set;}
        public string message {get; set;}
    }
}
