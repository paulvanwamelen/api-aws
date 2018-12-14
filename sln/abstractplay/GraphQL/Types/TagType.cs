using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class TagType : ObjectGraphType<GamesMetaTags>
    {
        public TagType()
        {
            Field<StringGraphType>("id", resolve: _ => GuidGenerator.HelperBAToString(((GamesMetaTags)_.Source).EntryId), description: "Tag entry's unique ID number");
            Field(x => x.Tag).Name("tag").Description("The tag itself");
            Field<GamesMetaType>("game", resolve: _ => ((GamesMetaTags)_.Source).Game, description: "The game the tag applies to");
            Field<UserType>("user", resolve: _ => ((GamesMetaTags)_.Source).Owner, description: "The user who applied the tag");
        }
    }
}