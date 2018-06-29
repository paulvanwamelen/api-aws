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
            Field<StringGraphType>("id", resolve: context => GuidGenerator.HelperBAToString(((Owners)context.Source).OwnerId), description: "User's ID number");
            Field<StringGraphType>("name", resolve: context => ((Owners)context.Source).OwnersNames.First().Name, description: "User's current display name.");
            Field(x => x.DateCreated, nullable: true).Name("created").Description("The date this account was created.");
            Field(x => x.Country, nullable: true).Name("country").Description("The ISO Alpha-2 code of the country the user says they're from.");
            Field(x => x.Tagline, nullable: true).Name("tagline").Description("The user's tagline.");
        }
    }

    public class APQuery : ObjectGraphType
    {
        public APQuery(MyContext db)
        {
            Field<UserType>(
                "user",
                arguments: new QueryArguments(new QueryArgument<StringGraphType> { Name = "id" }),
                resolve: context =>
                {
                    var id = context.GetArgument<string>("id");
                    return db.Owners.Include(x => x.OwnersNames).Single(x => x.OwnerId.Equals(GuidGenerator.HelperStringToBA(id)) && !x.Anonymous);
                }
            );
            Field<ListGraphType<UserType>>(
                "users",
                arguments: new QueryArguments(new QueryArgument<StringGraphType> { Name = "country" }),
                resolve: context =>
                {
                    var country = context.GetArgument<string>("country");
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
}
