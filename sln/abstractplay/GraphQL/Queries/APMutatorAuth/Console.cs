using System;
using GraphQL;
using GraphQL.Types;
using System.Linq;

using abstractplay.DB;
using abstractplay.Games;
using Amazon.Lambda.Core;

namespace abstractplay.GraphQL
{
    /*
     * This is the mutator for authenticated users.
     */
    public partial class APMutatorAuth : ObjectGraphType
    {
        private void Console(MyContext db)
        {
            Field<GamesDataType>(
                "newConsole",
                description: "Issue a new console command to a game",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<NewConsoleInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<NewConsoleDTO>("input");
 
                    var user = db.Owners.SingleOrDefault(x => x.CognitoId.Equals(context.cognitoId));
                    if (user == null)
                    {
                        throw new ExecutionError("You don't appear to have a user account! Only registered users can play.");
                    }

                    if (! Enum.IsDefined(typeof(ConsoleCommands), input.command))
                    {
                        throw new ExecutionError("The command you issued was unrecognized. This shouldn't happen if you issued the request through the official front end.");
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
                        throw new ExecutionError("This game has ended. No further actions are possible.");
                    }

                    //For some reason, `.Any(x => x.OwnerId.Equals(user.OwnerId))` is not working. I don't know why yet.
                    if (! game.GamesDataPlayers.Select(x => x.Owner).Contains(user))
                    {
                        throw new ExecutionError("You're not participating in this game and so cannot issue console commands.");
                    }

                    //Command-specific error checks
                    switch ((ConsoleCommands)input.command)
                    {
                        case ConsoleCommands.DRAW:
                            break;
                        case ConsoleCommands.FREEZE:
                            if (game.ClockFrozen)
                            {
                                throw new InvalidOperationException("The clocks are already frozen.");
                            }
                            break;
                        case ConsoleCommands.THAW:
                            if (! game.ClockFrozen)
                            {
                                throw new InvalidOperationException("The clocks are not frozen.");
                            }
                            break;
                    }

                    var consolekey = GuidGenerator.GenerateSequentialGuid();
                    var cmd = new Consoles
                    {
                        EntryId = consolekey,
                        GameId = game.EntryId,
                        Game = game,
                        OwnerId = user.OwnerId,
                        Owner = user,
                        Command = input.command
                    };
                    db.Consoles.Add(cmd);

                    var vote = new ConsolesVotes
                    {
                        EntryId = GuidGenerator.GenerateSequentialGuid(),
                        ConsoleId = consolekey,
                        Voter = user.OwnerId,
                        VoterNavigation = user,
                        Vote = true
                    };
                    db.ConsolesVotes.Add(vote);
                    db.SaveChanges();

                    return game;
                }
            );
            Field<GamesDataType>(
                "withdrawConsole",
                description: "Withdraw a console command",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<WithdrawConsoleInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<WithdrawConsoleDTO>("input");
 
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
                        throw new ExecutionError("The command ID you provided is malformed. Please verify and try again.");
                    }

                    //Does this console command exist?
                    var cmd = db.Consoles.SingleOrDefault(x => x.EntryId.Equals(binaryid));
                    if (cmd == null)
                    {
                        throw new ExecutionError("The command id you provided ("+ input.id +") does not appear to exist.");
                    }

                    if (cmd.OwnerId != user.OwnerId)
                    {
                        throw new ExecutionError("Only the person who issued the command can withdraw it.");
                    }

                    GamesData game = cmd.Game;
                    db.Consoles.Remove(cmd);
                    db.SaveChanges();

                    return game;
                }
            );
            Field<GamesDataType>(
                "voteConsole",
                description: "Vote on a console command",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<VoteConsoleInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<VoteConsoleDTO>("input");
 
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
                        throw new ExecutionError("The command ID you provided is malformed. Please verify and try again.");
                    }

                    //Does this console command exist?
                    var cmd = db.Consoles.SingleOrDefault(x => x.EntryId.Equals(binaryid));
                    if (cmd == null)
                    {
                        throw new ExecutionError("The command id you provided ("+ input.id +") does not appear to exist.");
                    }

                    GamesData game = cmd.Game;
                    //Delete any existing votes by this user
                    db.ConsolesVotes.RemoveRange(db.ConsolesVotes.Where(x => x.ConsoleId.Equals(cmd.EntryId) && x.Voter.Equals(user.OwnerId)));
                    //Set their vote
                    var vote = new ConsolesVotes
                    {
                        EntryId = GuidGenerator.GenerateSequentialGuid(),
                        ConsoleId = cmd.EntryId,
                        Console = cmd,
                        Voter = user.OwnerId,
                        VoterNavigation = user,
                        Vote = input.vote
                    };
                    db.ConsolesVotes.Add(vote);
                    db.SaveChanges();

                    //Check if all the votes are in and execute if so
                    if (db.ConsolesVotes.Where(x => x.ConsoleId.Equals(cmd.EntryId)).ToArray().Length == game.GamesDataPlayers.Count)
                    {
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

                        var args = new ExecuteCommandInput
                        {
                            Gameobj = gameobj,
                            Gamerec = game,
                            Console = cmd
                        };
                        DBFuncs.ExecuteCommand(db, args);
                        db.Consoles.Remove(cmd);
                    }

                    db.SaveChanges();
                    return game;
                }
            );
        }
    }
}
