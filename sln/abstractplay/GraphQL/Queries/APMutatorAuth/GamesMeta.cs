using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

using abstractplay.DB;
using abstractplay.Games;

namespace abstractplay.GraphQL
{
    /*
     * This is the mutator for authenticated users.
     */
    public partial class APMutatorAuth : ObjectGraphType
    {
        private void GamesMeta(MyContext db)
        {
            Field<ListGraphType<RankType>>(
                "updateRankings",
                description: "Update your game rankings",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<UpdateRankingsInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<UpdateRankingsDTO>("input");
 
                    var user = db.Owners.SingleOrDefault(x => x.CognitoId.Equals(context.cognitoId));
                    if (user == null)
                    {
                        throw new ExecutionError("You don't appear to have a user account! Only registered users can rank games.");
                    }

                    //Check for duplicates
                    HashSet<string> uniques = new HashSet<string>(from x in input.rankings select x.game);
                    if (uniques.Count != input.rankings.Count())
                    {
                        throw new ExecutionError("Your list contained duplicates. Please fix and resubmit.");
                    }

                    //Delete all existing rankings
                    db.GamesMetaRanks.RemoveRange(db.GamesMetaRanks.Where(x => x.OwnerId.Equals(user.OwnerId)));

                    //Populate with given input
                    foreach (var r in input.rankings)
                    {
                        var rank = new GamesMetaRanks
                        {
                            EntryId = GuidGenerator.GenerateSequentialGuid(),
                            GameId = GuidGenerator.HelperStringToBA(r.game),
                            OwnerId = user.OwnerId,
                            Rank = r.rank
                        };
                        db.GamesMetaRanks.Add(rank);
                    }

                    db.SaveChanges();
                    // TODO:
                    // For some reason, `Game` is not populating automatically in this resolver.
                    // For now, I'm manually populating it, but I need to figure out why lazy loading isn't working.
                    List<GamesMetaRanks> retlst = new List<GamesMetaRanks>();
                    foreach (var rec in user.GamesMetaRanks)
                    {
                        if (rec.GameId == null)
                        {
                            throw new ExecutionError("GameID was null!");
                        }
                        if (rec.Game == null)
                        {
                            rec.Game = db.GamesMeta.Single(x => x.GameId.Equals(rec.GameId));
                        }
                        retlst.Add(rec);
                    }
                    // return user.GamesMetaRanks.ToArray();
                    return retlst.ToArray();
                }
            );
       }
    }
}