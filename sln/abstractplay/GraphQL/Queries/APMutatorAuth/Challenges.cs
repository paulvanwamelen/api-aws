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
        private void Challenges(MyContext db)
        {
            Field<ChallengeType>(
                "issueChallenge",
                description: "Issue a new challenge",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<NewChallengeInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<NewChallengeDTO>("input");

                    var game = db.GamesMeta.SingleOrDefault(x => x.Shortcode.Equals(input.game));
                    if (game == null)
                    {
                        throw new ExecutionError("Could not find a game with the name "+input.game+".");
                    }
                    //Validate numPlayers
                    int[] counts = game.PlayerCounts.Split(',').Select(x => int.Parse(x)).ToArray();
                    if (! counts.Contains(input.numPlayers))
                    {
                        throw new ExecutionError("The number of players you requested ("+input.numPlayers.ToString() + ") is not supported by "+ game.Name +". Only the following are acceptable: " + game.PlayerCounts + ".");
                    }
                    //Set clock to default if necessary
                    if ( (input.clockStart == null) || (input.clockStart < 1) ) 
                    {
                        input.clockStart = 72;
                    }
                    if ( (input.clockInc == null) || (input.clockInc < 1) ) 
                    {
                        input.clockInc = 24;
                    }
                    if ( (input.clockMax == null) || (input.clockMax < 1) ) 
                    {
                        input.clockMax = 240;
                    }
                    //Validate variants
                    List<string> vars = game.GamesMetaVariants.Select(x => x.Name).ToList();
                    vars.Add("Unrated");
                    vars.Add("Hard Time");
                    foreach (var variant in input.variants)
                    {
                        if (! vars.Contains(variant))
                        {
                            throw new ExecutionError("The variant '"+variant+"' is not supported by "+game.Name+".");
                        }
                    }
                    //Validate any challengees (including seat)
                    foreach (var player in input.challengees)
                    {
                        if (! db.Owners.Any(x => x.OwnerId.Equals(GuidGenerator.HelperStringToBA(player))))
                        {
                            throw new ExecutionError("Could not find player ID "+player+".");
                        }
                    }

                    //Build record
                    var user = db.Owners.SingleOrDefault(x => x.CognitoId.Equals(context.cognitoId));
                    if (user == null)
                    {
                        throw new ExecutionError("You do not appear to have a user profile. You must create a profile before playing.");
                    }
                    byte[] challengeId = GuidGenerator.GenerateSequentialGuid();
                    var rec = new Challenges {
                        ChallengeId = challengeId,
                        GameId = game.GameId,
                        OwnerId = user.OwnerId,
                        NumPlayers = (byte)input.numPlayers,
                        Notes = input.notes,
                        ClockStart = (ushort)input.clockStart,
                        ClockInc = (ushort)input.clockInc,
                        ClockMax = (ushort)input.clockMax,
                    };
                    if (input.variants.Length > 0)
                    {
                        rec.Variants = String.Join('|', input.variants);
                    }
                    //Add issuer
                    var issuer = new ChallengesPlayers {
                        EntryId = GuidGenerator.GenerateSequentialGuid(),
                        ChallengeId = challengeId,
                        OwnerId = user.OwnerId,
                        Confirmed = true
                    };
                    bool seated = false;
                    if (input.seat != null)
                    {
                        if (input.numPlayers != 2)
                        {
                            throw new ExecutionError("The 'seat' field is only meaningful in two-player games.");
                        }
                        if ( (input.seat != 1) && (input.seat != 2) )
                        {
                            throw new ExecutionError("The only valid values of 'seat' are '1' and '2'.");
                        }
                        seated = true;
                        issuer.Seat = (byte)input.seat;
                    }
                    rec.ChallengesPlayers.Add(issuer);
                    foreach (var player in input.challengees)
                    {
                        var node = new ChallengesPlayers {
                            EntryId = GuidGenerator.GenerateSequentialGuid(),
                            ChallengeId = challengeId,
                            OwnerId = GuidGenerator.HelperStringToBA(player),
                            Confirmed = false
                        };
                        if (seated)
                        {
                            node.Seat = (byte) ((input.seat % 2) + 1);
                        }
                        rec.ChallengesPlayers.Add(node);
                    }

                    db.Challenges.Add(rec);
                    db.SaveChanges();
                    return rec;
                }
            );
            Field<ChallengeType>(
                "respondChallenge",
                description: "Confirm or withdraw from a pending challenge",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<RespondChallengeInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<RespondChallengeDTO>("input");

                    var user = db.Owners.SingleOrDefault(x => x.CognitoId.Equals(context.cognitoId));
                    if (user == null)
                    {
                        throw new ExecutionError("You don't appear to have a user account! You must create a profile before you can play.");
                    }

                    byte[] binaryid;
                    try
                    {
                        binaryid = GuidGenerator.HelperStringToBA(input.id);
                    }
                    catch
                    {
                        throw new ExecutionError("The challenge ID you provided is malformed. Please verify and try again.");
                    }

                    var challenge = db.Challenges.SingleOrDefault(x => x.ChallengeId.Equals(binaryid));
                    if (challenge == null)
                    {
                        throw new ExecutionError("The challenge '"+input.id+"' does not appear to exist.");
                    }

                    var player = db.ChallengesPlayers.SingleOrDefault(x => x.ChallengeId.Equals(challenge.ChallengeId) && x.OwnerId.Equals(user.OwnerId));
                    //challenge.ChallengesPlayers.SingleOrDefault(x => x.OwnerId.Equals(user.OwnerId));

                    //if confirming
                    if (input.confirmed)
                    {
                        //They were directly invited and so are already in the database
                        if ( (player != null) && (!player.Confirmed) )
                        {
                            player.Confirmed = true;
                            db.ChallengesPlayers.Update(player);
                        }
                        //otherwise, add them
                        else if (player == null)
                        {
                            var node = new ChallengesPlayers
                            {
                                EntryId = GuidGenerator.GenerateSequentialGuid(),
                                ChallengeId = GuidGenerator.HelperStringToBA(input.id),
                                OwnerId = user.OwnerId,
                                Confirmed = true,
                                Seat = null
                            };
                            db.ChallengesPlayers.Add(node);
                        }

                        //Check for full challenge and create game if necessary
                        if (challenge.ChallengesPlayers.Where(x => x.Confirmed).Count() == challenge.NumPlayers)
                        {
                            //Prepare the variant and player lists for the game factory
                            string[] variants;
                            if (String.IsNullOrWhiteSpace(challenge.Variants))
                            {
                                variants = new string[0];
                            }
                            else
                            {
                                variants = challenge.Variants.Split('|');
                            }
                            string[] players;
                            if (challenge.NumPlayers == 2)
                            {
                                var plist = new List<string>();
                                var parray = challenge.ChallengesPlayers.ToArray();
                                //only one of the players will have a defined seat
                                if ( (parray[0].Seat == 1) || (parray[1].Seat == 2) )
                                {
                                    plist.Add(GuidGenerator.HelperBAToString(parray[0].Owner.PlayerId));
                                    plist.Add(GuidGenerator.HelperBAToString(parray[1].Owner.PlayerId));
                                }
                                else if ( (parray[0].Seat == 2) || (parray[1].Seat == 1) )
                                {
                                    plist.Add(GuidGenerator.HelperBAToString(parray[1].Owner.PlayerId));
                                    plist.Add(GuidGenerator.HelperBAToString(parray[0].Owner.PlayerId));
                                }
                                else
                                {
                                    foreach (var o in challenge.ChallengesPlayers.Select(x => (Owners)x.Owner))
                                    {
                                        plist.Add(GuidGenerator.HelperBAToString(o.PlayerId));
                                    }
                                    plist.Shuffle();
                                }
                                players = plist.ToArray();
                            }
                            else
                            {
                                var plist = new List<string>();
                                foreach (var o in challenge.ChallengesPlayers.Select(x => (Owners)x.Owner))
                                {
                                    plist.Add(GuidGenerator.HelperBAToString(o.PlayerId));
                                }
                                plist.Shuffle();
                                players = plist.ToArray();
                            }

                            //Now create the game object. If it fails, then everything aborts.
                            Game gameobj;
                            try
                            {
                                gameobj = GameFactory.CreateGame(challenge.Game.Shortcode, players, variants);
                            } 
                            catch (ArgumentException e)
                            {
                                throw new ExecutionError("An error occurred while trying to create the game. Please alert the administrators. The game code said the following: " + e.Message);
                            }
                            //Everything appears to be in order, so now we create the various DB objects and store them
                            var ngdata = new NewGameInput
                            {
                                Gameobj = gameobj,
                                Shortcode = challenge.Game.Shortcode,
                                ClockStart = challenge.ClockStart,
                                ClockMax = challenge.ClockMax,
                                ClockInc = challenge.ClockInc,
                                Variants = challenge.Variants
                            };
                            var newgame = DBFuncs.NewGame(db, ngdata);

                            //Delete the challenge
                            db.Challenges.Remove(challenge);
                        }
                    }
                    //if withdrawing and the player entry already exists
                    else if (player != null)
                    {
                        //Is it the challenge issuer who's withdrawing?
                        if (player.OwnerId.SequenceEqual(challenge.OwnerId))
                        {
                            db.Challenges.Remove(challenge);
                        }
                        //Or someone else?
                        else
                        {
                            db.ChallengesPlayers.Remove(player);
                        }
                    }
                    db.SaveChanges();
                    return challenge;
                }
            );
        }
    }
}