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
            Field<GamesDataType>(
                "createGame",
                description: "Create a new game",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<NewGameInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<NewGameInputDTO>("input");

                    byte[] newgameid = GuidGenerator.GenerateSequentialGuid();
                    var game = new GamesData
                    {
                        EntryId = newgameid,
                        GameMetaId = db.GamesMeta.Single(x => x.Shortcode.Equals(input.shortcode)).GameId,
                        Closed = false,
                        Alert = false,
                        ClockFrozen = false,
                        ClockStart = input.clockStart,
                        ClockInc = input.clockInc,
                        ClockMax = input.clockMax
                    };
                    //variants
                    if (input.variants.Length > 0)
                    {
                        game.Variants = String.Join('|', input.variants);
                    }
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
                    //whoseturn
                    foreach (var player in input.players)
                    {
                        var owner = db.Owners.Single(x => x.PlayerId.Equals(GuidGenerator.HelperStringToBA(player)));
                        var rec = new GamesDataWhoseturn
                        {
                            GameId = newgameid,
                            Owner = owner
                        };
                        game.GamesDataWhoseturn.Add(rec);
                    }
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