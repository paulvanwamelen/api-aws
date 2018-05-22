using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

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
            string Complete_ErrorFree = "{\"displayName\": \"Perlkönig\", \"country\": \"ca\", \"tagline\": \"Lasciate ogni speranza, voi ch'entrate!\", \"anonymous\": false, \"consent\": true}";
            string Minimum_ErrorFree = "{\"displayName\": \"Perlkönig\", \"consent\": true}";
            string MissingName = "{\"country\": \"ca\", \"tagline\": \"Lasciate ogni speranza, voi ch'entrate!\", \"anonymous\": false, \"consent\": true}";
            string LongName = "{\"displayName\": \"0123456789012345678901234567890\", \"country\": \"ca\", \"tagline\": \"Lasciate ogni speranza, voi ch'entrate!\", \"anonymous\": false, \"consent\": true}";
            string ShortName = "{\"displayName\": \"01\", \"country\": \"ca\", \"tagline\": \"Lasciate ogni speranza, voi ch'entrate!\", \"anonymous\": false, \"consent\": true}";
            string MissingConsent = "{\"displayName\": \"Perlkönig\"}";
            string FalseConsent = "{\"displayName\": \"Perlkönig\", \"consent\": false}";

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
            Owner ret = dbc.Owners.Where(x => x.CognitoId.Equals(sub.ToByteArray())).ToList()[0];
            dbc.Remove(ret);
            dbc.SaveChanges();

            request.Body = Minimum_ErrorFree;
            response = functions.UsersPost(request, context);
            Assert.Equal(201, response.StatusCode);
            ret = dbc.Owners.Where(x => x.CognitoId.Equals(sub.ToByteArray())).ToList()[0];
            dbc.Remove(ret);
            dbc.SaveChanges();
        }
    }
}
