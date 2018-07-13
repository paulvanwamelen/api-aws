using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used when *updating* profiles
    public class PatchProfileInputType : InputObjectGraphType
    {
        public PatchProfileInputType()
        {
            Name = "PatchProfileInput";
            Description = "The input required when updating your profile";
            Field<StringGraphType>("name");
            Field<StringGraphType>("country");
            Field<StringGraphType>("tagline");
            Field<BooleanGraphType>("anonymous");
        }
    }

    public class PatchProfileDTO
    {
        public string name {get; set;}
        public string country {get; set;}
        public string tagline {get; set;}
        public bool? anonymous {get; set;}
    }
}
