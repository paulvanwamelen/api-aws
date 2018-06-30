using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Xunit;
using Xunit.Abstractions;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using abstractplay.DB;
using abstractplay.GraphQL;
using GraphQL;
using GraphQL.Types;
using System.Threading.Tasks;

using abstractplay;

namespace abstractplay.Tests
{
    public class FunctionTest
    {
        private readonly ITestOutputHelper output;

        public FunctionTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestGuid()
        {
            HashSet<byte[]> guids = new HashSet<byte[]>();
            Dictionary<string, int> guidlist = new Dictionary<string, int>();
            int maxnum = 100;
            for (int i=0; i<maxnum; i++)
            {
                byte[] g = GuidGenerator.GenerateSequentialGuid();
                guids.Add(g);
                guidlist[GuidGenerator.HelperBAToString(g)] = i;
            }
            Assert.Equal(maxnum, guids.Count);
            List<string> keys = guidlist.Keys.ToList();
            keys.Sort();
            for (int i=1; i<keys.Count; i++)
            {
                Assert.Equal(1, guidlist[keys[i]] - guidlist[keys[i - 1]]);
            }
        }

        [Fact]
        public void TestUsersPost()
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Functions functions = new Functions();

            Guid sub = new Guid("f686ace4-1946-4296-a4ff-6191d7e99004");

            context = new TestLambdaContext();
            request = new APIGatewayProxyRequest();
            request.RequestContext = new APIGatewayProxyRequest.ProxyRequestContext();
            request.RequestContext.Authorizer = new APIGatewayCustomAuthorizerContext();
            request.RequestContext.Authorizer.Claims = new Dictionary<string, string>() { { "sub", "f686ace4-1946-4296-a4ff-6191d7e99004" } };

            //Complete and error free
            string Complete_ErrorFree = "{\"displayName\": \"Perlk�nig\", \"country\": \"ca\", \"tagline\": \"Lasciate ogni speranza, voi ch'entrate!\", \"anonymous\": false, \"consent\": true}";
            string Minimum_ErrorFree = "{\"displayName\": \"Perlk�nig\", \"consent\": true}";
            string MissingName = "{\"country\": \"ca\", \"tagline\": \"Lasciate ogni speranza, voi ch'entrate!\", \"anonymous\": false, \"consent\": true}";
            string LongName = "{\"displayName\": \"0123456789012345678901234567890\", \"country\": \"ca\", \"tagline\": \"Lasciate ogni speranza, voi ch'entrate!\", \"anonymous\": false, \"consent\": true}";
            string ShortName = "{\"displayName\": \"01\", \"country\": \"ca\", \"tagline\": \"Lasciate ogni speranza, voi ch'entrate!\", \"anonymous\": false, \"consent\": true}";
            string MissingConsent = "{\"displayName\": \"Perlk�nig\"}";
            string FalseConsent = "{\"displayName\": \"Perlk�nig\", \"consent\": false}";

            request.Body = MissingName;
            response = functions.UsersPost(request, context);
            Assert.Equal(400, response.StatusCode);
            request.Body = LongName;
            response = functions.UsersPost(request, context);
            Assert.Equal(400, response.StatusCode);
            request.Body = ShortName;
            response = functions.UsersPost(request, context);
            Assert.Equal(400, response.StatusCode);
            request.Body = MissingConsent;
            response = functions.UsersPost(request, context);
            Assert.Equal(400, response.StatusCode);
            request.Body = FalseConsent;
            response = functions.UsersPost(request, context);
            Assert.Equal(400, response.StatusCode);
            request.Body = Complete_ErrorFree;
            response = functions.UsersPost(request, context);
            Assert.Equal(201, response.StatusCode);
            response = functions.UsersPost(request, context);
            Assert.Equal(400, response.StatusCode);

            //Minimal example then clean up
            using (MyContext dbc = new MyContext()) {
                Owners ret = dbc.Owners.Single(x => x.CognitoId.Equals(sub.ToByteArray()));
                dbc.Remove(ret);
                dbc.SaveChanges();

                request.Body = Minimum_ErrorFree;
                response = functions.UsersPost(request, context);
                Assert.Equal(201, response.StatusCode);
                ret = dbc.Owners.Single(x => x.CognitoId.Equals(sub.ToByteArray()));
                dbc.Remove(ret);
                dbc.SaveChanges();
            }
        }

