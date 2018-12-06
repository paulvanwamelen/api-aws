using System;
using System.Linq;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Amazon.Lambda.Core;

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
            Field<GamesDataType>(
                "createGame",
                description: "Create a new game",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<NewGameInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    LambdaLogger.Log("In resolver");
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<NewGameInputDTO>("input");
                    LambdaLogger.Log("Context and input processed");

                    byte[] newgameid = GuidGenerator.GenerateSequentialGuid();
                    var game = new GamesData
                    {
                        EntryId = newgameid,
                        GameMetaId = db.GamesMeta.Single(x => x.Shortcode.Equals(input.shortcode)).GameId,
                        Closed = false,
                        Alert = false,
                        ClockFrozen = false,
                        ClockStart = (ushort)input.clockStart,
                        ClockInc = (ushort)input.clockInc,
                        ClockMax = (ushort)input.clockMax
                    };
                    LambdaLogger.Log("GamesData object initialized");
                    //variants
                    if (input.variants.Length > 0)
                    {
                        game.Variants = String.Join('|', input.variants);
                    }
                    LambdaLogger.Log("Variants added");
                    //chat
                    if (! String.IsNullOrWhiteSpace(input.chat))
                    {
                        var chat = new GamesDataChats
                        {
                            ChatId = GuidGenerator.GenerateSequentialGuid(),
                            GameId = newgameid,
                            Message = input.chat
                        };
                        game.GamesDataChats.Add(chat);
                    }
                    //players
                    foreach (var player in input.players)
                    {
                        var owner = db.Owners.Single(x => x.PlayerId.Equals(GuidGenerator.HelperStringToBA(player)));
                        var rec = new GamesDataPlayers
                        {
                            GameId = newgameid,
                            Owner = owner
                        };
                        game.GamesDataPlayers.Add(rec);
                    }
                    LambdaLogger.Log("Players added");
                    //whoseturn
                    foreach (var player in input.whoseturn)
                    {
                        var owner = db.Owners.Single(x => x.PlayerId.Equals(GuidGenerator.HelperStringToBA(player)));
                        var rec = new GamesDataWhoseturn
                        {
                            GameId = newgameid,
                            Owner = owner
                        };
                        game.GamesDataWhoseturn.Add(rec);
                    }
                    LambdaLogger.Log("Whoseturn added");
                    //clocks
                    foreach (var player in input.players)
                    {
                        var owner = db.Owners.Single(x => x.PlayerId.Equals(GuidGenerator.HelperStringToBA(player)));
                        var rec = new GamesDataClocks
                        {
                            GameId = newgameid,
                            Owner = owner,
                            Bank = (short)input.clockStart
                        };
                        game.GamesDataClocks.Add(rec);
                    }
                    LambdaLogger.Log("Clocks added");
                    //state
                    var state = new GamesDataStates
                    {
                        StateId = GuidGenerator.GenerateSequentialGuid(),
                        GameId = newgameid,
                        State = input.state,
                        RenderRep = input.renderrep
                    };
                    game.GamesDataStates.Add(state);

                    db.GamesData.Add(game);
                    db.SaveChanges();
                    return game;
                }
            );
        }
    }
}