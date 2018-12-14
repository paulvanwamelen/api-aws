using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used when *creating* profiles
    public class UpdateRankingsInputType : InputObjectGraphType
    {
        public UpdateRankingsInputType()
        {
            Name = "UpdateRankingsInput";
            Description = "The input required when updating your game rankings";
            Field<NonNullGraphType<ListGraphType<RankInputType>>>("rankings", description: "The individual games and their ranks");
        }
    }

    public class UpdateRankingsDTO
    {
        public RankDTO[] rankings {get; set;}
    }
}