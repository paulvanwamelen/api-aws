using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used when adding or removing tags
    public class UpdateTagsInputType : InputObjectGraphType
    {
        public UpdateTagsInputType()
        {
            Name = "UpdateTagsInput";
            Description = "The input required when updating your game tags";
            Field<NonNullGraphType<ListGraphType<TagInputType>>>("tags", description: "The individual games and their tags");
        }
    }

    public class UpdateTagsDTO
    {
        public TagDTO[] tags {get; set;}
    }
}