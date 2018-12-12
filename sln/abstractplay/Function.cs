using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using GraphQL;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using abstractplay.DB;
using abstractplay.GraphQL;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace abstractplay
{
    public struct ResponseError
    {
        public string request;
        public string message;
    }
    
    public struct MutateRequest
    {
        public string query;
        public Dictionary<string, Object> variables;
    }

    public enum RemoteTypes {AI, GAME};

    public struct PingRequest
    {
        public RemoteTypes type;
        public string shortcode;
        public string url;
        public string currstate;
    }

    public struct NewGameRequest
    {
        public string shortcode;
        public string url;
        public string[] players;
        public string[] variants;
        public int clockStart;
        public int clockInc;
        public int clockMax;
    }

    public struct NewGameResponse
    {
        public string state;
        public string[] whoseturn;
        public string[] chat;
        public string renderrep;
    }

    public class DBFunctions
    {
        //private static readonly HttpClient client = new HttpClient();
        private static MyContext dbc = new MyContext();

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public DBFunctions()
        {
            //dbc = new MyContext();
            //dbc.Database.EnsureCreated();
        }

        /// <summary>
        /// A Lambda function to respond to GraphQL queries from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>GraphQL endpoint</returns>
        public async Task<APIGatewayProxyResponse> GraphQL(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string query = request.QueryStringParameters["query"];
            var schema = new APSchemaRO(dbc);
            var result = await new DocumentExecuter().ExecuteAsync(_ =>
            {
                _.Schema = schema;
                _.Query = query;
                _.ExposeExceptions = true;
            }).ConfigureAwait(false);

            var json = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = json,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json; charset=utf-8" } }
            };

            return response;
        }

        public async Task<APIGatewayProxyResponse> GraphQLAuth(APIGatewayProxyRequest request, ILambdaContext context)
        {
            UserContext ucontext = new UserContext(request);
            string query = "";
            Inputs vars = new Inputs();
            Schema schema = new Schema();
            if (request.HttpMethod == "GET")
            {
                schema = new APSchemaROAuth(dbc);
                query = request.QueryStringParameters["query"];
            } else if (request.HttpMethod == "POST")
            {
                if (request.Headers["Content-Type"].ToLower() != "application/json")
                {
                    var r = new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
                        Body = "This endpoint only accepts 'application/json' content",
                        Headers = new Dictionary<string, string> { { "Content-Type", "text/plain; charset=utf-8" } }
                    };
                    return r;
                }
                schema = new APSchemaRWAuth(dbc);
                dynamic input = JsonConvert.DeserializeObject(request.Body);
                query = (string)input.query.ToObject(typeof(string));
                string varjson = JsonConvert.SerializeObject(input.variables);
                vars = varjson.ToInputs();
            }

            var result = await new DocumentExecuter().ExecuteAsync(_ =>
            {
                _.Schema = schema;
                _.Query = query;
                _.Inputs = vars;
                _.UserContext = ucontext;
                _.ExposeExceptions = true;
            }).ConfigureAwait(false);
            
            var json = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = json,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json; charset=utf-8" } }
            };

            return response;
        }
    }

    public class Functions
    {
        public Functions()
        {

        }

        public APIGatewayProxyResponse GetSequentialGuid(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string guid = GuidGenerator.HelperBAToString(GuidGenerator.GenerateSequentialGuid());

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = guid,
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain; charset=utf-8" } }
            };

            return response;
        }        
    }
}
