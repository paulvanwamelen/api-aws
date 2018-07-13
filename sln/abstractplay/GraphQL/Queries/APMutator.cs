//using System.Linq;
using GraphQL.Types;
//using Microsoft.EntityFrameworkCore;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    /*
     * This is the SNS-only mutator.
     */
    public class APMutator : ObjectGraphType
    {
        public APMutator(MyContext db)
        {
            Field<GamesMetaType>(
                "updateGameMetadata",
                description: "Update a game's metadata",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<GameMetadataInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var profile = _.GetArgument<GameMetadataDTO>("input");
                    //TODO
                    return null;
                }
            );
        }
    }
}