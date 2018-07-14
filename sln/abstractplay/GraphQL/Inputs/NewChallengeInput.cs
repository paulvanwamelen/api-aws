using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used by the SNS mutator only to update game metadata
    public class NewChallengeInputType : InputObjectGraphType
    {
        public NewChallengeInputType()
        {
            Name = "NewChallengeInput";
            Description = "The input required when issuing a new challenge";
            Field<NonNullGraphType<StringGraphType>>("game", description: "The game you want to play");
            Field<NonNullGraphType<IntGraphType>>("numPlayers", description: "The total number of players, including yourself");
            Field<StringGraphType>("notes", description: "Any notes you want potential challengers to see");
            Field<IntGraphType>("clockStart", description: "Game clock starting value, in hours (default: 72)");
            Field<IntGraphType>("clockInc", description: "Game clock increment value, in hours (default: 24)");
            Field<IntGraphType>("clockMax", description: "Game clock maximum value, in hours (default: 240)");
            Field<IntGraphType>("seat", description: "Only considered for two-player games; a value of 1 means you want to go first; a value of 2 means your opponent will go first");
            Field<NonNullGraphType<ListGraphType<StringGraphType>>>("variants", description: "A list of supported variants you wish to apply to this game");
            Field<NonNullGraphType<ListGraphType<StringGraphType>>>("challengees", description: "A list of player IDs you wish to directly challenge; any remaining seats will be available to anyone");
        }
    }

    public class NewChallengeDTO
    {
        public string game {get; set;}
        public int numPlayers {get; set;}
        public string notes {get; set;}
        public int? clockStart {get; set;}
        public int? clockInc {get; set;}
        public int? clockMax {get; set;}
        public int? seat {get; set;}
        public string[] variants {get; set;}
        public string[] challengees {get; set;}
    }
}
