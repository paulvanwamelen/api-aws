using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used when *creating* profiles
    public class NewProfileInputType : InputObjectGraphType
    {
        public NewProfileInputType()
        {
            Name = "NewProfileInput";
            Description = "The input required when creating a new profile.";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<NonNullGraphType<BooleanGraphType>>("consent");
            Field<BooleanGraphType>("anonymous");
            Field<StringGraphType>("country");
            Field<StringGraphType>("tagline");
        }
    }

    public class NewProfileDTO
    {
        public string name {get; set;}
        public bool consent {get; set;}
        public bool anonymous {get; set;}
        public string country {get; set;}
        public string tagline {get; set;}
    }
}