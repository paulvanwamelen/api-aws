using GraphQL.Types;

namespace abstractplay.GraphQL
{
    public class GameMetadataInputType : InputObjectGraphType
    {
        public GameMetadataInputType()
        {
            Name = "GameMetadataInput";
            Description = "The input required when updating game metadata";
            Field<StringGraphType>("shortcode");
            Field<StringGraphType>("state");
            Field<StringGraphType>("version");
            Field<StringGraphType>("description");
            Field<StringGraphType>("changelog");
            Field<ListGraphType<IntGraphType>>("playercounts");
            Field<ListGraphType<VariantInputType>>("variants");
        }
    }

    public class GameMetadataDTO
    {
        public string shortcode {get; set;}
        public string state {get; set;}
        public string version {get; set;}
        public string description {get; set;}
        public string changelog {get; set;}
        public int[] playercounts {get; set;}
        public VariantInputDTO[] variants {get; set;}
    }
}
