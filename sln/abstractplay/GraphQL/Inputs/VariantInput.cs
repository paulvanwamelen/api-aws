using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used by the SNS mutator only to update game metadata
    public class VariantInputType : InputObjectGraphType
    {
        public VariantInputType()
        {
            Name = "VariantInput";
            Description = "Defines a variant in a game's metadata";
            Field<StringGraphType>("name");
            Field<StringGraphType>("note");
            Field<StringGraphType>("group");
        }
    }

    public class VariantInputDTO
    {
        public string name {get; set;}
        public string note {get; set;}
        public string group {get; set;}
    }
}
