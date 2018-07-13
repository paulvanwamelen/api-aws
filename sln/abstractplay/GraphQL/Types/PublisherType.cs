using System.Linq;
using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class PublisherType : ObjectGraphType<GamesMetaPublishers>
    {
        public PublisherType()
        {
            Field<StringGraphType>("id", resolve: _ => GuidGenerator.HelperBAToString(((GamesMetaPublishers)_.Source).PublisherId), description: "Publisher's unique ID number");
            Field(x => x.Name).Name("name").Description("Publisher's name");
            Field(x => x.Url).Name("url").Description("Publisher's website");
            Field<ListGraphType<GamesMetaType>>("games", resolve: _ => ((GamesMetaPublishers)_.Source).GamesMeta.ToArray(), description: "Games by this publisher");
        }
    }
}