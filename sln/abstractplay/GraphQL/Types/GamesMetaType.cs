using System.Linq;
using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class GamesMetaType : ObjectGraphType<GamesMeta>
    {
        public GamesMetaType()
        {
            Field<StringGraphType>("id", resolve: _ => GuidGenerator.HelperBAToString(((GamesMeta)_.Source).GameId), description: "Game's unique ID number");
            Field(x => x.Shortcode).Name("shortcode").Description("The game's short name (camel cased, no spaces)");
            Field(x => x.Name).Name("name").Description("The game's full name");
            Field(x => x.LiveDate, nullable: true).Name("dateLive").Description("The date the game first went live");
            Field(x => x.Description).Name("description").Description("A long-form, markdown-formatted description of the game");
            Field(x => x.IsLive).Name("isLive").Description("Whether the game is currently available for play");
            Field(x => x.PlayerCounts).Name("playerCounts").Description("List of the different player counts the game supports");
            Field(x => x.Version).Name("version").Description("The current version number of the game code");
            Field(x => x.Changelog, nullable: true).Name("changelog").Description("Markdown-formatted log of changes made to the game code over time");
            Field<PublisherType>("publisher", resolve: _ => ((GamesMeta)_.Source).Publisher, description: "The game's publisher");
            Field<ListGraphType<VariantType>>("variants", resolve: _ => ((GamesMeta)_.Source).GamesMetaVariants.ToArray(), description: "The game's available variants (doesn't include built-in, universal variants)");
            Field<ListGraphType<TagType>>("tags", resolve: _ => ((GamesMeta)_.Source).GamesMetaTags, description: "The tags applied to this game");
        }
    }
}