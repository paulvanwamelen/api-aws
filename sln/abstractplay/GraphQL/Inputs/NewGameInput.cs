using GraphQL.Types;

namespace abstractplay.GraphQL
{
    //Used by the SNS mutator only to create a new game
    public class NewGameInputType : InputObjectGraphType
    {
        public NewGameInputType()
        {
            Name = "NewGameInput";
            Description = "Defines the input required when creating a new game";
            Field<NonNullGraphType<StringGraphType>>("shortcode");
            Field<NonNullGraphType<ListGraphType<StringGraphType>>>("variants");
            Field<NonNullGraphType<ListGraphType<StringGraphType>>>("players");
            Field<NonNullGraphType<ListGraphType<StringGraphType>>>("whoseturn");
            Field<NonNullGraphType<StringGraphType>>("state");
            Field<NonNullGraphType<StringGraphType>>("renderrep");
            Field<StringGraphType>("chat");
            Field<NonNullGraphType<IntGraphType>>("clockStart");
            Field<NonNullGraphType<IntGraphType>>("clockInc");
            Field<NonNullGraphType<IntGraphType>>("clockMax");
        }
    }

    public class NewGameInputDTO
    {
        public string shortcode {get; set;}
        public string[] variants {get; set;}
        public string[] players {get; set;}
        public string[] whoseturn {get; set;}
        public string state {get; set;}
        public string renderrep {get; set;}
        public string chat {get; set;}
        public int clockStart {get; set;}
        public int clockInc {get; set;}
        public int clockMax {get; set;}
    }
}
