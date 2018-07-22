using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used by the authenticated mutator for accepting/withdrawing from challenges
    public class RespondChallengeInputType : InputObjectGraphType
    {
        public RespondChallengeInputType()
        {
            Name = "RespondChallengeInput";
            Description = "The input required when accepting or withdrawing from a pending challenge";
            Field<NonNullGraphType<StringGraphType>>("id", description: "The challenge's ID number");
            Field<NonNullGraphType<BooleanGraphType>>("confirmed", description: "True accepts the challenge if not already accepted and does nothing if already accepted. If you are the last person to confirm, the game will immediately start. False withdraws from the challenge if already accepted and does nothing if not already accepted. If the issuer of the challenge withdraws, the challenge itself will be withdrawn, regardless of any other confirmed players.");
        }
    }

    public class RespondChallengeDTO
    {
        public string id {get; set;}
        public bool confirmed {get; set;}
    }
}