        [Fact]
        public void TestContextOwner()
        {
            byte[] ownerId = GuidGenerator.GenerateSequentialGuid();
            Guid playerId = Guid.NewGuid();
            DateTime now = DateTime.UtcNow;
            now = new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
            Guid cognitoId = new Guid("f686ace4-1946-4296-a4ff-6191d7e99004");

            using (MyContext dbc = new MyContext()) {

                Owners owner;
                OwnersNames ne;

                owner = new Owners { OwnerId = ownerId, CognitoId = cognitoId.ToByteArray(), PlayerId = playerId.ToByteArray(), DateCreated = now, ConsentDate = now, Anonymous = false, Country = "CA", Tagline = "Lasciate ogni speranza, voi ch'entrate!" };
                ne = new OwnersNames { EntryId = GuidGenerator.GenerateSequentialGuid(), OwnerId = ownerId, EffectiveFrom = now, Name = "Perlkönig" };
                owner.OwnersNames.Add(ne);
                dbc.Add(owner);
                dbc.SaveChanges();
            }

            using (MyContext dbc = new MyContext()) {
                Owners ret;
                OwnersNames activeName;
                ret = dbc.Owners
                    .Include(x => x.OwnersNames)
                    .Single(x => x.OwnerId.Equals(ownerId));
                activeName = ret.OwnersNames.First();
                //output.WriteLine(JsonConvert.SerializeObject(ret, Formatting.Indented, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore}));

                List<NameHistory> nh = new List<NameHistory>();
                foreach (var e in ret.OwnersNames.ToArray())
                {
                    NameHistory node = new NameHistory
                    {
                        name = e.Name,
                        effective_date = e.EffectiveFrom.ToString("o")
                    };
                    nh.Add(node);
                }
                //Return the object.
                ResponseUser ru = new ResponseUser()
                {
                    id = GuidGenerator.HelperBAToString(ownerId),
                    name = activeName.Name,
                    country = ret.Country,
                    member_since = ret.DateCreated.ToString("o"),
                    tagline = ret.Tagline,
                    name_history = nh
                };
                string generated = JsonConvert.SerializeObject(ru);
                output.WriteLine(generated);

                ResponseUser testru = new ResponseUser()
                {
                    id = GuidGenerator.HelperBAToString(ownerId),
                    name = "Perlkönig",
                    country = "CA",
                    member_since = now.ToString("o"),
                    tagline = "Lasciate ogni speranza, voi ch'entrate!",
                    name_history = new List<NameHistory>()
                    {
                        new NameHistory()
                        {
                            name = "Perlkönig",
                            effective_date = now.ToString("o")
                        }
                    }
                };
                string constructed = JsonConvert.SerializeObject(testru);
                output.WriteLine(constructed);
                Assert.Equal(constructed, generated);

                //Clean up
                dbc.Remove(ret);
                dbc.SaveChanges();
            }
        }

        [Fact]
        public async void TestGraphQL()
        {
            byte[] ownerId = GuidGenerator.GenerateSequentialGuid();
            Guid playerId = Guid.NewGuid();
            DateTime now = DateTime.UtcNow;
            now = new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
            Guid cognitoId = new Guid("f686ace4-1946-4296-a4ff-6191d7e99004");

            using (MyContext dbc = new MyContext())
            {

                Owners owner;
                OwnersNames ne;

                owner = new Owners { OwnerId = ownerId, CognitoId = cognitoId.ToByteArray(), PlayerId = playerId.ToByteArray(), DateCreated = now, ConsentDate = now, Anonymous = false, Country = "CA", Tagline = "Lasciate ogni speranza, voi ch'entrate!" };
                ne = new OwnersNames { EntryId = GuidGenerator.GenerateSequentialGuid(), OwnerId = ownerId, EffectiveFrom = now, Name = "Perlkönig" };
                owner.OwnersNames.Add(ne);
                dbc.Add(owner);
                dbc.SaveChanges();
            }

            var ctx = new MyContext();
            //var query = @"{
            //    user(id: """ + GuidGenerator.HelperBAToString(ownerId) + @""") {
            //        id,
            //        name,
            //        country
            //    }
            //}";
            var query = @"{
                users(country:""CA"") {
                    id, name, country
                }
            }";
            //var query = @"{
            //    __schema {
            //        queryType {
            //            name
            //        }
            //    }
            //}";
            var schema = new Schema { Query = new APQuery(ctx) };
            var result = await new DocumentExecuter().ExecuteAsync(_ =>
            {
                _.Schema = schema;
                _.Query = query;
            }).ConfigureAwait(false);
            var json = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            Console.WriteLine(json);

            //Clean up
            var ret = ctx.Owners.Single(x => x.OwnerId.Equals(ownerId));
            ctx.Remove(ret);
            ctx.SaveChanges();
        }
    }
}
