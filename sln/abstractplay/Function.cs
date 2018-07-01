using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
//using System.Net.Http;
//using System.Net.Http.Headers;

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

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace abstractplay
{
    public struct ResponseError
    {
        public string request;
        public string message;
    }

    public struct NameHistory
    {
        public string name;
        public string effective_date;
    }

    public struct ResponseUser
    {
        public string id;
        public string name;
        public string country;
        public string member_since;
        public string tagline;
        public List<NameHistory> name_history;
    }

    public struct MutateRequest
    {
        public string query;
        public Dictionary<string, Object> variables;
    }

    public class Functions
    {
        private readonly string SCHEMA_USER = "https://www.abstractplay.com/schemas/resources_user/1-0-0.json";
        //private static readonly HttpClient client = new HttpClient();
        private static MyContext dbc = new MyContext();

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
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
                string query = rec.Sns.Message;
                var result = await new DocumentExecuter().ExecuteAsync(_ =>
                {
                    _.Schema = schema;
                    _.Query = query;
                }).ConfigureAwait(false);
                var json = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                LambdaLogger.Log("Mutation request received:\nQuery\n" + query + "\nResult\n" + json);
            }
            return null;
        }

        public async Task<APIGatewayProxyResponse> UsersPost(APIGatewayProxyRequest request, ILambdaContext context)
        {
            //LambdaLogger.Log("Request: " + JsonConvert.SerializeObject(request));
            //LambdaLogger.Log("Context: " + JsonConvert.SerializeObject(context));

            APIGatewayProxyResponse response;
            dynamic body = JsonConvert.DeserializeObject(request.Body);

            //displayName is required
            string name;
            StringInfo strinfo;
            try
            {
                name = (string)body.displayName.ToObject(typeof(string));
                strinfo = new StringInfo(name);
                //LambdaLogger.Log("Display Name: " + name);
            } catch (Exception e)
            {
                ResponseError r = new ResponseError()
                {
                    request = JsonConvert.SerializeObject(body),
                    message = "The 'displayName' field is required."
                };
                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = JsonConvert.SerializeObject(r),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json; charset=utf-8" } }
                };
                return response;
            }

            //displayName must be between 3 and 30 characters
            if ( (strinfo.LengthInTextElements < 3) || (strinfo.LengthInTextElements > 30) )
            {
                ResponseError r = new ResponseError()
                {
                    request = JsonConvert.SerializeObject(body),
                    message = "The 'displayName' field must be between 3 and 30 utf-8 characters in length."
                };
                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = JsonConvert.SerializeObject(r),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json; charset=utf-8" } }
                };
                return response;
            }

            //Country Code
            string country = null;
            try
            {
                country = (string)body.country.ToObject(typeof(string));
                country = country.ToUpper();
                //LambdaLogger.Log("Country: " + country);
            }
            catch (Exception e)
            {
                //Do nothing because "country" defaults to null.
                //Ideally we'd check that a property exists, but we can't do that with dynamic variables.
            }

            if (country != null)
            {
                country = (string)body.country.ToObject(typeof(string));
                country = country.ToUpper();
                if (country.Length != 2)
                {
                    ResponseError r = new ResponseError()
                    {
                        request = JsonConvert.SerializeObject(body),
                        message = "The 'country' field must consist of only two characters, representing a valid ISO 3166-1 alpha-2 country code."
                    };
                    response = new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Body = JsonConvert.SerializeObject(r),
                        Headers = new Dictionary<string, string> { { "Content-Type", "application/json; charset=utf-8" } }
                    };
                    return response;
                }
            }

            //Tagline
            string tagline = null;
            try
            {
                tagline = (string)body.tagline.ToObject(typeof(string));
                strinfo = new StringInfo(tagline);
                //LambdaLogger.Log("Tagline: " + tagline);
            } catch (Exception e)
            {
                //Do nothing because "tagline" defaults to null.
                //Ideally we'd check that a property exists, but we can't do that with dynamic variables.
            }
            if (tagline != null)
            {
                tagline = (string)body.tagline.ToObject(typeof(string));
                strinfo = new StringInfo(tagline);
                if (strinfo.LengthInTextElements > 255)
                {
                    ResponseError r = new ResponseError()
                    {
                        request = JsonConvert.SerializeObject(body),
                        message = "The 'tagline' field may not exceed 255 characters in length."
                    };
                    response = new APIGatewayProxyResponse
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Body = JsonConvert.SerializeObject(r),
                        Headers = new Dictionary<string, string> { { "Content-Type", "application/json; charset=utf-8" } }
                    };
                    return response;
                }
            }

            //Anonymous?
            bool anon = false;
            try
            {
                anon = (bool)body.anonymous.ToObject(typeof(bool));
            } catch (Exception e)
            {
                //Do nothing because "anon" defaults to null.
                //Ideally we'd check that a property exists, but we can't do that with dynamic variables.
            }
            //LambdaLogger.Log("Anonymous? " + anon.ToString());

            //Consent
            bool consent = false;
            try
            {
                consent = (bool)body.consent.ToObject(typeof(bool));
                //LambdaLogger.Log("Consent? " + consent.ToString());
            } catch (Exception e)
            {
                ResponseError r = new ResponseError()
                {
                    request = JsonConvert.SerializeObject(body),
                    message = "You must consent to the terms of service and to the processing of your data to create an account and use Abstract Play."
                };
                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = JsonConvert.SerializeObject(r),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json; charset=utf-8" } }
                };
                return response;
            }
            //LambdaLogger.Log("Consent? " + consent.ToString());

            if (! consent)
            {
                ResponseError r = new ResponseError()
                {
                    request = JsonConvert.SerializeObject(body),
                    message = "You must consent to the terms of service and to the processing of your data to create an account and use Abstract Play."
                };
                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = JsonConvert.SerializeObject(r),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json; charset=utf-8" } }
                };
                return response;
            }

            //Check for cognito ID
            //LambdaLogger.Log("Checking for duplicate Cognito ID");
            string sub = null;
            try
            {
                sub = request.RequestContext.Authorizer.Claims["sub"];
            } catch (Exception e)
            {
                ResponseError r = new ResponseError()
                {
                    request = JsonConvert.SerializeObject(body),
                    message = "You do not appear to be logged in!"
                };
                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Body = JsonConvert.SerializeObject(r),
                    Headers = new Dictionary<string, string> {
                        { "Content-Type", "application/json; charset=utf-8" },
                        { "WWW-Authenticate", "OAuth realm=\"https://www.abstractplay.com/play\"" }
                    }
                };
                return response;
            }
            if (String.IsNullOrEmpty(sub))
            {
                ResponseError r = new ResponseError()
                {
                    request = JsonConvert.SerializeObject(body),
                    message = "You do not appear to be logged in!"
                };
                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Body = JsonConvert.SerializeObject(r),
                    Headers = new Dictionary<string, string> {
                        { "Content-Type", "application/json; charset=utf-8" },
                        { "WWW-Authenticate", "OAuth realm=\"https://www.abstractplay.com/play\"" }
                    }
                };
                return response;
            }
            Guid cognitoId = new Guid(sub);
            //LambdaLogger.Log("Cognito ID: " + cognitoId.ToString());

            //Duplicate checking is no longer done here.
            //Create the GraphQL query and publish to SNS
            var rec = new ProfileDTO()
            {
                ownerId = GuidGenerator.HelperBAToString(GuidGenerator.GenerateSequentialGuid()),
                cognitoId = GuidGenerator.HelperBAToString(cognitoId.ToByteArray()),
                playerId = GuidGenerator.HelperBAToString(Guid.NewGuid().ToByteArray()),
                name = name,
                anonymous = anon,
                country = country,
                tagline = tagline
            };
            // var req = new MutateRequest
            // {
            //     query = "mutation ($profile:ProfileInput!){ createProfile(input: $profile) {}}",
            //     variables = new Dictionary<string, object>
            //     {
            //         {"input", rec}
            //     }
            // };
            // string query = JsonConvert.SerializeObject(req);
            //string query = "{mutation {createProfile(input: "+ JsonConvert.SerializeObject(rec) +")}}";
            string query = "mutation {createProfile(input: {ownerId: \"" + rec.ownerId + "\", cognitoId: \""+rec.cognitoId+"\", playerId: \""+rec.playerId+"\", name: \""+rec.name+"\", anonymous: "+rec.anonymous.ToString().ToLower()+", country: \""+rec.country+"\", tagline: \""+rec.tagline+"\"}) {id} }";
            string snsarn = System.Environment.GetEnvironmentVariable("sns_mutator");
            LambdaLogger.Log("The following query is being sent to SNS arn "+ snsarn +":\n" + query);

            try
            {
                var snsclient = new AmazonSimpleNotificationServiceClient(Amazon.RegionEndpoint.USEast2);
                var snsreq = new PublishRequest(snsarn, query);
                await snsclient.PublishAsync(snsreq).ConfigureAwait(false);
            } catch (Exception e)
            {
                ResponseError r = new ResponseError()
                {
                    request = JsonConvert.SerializeObject(body),
                    message = e.ToString()
                };
                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Body = JsonConvert.SerializeObject(r),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json; charset=utf-8" } }
                };
                return response;
            }

            //Return the object.
            response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Accepted,
                Body = "It may take a few seconds for your profile to be created.",
                Headers = new Dictionary<string, string> {
                    { "Content-Type", "text/plain; charset=utf-8" },
                    { "Location", "/graphql?query={me{id}}" },
                }
            };

            return response;
        }
    }
}
