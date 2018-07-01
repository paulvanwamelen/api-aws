using System;
using System.Collections.Generic;
using System.Text;
using abstractplay.DB;
using gql = GraphQL;
using GraphQL.Types;
//using GraphQL.Builders;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

    public struct ProfileDTO
    {
        public string ownerId;
        public string cognitoId;
        public string playerId;
        public string name;
        public bool anonymous;
        public string country;
        public string tagline;
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
                    Guid cognitoId = new Guid(profile.cognitoId);
                    Owners owner = new Owners { OwnerId = GuidGenerator.HelperStringToBA(profile.ownerId), CognitoId = cognitoId.ToByteArray(), PlayerId = GuidGenerator.HelperStringToBA(profile.playerId), DateCreated = now, ConsentDate = now, Anonymous = profile.anonymous, Country = profile.country, Tagline = profile.tagline };
                    OwnersNames ne = new OwnersNames { EntryId = GuidGenerator.GenerateSequentialGuid(), OwnerId = GuidGenerator.HelperStringToBA(profile.ownerId), EffectiveFrom = now, Name = profile.name};
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

    public class APSchemaRW : Schema
    {
        public APSchemaRW(Func<Type, GraphType> resolveType)
            : base(resolveType)
        {
            Query = (APQuery)resolveType(typeof (APQuery));
            Mutation = (APMutator)resolveType(typeof (APMutator));
        }        
        public APSchemaRW(MyContext db)
        {
            Query = new APQuery(db);
            Mutation = new APMutator(db);
        }
    }
}
