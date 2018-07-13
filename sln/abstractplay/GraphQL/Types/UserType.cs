using System.Linq;
using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class UserType : ObjectGraphType<Owners>
    {
        public UserType()
        {
            Field<StringGraphType>("id", resolve: _ => GuidGenerator.HelperBAToString(((Owners)_.Source).OwnerId), description: "User's ID number");
            Field<StringGraphType>("name", resolve: _ => ((Owners)_.Source).OwnersNames.OrderByDescending(x => x.EffectiveFrom).First().Name, description: "User's current display name.");
            Field(x => x.DateCreated, nullable: true).Name("created").Description("The date this account was created.");
            Field(x => x.Country, nullable: true).Name("country").Description("The ISO Alpha-2 code of the country the user says they're from.");
            Field(x => x.Tagline, nullable: true).Name("tagline").Description("The user's tagline.");
            Field<ListGraphType<NameHistoryType>>("nameHistory", resolve: _ => ((Owners)_.Source).OwnersNames.ToArray(), description: "Past display names this player has used");
            Field<ListGraphType<TagType>>("tags", resolve: _ => ((Owners)_.Source).GamesMetaTags, description: "The tags this user has applied");
        }
    }
}
