using System.Linq;
using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class MeType : ObjectGraphType<Owners>
    {
        public MeType()
        {
            Field<StringGraphType>("id", resolve: _ => GuidGenerator.HelperBAToString(((Owners)_.Source).OwnerId), description: "Your ID number");
            Field<StringGraphType>(
                "name", 
                description: "Your current display name",
                resolve: _ => ((Owners)_.Source).OwnersNames.OrderByDescending(x => x.EffectiveFrom).First().Name
            );
            Field(x => x.DateCreated).Name("created").Description("The date your account was created");
            Field(x => x.Country).Name("country").Description("The country you say you're from");
            Field(x => x.Anonymous).Name("anonymous").Description("Whether your account is flagged as anonymous");
            Field(x => x.Tagline).Name("tagline").Description("your tagline");
            Field<ListGraphType<NameHistoryType>>(
                "nameHistory", 
                description: "Your past display names",
                resolve: _ => ((Owners)_.Source).OwnersNames.ToArray()
            );
            Field<ListGraphType<RankType>>(
                "ranks",
                description: "Ranks you have applied",
                resolve: _ => ((Owners)_.Source).GamesMetaRanks.ToArray()
            );
            Field<ListGraphType<TagType>>(
                "tags", 
                description: "Tags you have applied",
                resolve: _ => ((Owners)_.Source).GamesMetaTags.ToArray()
            );
            Field<ListGraphType<ChallengeType>>(
                "challenges", 
                description: "Challenges you have issued",
                resolve: _ => ((Owners)_.Source).Challenges.ToArray()
            );
            Field<ListGraphType<ChallengeType>>(
                "challenged",
                description: "Challenges extended directly to you",
                resolve: _ => ((Owners)_.Source).ChallengesPlayers.Where(x => !x.Confirmed).Select(x => x.Challenge).ToArray()
            );
            Field<ListGraphType<GamesDataClockType>>(
                "banks",
                description: "List of your clock banks",
                resolve: _ => ((Owners)_.Source).GamesDataClocks.ToArray()
            );
            Field<ListGraphType<GamesDataType>>(
                "games",
                description: "The games you are currently playing",
                resolve: _ =>((Owners)_.Source).GamesDataPlayers.Select(x => (GamesData)x.Game).ToArray()
            );
            Field<ListGraphType<GamesDataType>>(
                "myTurn",
                description: "The games in which you can currently move",
                resolve: _ => ((Owners)_.Source).GamesDataWhoseturn.Select(x => (GamesData)x.Game).ToArray()
            );
            Field<ListGraphType<DmsType>>(
                "dms",
                description: "List of direct messages received",
                resolve: _ => ((Owners)_.Source).DmsReceiver.ToArray()
            );
            Field<ListGraphType<DmsType>>(
                "newDms",
                description: "List of unread direct messages",
                resolve: _ => ((Owners)_.Source).DmsReceiver.Where(x => x.Read.Equals(false)).ToArray()
            );
        }
    }
}