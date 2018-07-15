using System;
using System.Linq;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

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
                    var input = _.GetArgument<GameMetadataDTO>("input");

                    var game = db.GamesMeta.Single(x => x.Shortcode.Equals(input.shortcode));
                    game.State = input.state;
                    game.Version = input.version;
                    game.Description = input.description;
                    game.Changelog = input.changelog;
                    game.PlayerCounts = String.Join(',', input.playercounts.Select(x => x.ToString()).ToArray());

                    db.GamesMetaVariants.RemoveRange(game.GamesMetaVariants);
                    foreach (var variant in input.variants)
                    {
                        var newvar = new GamesMetaVariants {
                            GameId = game.GameId,
                            VariantId = GuidGenerator.GenerateSequentialGuid(),
                            Name = variant.name,
                            Note = variant.note,
                            Group = variant.group
                        };
                        game.GamesMetaVariants.Add(newvar);
                    }

                    db.GamesMeta.Update(game);
                    db.SaveChanges();
                    return game;
                }
            );
            Field<GameStatusType>(
                "updateGameStatus",
                description: "Update a game's status",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<GameStatusInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<GameStatusDTO>("input");

                    var game = db.GamesMeta.Single(x => x.Shortcode.Equals(input.shortcode));
                    var status = new GamesMetaStatus {
                        StatusId = GuidGenerator.GenerateSequentialGuid(),
                        GameId = game.GameId,
                        IsUp = input.isUp,
                        Message = input.message
                    };
                    game.GamesMetaStatus.Add(status);
                    db.GamesMeta.Update(game);
                    db.SaveChanges();
                    return status;
                }
            );
        }
    }
}