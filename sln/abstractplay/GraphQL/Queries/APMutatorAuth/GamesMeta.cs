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
            Field<ListGraphType<TagType>>(
                "updateTags",
                description: "Update your game tags. The tags you provide replace all existing tags for the given game. This action is therefore idempotent.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<UpdateTagsInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<UpdateTagsDTO>("input");

                    var user = db.Owners.SingleOrDefault(x => x.CognitoId.Equals(context.CognitoId));
                    if (user == null)
                    {
                        throw new ExecutionError("You don't appear to have a user account! Only registered users can tag games.");
                    }

                    //Check for duplicates
                    HashSet<string> uniques = new HashSet<string>(from x in input.tags select x.game);
                    if (uniques.Count != input.tags.Count())
                    {
                        throw new ExecutionError("Your list contained duplicate game IDs. Please fix and resubmit.");
                    }

                    foreach (var g in input.tags)
                    {
                        //Delete all existing tags for this game and user
                        db.GamesMetaTags.RemoveRange(db.GamesMetaTags.Where(x => x.OwnerId.Equals(user.OwnerId) && x.GameId.Equals(GuidGenerator.HelperStringToBA(g.game))));
                        //Add the given tags
                        foreach (var t in g.tags)
                        {
                            var tag = new GamesMetaTags
                            {
                                EntryId = GuidGenerator.GenerateSequentialGuid(),
                                GameId = GuidGenerator.HelperStringToBA(g.game),
                                OwnerId = user.OwnerId,
                                Tag = t
                            };
                            db.GamesMetaTags.Add(tag);
                        }
                    }
                    db.SaveChanges();
                    // TODO:
                    // For some reason, `Game` is not populating automatically in this resolver.
                    // For now, I'm manually populating it, but I need to figure out why lazy loading isn't working.
                    List<GamesMetaTags> retlst = new List<GamesMetaTags>();
                    foreach (var rec in user.GamesMetaTags)
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
                    return retlst.ToArray();
                    // return user.GamesMetaTags.ToArray();
                }
            );
            Field<ListGraphType<RankType>>(
                "updateRankings",
                description: "Update your game rankings. This input completely replaces existing rankings. This action is therefore idempotent.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<UpdateRankingsInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<UpdateRankingsDTO>("input");

                    var user = db.Owners.SingleOrDefault(x => x.CognitoId.Equals(context.CognitoId));
                    if (user == null)
                    {
                        throw new ExecutionError("You don't appear to have a user account! Only registered users can rank games.");
                    }

                    //Check for duplicates
                    HashSet<string> uniques = new HashSet<string>(from x in input.rankings select x.game);
                    if (uniques.Count != input.rankings.Count())
                    {
                        throw new ExecutionError("Your list contained duplicates game IDs. Please fix and resubmit.");
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
