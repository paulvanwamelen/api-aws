using System;
using System.Linq;
using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class ChallengeType : ObjectGraphType<Challenges>
    {
        public ChallengeType(): base() 
        {
            Field<StringGraphType>("id", description: "This challenge's unique ID", resolve: _ => GuidGenerator.HelperBAToString(((Challenges)_.Source).ChallengeId));
            Field<GamesMetaType>("game", resolve: _ => ((Challenges)_.Source).Game, description:"The game type of this challenge");
            Field<UserType>("issuer", resolve: _ => ((Challenges)_.Source).Owner, description: "The player who issued the challenge");
            Field<IntGraphType>("numPlayers", description: "The number of players", resolve: _ => (int)((Challenges)_.Source).NumPlayers);
            Field(x => x.DateIssued).Name("dateIssued").Description("The date the challenge was issued");
            Field(x => x.Notes).Name("notes").Description("Free-form notes the issuer included with the challenge");
            Field<IntGraphType>("clockStart", description: "The game clock's starting value, in hours", resolve: _ => (int)((Challenges)_.Source).ClockStart);
            Field<IntGraphType>("clockInc", description: "The game clock's increment value, in hours", resolve: _ => (int)((Challenges)_.Source).ClockInc);
            Field<IntGraphType>("clockMax", description: "The game clock's maximum value, in hours", resolve: _ => (int)((Challenges)_.Source).ClockMax);
            Field<ListGraphType<StringGraphType>>("variants", description: "List of variants applied to this challenge", resolve: _ => ((Challenges)_.Source).Variants.Split('|'));
            Field<ListGraphType<UserType>>("players", description:"List of players who have so far accepted the challenge", resolve: _ => ((Challenges)_.Source).ChallengesPlayers.ToArray());
        }
    }
}