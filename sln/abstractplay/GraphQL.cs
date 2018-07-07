using System;
using System.Collections.Generic;
using System.Text;
using abstractplay.DB;
using gql = GraphQL;
using GraphQL.Types;
//using GraphQL.Builders;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

namespace abstractplay.GraphQL
{
    public class UserType : ObjectGraphType<Owners>
    {
        public UserType()
        {
            Field<StringGraphType>("id", resolve: _ => GuidGenerator.HelperBAToString(((Owners)_.Source).OwnerId), description: "User's ID number");
            Field<StringGraphType>("name", resolve: _ => ((Owners)_.Source).OwnersNames.First().Name, description: "User's current display name.");
            Field(x => x.DateCreated, nullable: true).Name("created").Description("The date this account was created.");
            Field(x => x.Country, nullable: true).Name("country").Description("The ISO Alpha-2 code of the country the user says they're from.");
            Field(x => x.Tagline, nullable: true).Name("tagline").Description("The user's tagline.");
            Field<ListGraphType<NameHistoryType>>("nameHistory", resolve: _ => ((Owners)_.Source).OwnersNames.ToArray(), description: "Past display names this player has used");
        }
    }

    public class NameHistoryType : ObjectGraphType<OwnersNames> 
    {
        public NameHistoryType()
        {
            Field(x => x.Name).Name("name").Description("The display name");
            Field(x => x.EffectiveFrom).Name("effectiveFrom").Description("The date this name was effective from");
        }
    }

    public class MeType : UserType
    {
        public MeType(): base() {}
    }

    public class UserContext
    {
        public byte[] cognitoId = null;

        public UserContext(APIGatewayProxyRequest request)
        {
            string sub;
            try
            {
                sub = request.RequestContext.Authorizer.Claims["sub"];
                cognitoId = new Guid(sub).ToByteArray();
            } catch (Exception e)
            {
                cognitoId = null;
            }
        }
    }

    public class APQuery : ObjectGraphType
    {
        public APQuery(MyContext db)
        {
            Field<UserType>(
                "user",
                arguments: new QueryArguments(new QueryArgument<StringGraphType> { Name = "id" }),
                resolve: _ =>
                {
                    var id = _.GetArgument<string>("id");
                    return db.Owners.Include(x => x.OwnersNames).Single(x => x.OwnerId.Equals(GuidGenerator.HelperStringToBA(id)) && !x.Anonymous);
                }
            );
            Field<ListGraphType<UserType>>(
                "users",
                arguments: new QueryArguments(new QueryArgument<StringGraphType> { Name = "country" }),
                resolve: _ =>
                {
                    var country = _.GetArgument<string>("country");
                    if (String.IsNullOrWhiteSpace(country))
                    {
                        return db.Owners.Include(x => x.OwnersNames).Where(x => !x.Anonymous).ToArray();
                    }
                    else
                    {
                        return db.Owners.Include(x => x.OwnersNames).Where(x => x.Country.Equals(country) && !x.Anonymous).ToArray();
                    }
                }
            );
        }
    }

    public class APQueryAuth : APQuery
    {
        public APQueryAuth(MyContext db) : base(db)
        {
            Field<MeType>(
                "me",
                resolve: _ =>
                {
                    var context = (UserContext)_.UserContext;
                    if (context.cognitoId == null) {
                        throw new UnauthorizedAccessException("You do not appear to be logged in");
                    }
                    return db.Owners.Include(x => x.OwnersNames).Single(x => x.CognitoId.Equals(context.cognitoId));
                }
            );
        }
    }

    public class ProfileInputType : InputObjectGraphType
    {
        public ProfileInputType()
        {
            Name = "ProfileInput";
            Field<NonNullGraphType<StringGraphType>>("ownerId");
            Field<NonNullGraphType<StringGraphType>>("cognitoId");
            Field<NonNullGraphType<StringGraphType>>("playerId");
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<NonNullGraphType<BooleanGraphType>>("anonymous");
            Field<StringGraphType>("country");
            Field<StringGraphType>("tagline");
        }
    }

    public class ProfileDTO
    {
        public string ownerId {get; set;}
        public string cognitoId {get; set;}
        public string playerId {get; set;}
        public string name {get; set;}
        public bool anonymous {get; set;}
        public string country {get; set;}
        public string tagline {get; set;}
    }

    public class APMutator : ObjectGraphType
    {
        public APMutator(MyContext db)
        {
            Field<UserType>(
                "createProfile",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<ProfileInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var profile = _.GetArgument<ProfileDTO>("input");
                    DateTime now = DateTime.UtcNow;
                    Owners owner = new Owners { 
                        OwnerId = GuidGenerator.HelperStringToBA(profile.ownerId), 
                        CognitoId = GuidGenerator.HelperStringToBA(profile.cognitoId), 
                        PlayerId = GuidGenerator.HelperStringToBA(profile.playerId), 
                        DateCreated = now, 
                        ConsentDate = now, 
                        Anonymous = profile.anonymous, 
                        Country = profile.country, 
                        Tagline = profile.tagline 
                    };
                    OwnersNames ne = new OwnersNames { 
                        EntryId = GuidGenerator.GenerateSequentialGuid(), 
                        OwnerId = GuidGenerator.HelperStringToBA(profile.ownerId), 
                        EffectiveFrom = now, 
                        Name = profile.name
                    };
                    owner.OwnersNames.Add(ne);
                    db.Add(owner);
                    db.SaveChanges();
                    return owner;
                }
            );
        }
    }

    public class APSchemaRO : Schema
    {
        public APSchemaRO(MyContext db)
        {
            Query = new APQuery(db);
        }
    }

    public class APSchemaROAuth : Schema
    {
        public APSchemaROAuth(MyContext db)
        {
            Query = new APQueryAuth(db);
        }
    }

    public class APSchemaRW : Schema
    {
        public APSchemaRW(MyContext db)
        {
            Query = new APQuery(db);
            Mutation = new APMutator(db);
        }
    }
}
