using System;
using System.Linq;
using GraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    /*
     * This query should contain only data available to unauthenticated users.
     */
    public class APQuery : ObjectGraphType
    {
        public APQuery(MyContext db)
        {
            Field<UserType>(
                "user",
                description: "A single user record",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id" }),
                resolve: _ =>
                {
                    var id = _.GetArgument<string>("id");
                    return db.Owners.Single(x => x.OwnerId.Equals(GuidGenerator.HelperStringToBA(id)) && !x.Anonymous);
                }
            );
            Field<ListGraphType<UserType>>(
                "users",
                description: "List of registered users",
                arguments: new QueryArguments(new QueryArgument<StringGraphType> { Name = "country" }),
                resolve: _ =>
                {
                    var country = _.GetArgument<string>("country");
                    if (String.IsNullOrWhiteSpace(country))
                    {
                        return db.Owners.Where(x => !x.Anonymous).ToArray();
                    }
                    else
                    {
                        return db.Owners.Where(x => x.Country.Equals(country) && !x.Anonymous).ToArray();
                    }
                }
            );
            Field<GamesMetaType>(
                "gameMeta",
                description: "A single game's metadata",
                arguments: new QueryArguments(new[] {
                    new QueryArgument<StringGraphType> { Name = "id", Description = "The game's unique id" }, 
                    new QueryArgument<StringGraphType> { Name = "shortcode", Description = "The game's shortcode" } 
                }),
                resolve: _ =>
                {
                    var id = _.GetArgument<string>("id");
                    var shortcode = _.GetArgument<string>("shortcode");
                    if (!String.IsNullOrWhiteSpace(id))
                    {
                        return db.GamesMeta.Single(x => x.GameId.Equals(GuidGenerator.HelperStringToBA(id)));

                    } else if (!String.IsNullOrWhiteSpace(shortcode))
                    {
                        return db.GamesMeta.Single(x => x.Shortcode.Equals(shortcode));
                    } else 
                    {
                        throw new ExecutionError("You must provide either the game's unique ID or its shortcode.");
                    }
                }
            );
            Field<ListGraphType<GamesMetaType>>(
                "gamesMeta",
                description: "Metadata for multiple games",
                resolve: _ =>
                {
                    return db.GamesMeta.ToArray();
                }
            );

            Field<ChallengeType>(
                "challenge",
                description: "A specific challenge",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id" }),
                resolve: _ =>
                {
                    var id = _.GetArgument<string>("id");
                    return db.Challenges.Single(x => x.ChallengeId.Equals(GuidGenerator.HelperStringToBA(id)));
                }
            );
            Field<ListGraphType<ChallengeType>>(
                "challenges",
                description: "A list of all challenges",
                resolve: _ => db.Challenges.ToArray()
            );
        }
    }
}