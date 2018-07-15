using System.Linq;
using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class MeType : UserType
    {
        public MeType(): base() 
        {
            Field(x => x.Anonymous).Name("anonymous").Description("Whether the account is flagged as anonymous");
            Field<ListGraphType<ChallengeType>>(
                "challenged",
                description: "List of challenges extended directly to you",
                resolve: _ => ((Owners)_.Source).ChallengesPlayers.Where(x => !x.Confirmed).Select(x => x.Challenge).ToArray()
            );
        }
    }
}