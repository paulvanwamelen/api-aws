using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using GraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    /*
     * This is the mutator for authenticated users.
     */
    public class APMutatorAuth : ObjectGraphType
    {
        public APMutatorAuth(MyContext db)
        {
            Field<UserType>(
                "createProfile",
                description: "Create a new profile",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<NewProfileInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var profile = _.GetArgument<NewProfileDTO>("input");
                    StringInfo strinfo;

                    //Validate input
                    //check for duplicate cognitoId first
                    if (db.Owners.Any(x => x.CognitoId.Equals(context.cognitoId)))
                    {
                        throw new ExecutionError("You already have an account. Use the `updateProfile` command to make changes to an existing profile.");
                    }

                    //name
                    if (String.IsNullOrWhiteSpace(profile.name))
                    {
                        throw new ExecutionError("The 'name' field is required.");
                    }
                    strinfo = new StringInfo(profile.name);
                    if ( (strinfo.LengthInTextElements < 3) || (strinfo.LengthInTextElements > 30) )
                    {
                        throw new ExecutionError("The 'name' field must be between 3 and 30 utf-8 characters in length.");
                    }
                    if (db.OwnersNames.Any(x => x.Name.Equals(profile.name)))
                    {
                        throw new ExecutionError("The name you requested is either in use or has been recently used. Please choose a different one.");
                    }

                    //country
                    if ( (!String.IsNullOrWhiteSpace(profile.country)) && (profile.country.Length != 2) )
                    {
                        throw new ExecutionError("The 'country' field must consist of only two characters, representing a valid ISO 3166-1 alpha-2 country code.");
                    }
                    profile.country = profile.country.ToUpper();

                    //tagline
                    if (!String.IsNullOrWhiteSpace(profile.tagline))
                    {
                        strinfo = new StringInfo(profile.tagline);
                        if (strinfo.LengthInTextElements > 255)
                        {
                            throw new ExecutionError("The 'tagline' field may not exceed 255 utf-8 characters.");
                        }
                    }

                    //anonymous
                    //Apparently doesn't need to be checked. Should auto-default to false.

                    //consent
                    if (profile.consent != true)
                    {
                        throw new ExecutionError("You must consent to the terms of service and to the processing of your data to create an account and use Abstract Play.");
                    }

                    //Create record
                    DateTime now = DateTime.UtcNow;
                    byte[] ownerId = GuidGenerator.GenerateSequentialGuid();
                    byte[] playerId = Guid.NewGuid().ToByteArray();
                    Owners owner = new Owners { 
                        OwnerId = ownerId, 
                        CognitoId = context.cognitoId, 
                        PlayerId = playerId, 
                        DateCreated = now, 
                        ConsentDate = now, 
                        Anonymous = profile.anonymous, 
                        Country = profile.country, 
                        Tagline = profile.tagline 
                    };
                    OwnersNames ne = new OwnersNames { 
                        EntryId = GuidGenerator.GenerateSequentialGuid(), 
                        OwnerId = ownerId, 
                        EffectiveFrom = now, 
                        Name = profile.name
                    };
                    owner.OwnersNames.Add(ne);
                    db.Add(owner);
                    db.SaveChanges();
                    return owner;
                }
            );

            Field<UserType>(
                "updateProfile",
                description: "Update your existing profile",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<PatchProfileInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<PatchProfileDTO>("input");
                    LambdaLogger.Log(JsonConvert.SerializeObject(input));

                    //Load profile first
                    Owners rec = db.Owners.Include(x => x.OwnersNames).Single(x => x.CognitoId.Equals(context.cognitoId));
                    if (rec == null)
                    {
                        throw new ExecutionError("Could not find your profile. You need to do `createProfile` first.");
                    }

                    //name
                    if (!String.IsNullOrWhiteSpace(input.name))
                    {
                        var strinfo = new StringInfo(input.name);
                        if ( (strinfo.LengthInTextElements < 3) || (strinfo.LengthInTextElements > 30) )
                        {
                            throw new ExecutionError("The 'name' field must be between 3 and 30 utf-8 characters in length.");
                        }
                        if (db.OwnersNames.Any(x => x.Name.Equals(input.name)))
                        {
                            throw new ExecutionError("The name you requested is either in use or has been recently used. Please choose a different one.");
                        }
                        DateTime now = DateTime.UtcNow;
                        OwnersNames ne = new OwnersNames { 
                            EntryId = GuidGenerator.GenerateSequentialGuid(), 
                            OwnerId = rec.OwnerId, 
                            EffectiveFrom = now, 
                            Name = input.name
                        };
                        rec.OwnersNames.Add(ne);
                    }

                    //country
                    if (!String.IsNullOrWhiteSpace(input.country))
                    {
                        if (input.country.Length != 2)
                        {
                            throw new ExecutionError("The 'country' field must consist of only two characters, representing a valid ISO 3166-1 alpha-2 country code.");
                        }
                        rec.Country = input.country.ToUpper();
                    //If it's not null, it's an empty or whitespace string, so remove from the profile
                    } else if (input.country != null)
                    {
                        rec.Country = null;
                    }

                    //tagline
                    if (!String.IsNullOrWhiteSpace(input.tagline))
                    {
                        var strinfo = new StringInfo(input.tagline);
                        if (strinfo.LengthInTextElements > 255)
                        {
                            throw new ExecutionError("The 'tagline' field may not exceed 255 utf-8 characters.");
                        }
                        rec.Tagline = input.tagline;
                    } else if (input.tagline != null)
                    {
                        rec.Tagline = null;
                    }

                    //anonymous
                    if (input.anonymous != null)
                    {
                        rec.Anonymous = (bool)input.anonymous;
                    }

                    db.Owners.Update(rec);
                    db.SaveChanges();
                    return rec;
                }
            );

            Field<ChallengeType>(
                "issueChallenge",
                description: "Issue a new challenge",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<NewChallengeInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<NewChallengeDTO>("input");

                    var game = db.GamesMeta.Include(x => x.GamesMetaVariants).Single(x => x.Shortcode.Equals(input.game));
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
                        if (! db.Owners.Any(x => x.PlayerId.Equals(player)))
                        {
                            throw new ExecutionError("Could not find player ID "+player+".");
                        }
                    }

                    //Build record
                    var user = db.Owners.Single(x => x.CognitoId.Equals(context.cognitoId));
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

        }
    }
}