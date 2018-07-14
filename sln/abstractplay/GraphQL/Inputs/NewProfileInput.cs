using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used when *creating* profiles
    public class NewProfileInputType : InputObjectGraphType
    {
        public NewProfileInputType()
        {
            Name = "NewProfileInput";
            Description = "The input required when creating a new profile";
            Field<NonNullGraphType<StringGraphType>>("name", description: "Your desired display name. It must be unique across names currently in use and those recently used.");
            Field<NonNullGraphType<BooleanGraphType>>("consent", description: "Whether you consent to the terms of service and the privacy policy");
            Field<BooleanGraphType>("anonymous", description: "Whether you prefer to remain anonymous; can be changed at any time");
            Field<StringGraphType>("country", description: "The ISO 3166-1 alpha-2 country code of where you want to tell people you're from");
            Field<StringGraphType>("tagline", description: "A free-form tagline (255 characters max)");
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