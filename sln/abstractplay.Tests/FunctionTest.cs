using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using abstractplay.DB;

using abstractplay;

namespace abstractplay.Tests
{
    public class FunctionTest
    {
        public FunctionTest()
        {
        }

        [Fact]
        public void TestRootGetMethod()
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Functions functions = new Functions();


            request = new APIGatewayProxyRequest();
            context = new TestLambdaContext();
            response = functions.RootGet(request, context);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("Hello AWS Serverless", response.Body);
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

            //Need to delete entries now
            MyContext dbc = new MyContext();
            var ownerlist = dbc.Owners.Where(x => x.CognitoId.Equals(sub.ToByteArray())).ToList();
            Assert.Single(ownerlist);
            Owners ret = ownerlist[0];
            Assert.NotNull(ret);
            dbc.Remove(ret);
            dbc.SaveChanges();

            request.Body = Minimum_ErrorFree;
            response = functions.UsersPost(request, context);
            Assert.Equal(201, response.StatusCode);
            ownerlist = dbc.Owners.Where(x => x.CognitoId.Equals(sub.ToByteArray())).ToList();
            Assert.Single(ownerlist);
            ret = ownerlist[0];
            Assert.NotNull(ret);
            dbc.Remove(ret);
            dbc.SaveChanges();
        }

        [Fact]
        public void TestContextOwner()
        {
            MyContext dbc = new MyContext();
            Guid cognitoId = new Guid("f686ace4-1946-4296-a4ff-6191d7e99004");

            Owners owner;
            OwnersNames ne;
            byte[] ownerId = GuidGenerator.GenerateSequentialGuid();
            Guid playerId = Guid.NewGuid();
            DateTime now = DateTime.UtcNow;

            owner = new Owners { OwnerId = ownerId, CognitoId = cognitoId.ToByteArray(), PlayerId = playerId.ToByteArray(), DateCreated = now, ConsentDate = now, Anonymous = Convert.ToSByte(false), Country = "CA", Tagline = "Lasciate ogni speranza, voi ch'entrate!" };
            ne = new OwnersNames { EntryId = GuidGenerator.GenerateSequentialGuid(), OwnerId = ownerId, EffectiveFrom = now, Name = "Perlkönig" };
            owner.OwnersNames.Add(ne);
            dbc.Add(owner);
            dbc.SaveChanges();

            Owners ret;
            OwnersNames activeName;
            var ownerlist = dbc.Owners.Where(x => x.OwnerId.Equals(ownerId)).ToList();
            Assert.Single(ownerlist);
            ret = ownerlist[0];
            activeName = ret.OwnersNames.ToList()[0];

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
            Debug.WriteLine(generated);

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
            Debug.WriteLine(constructed);
            Assert.Equal(constructed, generated);

            //Clean up
            Debug.WriteLine("Trying to clean up");
            dbc.Remove(owner);
            dbc.SaveChanges();
        }
    }
}
