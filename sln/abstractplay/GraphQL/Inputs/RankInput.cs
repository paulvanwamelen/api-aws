using GraphQL.Types;

namespace abstractplay.GraphQL
{
    public class RankInputType : InputObjectGraphType
    {
        public RankInputType()
        {
            Name = "RankInput";
            Description = "The individual input required when updating your game rankings";
            Field<NonNullGraphType<StringGraphType>>("game", description: "The game ID of the game being ranked");
            Field<NonNullGraphType<IntGraphType>>("rank", description: "The rank assigned to this game");
        }
    }

    public class RankDTO
    {
        public string game {get; set;}
        public ushort rank {get; set;}
    }
}
