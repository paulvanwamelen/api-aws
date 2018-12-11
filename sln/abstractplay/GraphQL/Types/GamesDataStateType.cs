using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class GamesDataStateType : ObjectGraphType<GamesDataStates> 
    {
        public GamesDataStateType()
        {
            Field<StringGraphType>(
                "id",
                description: "This state's unique ID",
                resolve: _ => GuidGenerator.HelperBAToString(((GamesDataStates)_.Source).StateId)
            );
            Field<GamesDataType>(
                "game",
                description: "The game this state is associated with",
                resolve: _ => ((GamesDataStates)_.Source).Game
            );
            Field(x => x.State).Name("state").Description("The text of the state itself");
            Field(x => x.RenderRep).Name("renderrep").Description("The render representation of this state");
            Field(x => x.Timestamp).Name("timestamp").Description("The date and time this state was registered");
        }
    }
}
