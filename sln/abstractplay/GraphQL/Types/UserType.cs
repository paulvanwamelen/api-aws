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
            Field<StringGraphType>(
                "name", 
                description: "User's current display name",
                resolve: _ => 
                {
                    var rec = (Owners)_.Source;
                    if (rec.Anonymous)
                    {
                        return null;
                    } else {
                        return rec.OwnersNames.OrderByDescending(x => x.EffectiveFrom).First().Name;
                    }
                }
            );
            Field<DateGraphType>(
                "created",
                description: "The date this account was created",
                resolve: _ => 
                {
                    var rec = (Owners)_.Source;
                    if (rec.Anonymous)
                    {
                        return null;
                    } else {
                        return rec.DateCreated;
                    }
                }
            );
            Field<StringGraphType>(
                "country",
                description: "The country the user says they're from",
                resolve: _ => 
                {
                    var rec = (Owners)_.Source;
                    if (rec.Anonymous)
                    {
                        return null;
                    } else {
                        return rec.Country;
                    }
                }
            );
            Field<StringGraphType>(
                "tagline",
                description: "The user's tagline",
                resolve: _ => 
                {
                    var rec = (Owners)_.Source;
                    if (rec.Anonymous)
                    {
                        return null;
                    } else {
                        return rec.Tagline;
                    }
                }
            );
            Field<ListGraphType<NameHistoryType>>(
                "nameHistory", 
                description: "Past display names this player has used",
                resolve: _ => 
                {
                    var rec = (Owners)_.Source;
                    if (rec.Anonymous)
                    {
                        return null;
                    } else {
                        return rec.OwnersNames.ToArray();
                    }
                }
            );
            Field<ListGraphType<TagType>>(
                "tags", 
                description: "The tags this user has applied",
                resolve: _ => 
                {
                    var rec = (Owners)_.Source;
                    if (rec.Anonymous)
                    {
                        return null;
                    } else {
                        return rec.GamesMetaTags.ToArray();
                    }
                }
            );
            Field<ListGraphType<ChallengeType>>(
                "challenges", 
                description: "Challenges this user has issued",
                resolve: _ => 
                {
                    var rec = (Owners)_.Source;
                    if (rec.Anonymous)
                    {
                        return null;
                    } else {
                        return rec.Challenges.ToArray();
                    }
                }
            );
        }
    }
}
