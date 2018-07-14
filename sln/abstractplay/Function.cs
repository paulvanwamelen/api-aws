using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Headers;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.SNSEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Newtonsoft.Json;
using System.Globalization;
using System.Diagnostics;
using abstractplay.DB;
using abstractplay.GraphQL;
using GraphQL;
using GraphQL.Types;
using GraphQL.Validation;

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

    public struct GameMetadataResponse
    {
        public string state;
        public string version;
        public int[] playercounts;
        public string description;
        public string changelog;
        public VariantInputDTO[] variants;
    }

    public class DBFunctions
    {
        private static readonly string GAMES_URL = "https://games.abstractplay.com/";
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

        /// <summary>
        /// A Lambda function to respond to GraphQL queries from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>GraphQL endpoint</returns>
        public async Task<string> Mutator(SNSEvent snsevent, ILambdaContext context)
        {
            var schema = new APSchemaRW(dbc);
            foreach (var rec in snsevent.Records)
            {
                dynamic input = JsonConvert.DeserializeObject(rec.Sns.Message);
                string query = (string)input.query.ToObject(typeof(string));
                string varjson = JsonConvert.SerializeObject(input.variables);
                var vars = varjson.ToInputs();
                var result = await new DocumentExecuter().ExecuteAsync(_ =>
                {
                    _.Schema = schema;
                    _.Query = input.query;
                    _.Inputs = vars;
                    _.ExposeExceptions = true;
                }).ConfigureAwait(false);
                var json = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                LambdaLogger.Log("Mutation request received:\nQuery\n" + rec.Sns.Message + "\nVars\n" + JsonConvert.SerializeObject(vars, Formatting.Indented) + "\nResult\n" + json);
            }
            return null;
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

        public async Task<string> Pinger(SNSEvent snsevent, ILambdaContext context)
        {
            foreach (var rec in snsevent.Records)
            {
                PingRequest req = JsonConvert.DeserializeObject<PingRequest>(rec.Sns.Message);
                LambdaLogger.Log("Received the following ping request:\n"+rec.Sns.Message);
                var snsclient = new AmazonSimpleNotificationServiceClient(Amazon.RegionEndpoint.USEast2);

                //check state first
                bool needMeta = true;
                if (!String.IsNullOrWhiteSpace(req.currstate))
                {
                    LambdaLogger.Log("Fetching state at " + req.url);
                    using (HttpClient client = new HttpClient())
                    using (var postbody = new StringContent("{\"mode\": \"ping\"}"))
                    using (HttpResponseMessage response = await client.PostAsync(req.url, postbody))
                    using (HttpContent content = response.Content)
                    {
                        //Check header and send status update
                        GameStatusDTO payload;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            payload = new GameStatusDTO {
                                shortcode = req.shortcode,
                                isUp = true,
                                message = "Action: Ping"
                            };
                        } else 
                        {
                            payload = new GameStatusDTO {
                                shortcode = req.shortcode,
                                isUp = false,
                                message = "Action: Ping, Status Code: " + response.StatusCode.ToString()
                            };
                            //Don't let the logic continue after the status update
                            needMeta = false;
                        }

                        var mutreq = new MutateRequest {
                            query = "mutation ($data: GameStatusInput!){ updateGameStatus (input: $data) {id}}",
                            variables = new Dictionary<string, object>
                            {
                                {"data", payload}
                            }
                        };
                        string query = JsonConvert.SerializeObject(mutreq);
                        string snsarn = System.Environment.GetEnvironmentVariable("sns_mutator");
                        LambdaLogger.Log("The following query is being sent to SNS arn "+ snsarn +":\n" + query);
                        var snsreq = new PublishRequest(snsarn, query);
                        await snsclient.PublishAsync(snsreq).ConfigureAwait(false);

                        if (needMeta)
                        {
                            //Now process content and act accordingly
                            string result = await content.ReadAsStringAsync();
                            dynamic input = JsonConvert.DeserializeObject(result);
                            string state = (string)input.state.ToObject(typeof(string));

                            if (state == req.currstate)
                            {
                                needMeta = false;
                            }
                        }
                    }
                }

                if (needMeta)
                {
                    string graphquery;
                    object vars;
                    if (req.type == RemoteTypes.GAME)
                    {
                        LambdaLogger.Log("Fetching full metadata.");
                        using (HttpClient client = new HttpClient())
                        using (var postbody = new StringContent("{\"mode\": \"metadata\"}"))
                        using (HttpResponseMessage response = await client.PostAsync(req.url, postbody))
                        using (HttpContent content = response.Content)
                        {
                            // ... Read the string.
                            string result = await content.ReadAsStringAsync();
                            LambdaLogger.Log("Received the following metadata:\n"+result);
                            GameMetadataResponse input = JsonConvert.DeserializeObject<GameMetadataResponse>(result);

                            vars = new GameMetadataDTO() {
                                shortcode = req.shortcode,
                                state = input.state,
                                version = input.version,
                                playercounts = input.playercounts,
                                description = input.description,
                                changelog = input.changelog,
                                variants = input.variants
                            };
                            graphquery = "mutation ($data: GameMetadataInput!){ updateGameMetadata(input: $data) {id}}";
                        }
                    } else
                    {
                        //AIs, which aren't implemented yet
                        graphquery = "";
                        vars = new object();
                    }
                    var mutreq = new MutateRequest
                    {
                        query = graphquery,
                        variables = new Dictionary<string, object>
                        {
                            {"data", vars}
                        }
                    };
                    string query = JsonConvert.SerializeObject(mutreq);
                    string snsarn = System.Environment.GetEnvironmentVariable("sns_mutator");
                    LambdaLogger.Log("The following query is being sent to SNS arn "+ snsarn +":\n" + query);
                    var snsreq = new PublishRequest(snsarn, query);
                    await snsclient.PublishAsync(snsreq).ConfigureAwait(false);
                }
            }
            return null;
        }
    }
}
