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
        private void GamesData(MyContext db)
        {
            Field<GamesDataType>(
                "moveGame",
                description: "Submit a move to a game",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<MoveGameInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<MoveGameDTO>("input");
 
                    var user = db.Owners.SingleOrDefault(x => x.CognitoId.Equals(context.cognitoId));
                    if (user == null)
                    {
                        throw new ExecutionError("You don't appear to have a user account! Only registered users can play.");
                    }

                    byte[] binaryid;
                    try
                    {
                        binaryid = GuidGenerator.HelperStringToBA(input.id);
                    }
                    catch
                    {
                        throw new ExecutionError("The game ID you provided is malformed. Please verify and try again.");
                    }

                    //Does this game id exist?
                    var game = db.GamesData.SingleOrDefault(x => x.EntryId.Equals(binaryid));
                    if (game == null)
                    {
                        throw new ExecutionError("The game id you provided ("+ input.id +") does not appear to exist.");
                    }

                    if (game.Closed)
                    {
                        throw new ExecutionError("This game has ended. No further moves are possible.");
                    }

                    //Load the latest game state
                    Game gameobj;
                    try
                    {
                        gameobj = GameFactory.LoadGame(game.GameMeta.Shortcode, game.GamesDataStates.Last().State);
                    }
                    catch (Exception e)
                    {
                        throw new ExecutionError("An error occurred while trying to load the game. Please alert the administrators. The game code said the following: " + e.Message);
                    }

                    //Is the move legal?
                    Game newgameobj;
                    try
                    {
                        newgameobj = gameobj.Move(GuidGenerator.HelperBAToString(user.PlayerId), input.move);
                    }
                    catch (Exception e)
                    {
                        throw new ExecutionError("Your move was not accepted. The game code said the following: " + e.Message);
                    }

                    //Build game object
                    var uginput = new UpdateGameInput
                    {
                        Gameobj = newgameobj,
                        Gamerec = game,
                        Mover = user.OwnerId
                    };
                    DBFuncs.UpdateGame(db, uginput);
                    db.SaveChanges();

                    return game;
                }
            );
            Field<GamesDataPreviewType>(
                "moveGamePreview",
                description: "Preview a move to a game",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<MoveGameInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<MoveGameDTO>("input");
 
                    var user = db.Owners.SingleOrDefault(x => x.CognitoId.Equals(context.cognitoId));
                    if (user == null)
                    {
                        throw new ExecutionError("You don't appear to have a user account! Only registered users can play.");
                    }

                    byte[] binaryid;
                    try
                    {
                        binaryid = GuidGenerator.HelperStringToBA(input.id);
                    }
                    catch
                    {
                        throw new ExecutionError("The game ID you provided is malformed. Please verify and try again.");
                    }

                    //Does this game id exist?
                    var game = db.GamesData.SingleOrDefault(x => x.EntryId.Equals(binaryid));
                    if (game == null)
                    {
                        throw new ExecutionError("The game id you provided ("+ input.id +") does not appear to exist.");
                    }

                    if (game.Closed)
                    {
                        throw new ExecutionError("This game has ended. No further moves are possible.");
                    }

                    //Load the latest game state
                    Game gameobj;
                    try
                    {
                        gameobj = GameFactory.LoadGame(game.GameMeta.Shortcode, game.GamesDataStates.Last().State);
                    }
                    catch (Exception e)
                    {
                        throw new ExecutionError("An error occurred while trying to load the game. Please alert the administrators. The game code said the following: " + e.Message);
                    }

                    //Is the move legal?
                    Game newgameobj;
                    try
                    {
                        newgameobj = gameobj.Move(GuidGenerator.HelperBAToString(user.PlayerId), input.move);
                    }
                    catch (Exception e)
                    {
                        throw new ExecutionError("Your move was not accepted. The game code said the following: " + e.Message);
                    }

                    return newgameobj;
                }
            );
            Field<GamesDataChatType>(
                "newChat",
                description: "Post a new chat message to a game",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<NewChatInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<NewChatDTO>("input");
 
                    var user = db.Owners.SingleOrDefault(x => x.CognitoId.Equals(context.cognitoId));
                    if (user == null)
                    {
                        throw new ExecutionError("You don't appear to have a user account! Only registered users can chat in games.");
                    }

                    byte[] binaryid;
                    try
                    {
                        binaryid = GuidGenerator.HelperStringToBA(input.id);
                    }
                    catch
                    {
                        throw new ExecutionError("The game ID you provided is malformed. Please verify and try again.");
                    }

                    //Does this game id exist?
                    var game = db.GamesData.SingleOrDefault(x => x.EntryId.Equals(binaryid));
                    if (game == null)
                    {
                        throw new ExecutionError("The game id you provided ("+ input.id +") does not appear to exist.");
                    }

                    var rec = new GamesDataChats
                    {
                        ChatId = GuidGenerator.GenerateSequentialGuid(),
                        GameId = binaryid,
                        OwnerId = user.OwnerId,
                        Message = input.message
                    };
                    db.GamesDataChats.Add(rec);
                    db.SaveChanges();
                    return rec;
                }
            );
        }
    }
}