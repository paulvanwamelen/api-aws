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
using Newtonsoft.Json;
using System.Globalization;
using System.Diagnostics;
using abstractplay.DB;

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
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of blogs</returns>
        public APIGatewayProxyResponse RootGet(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "Hello AWS Serverless",
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain; charset=utf-8" } }
            };

            return response;
        }

        /// <summary>
        /// A Lambda function to respond to GraphQL queries from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>GraphQL endpoint</returns>
        public APIGatewayProxyResponse GraphQL(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "Hello AWS Serverless",
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain; charset=utf-8" } }
            };

            return response;
        }

        public APIGatewayProxyResponse UserGet(APIGatewayProxyRequest request, ILambdaContext context)
        {
            APIGatewayProxyResponse response;
            string ownerIdHex = request.PathParameters["id"];
            LambdaLogger.Log("Hex ID: " + ownerIdHex);

            //Refetch the object to make sure we're returning canonical data
            Owners ret;
            OwnersNames activeName;
            try
            {
                byte[] ownerId = GuidGenerator.HelperStringToBA(ownerIdHex);
                ret = dbc.Owners
                    .Include(x => x.OwnersNames)
                    .Single(x => x.OwnerId.Equals(ownerId) && !x.Anonymous);
                activeName = ret.OwnersNames.First();
            }
            catch (Exception e)
            {
                ResponseError r = new ResponseError()
                {
                    request = "",
                    message = "Could not find the requested user: " + ownerIdHex
                };
                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Body = JsonConvert.SerializeObject(r),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json; charset=utf-8" } }
                };
                return response;
            }

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
                id = ownerIdHex,
                name = activeName.Name,
                country = ret.Country,
                member_since = ret.DateCreated.ToString("o"),
                tagline = ret.Tagline,
                name_history = nh
            };
            response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(ru),
                Headers = new Dictionary<string, string> {
                    { "Content-Type", "text/plain; charset=utf-8" },
                    { "Link", "<" + SCHEMA_USER + ">; rel=\"describedBy\"" }
                }
            };

            return response;
        }

        public APIGatewayProxyResponse UsersPost(APIGatewayProxyRequest request, ILambdaContext context)
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

            //Check for duplicate cognito ID
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
            if (dbc.Owners.Any(x => x.CognitoId.Equals(cognitoId.ToByteArray())))
            {
                ResponseError r = new ResponseError()
                {
                    request = JsonConvert.SerializeObject(body),
                    message = "You already have an account. Having multiple accounts if forbidden."
                };
                response = new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = JsonConvert.SerializeObject(r),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json; charset=utf-8" } }
                };
                return response;
            }

            //All is well! Create the object.
            //LambdaLogger.Log("Creating the database entry!");
            Owners owner;
            OwnersNames ne;
            byte[] ownerId = GuidGenerator.GenerateSequentialGuid();
            try
            {
                Guid playerId = Guid.NewGuid();
                DateTime now = DateTime.UtcNow;

                owner = new Owners { OwnerId = ownerId, CognitoId = cognitoId.ToByteArray(), PlayerId = playerId.ToByteArray(), DateCreated = now, ConsentDate = now, Anonymous = anon, Country = country, Tagline = tagline };
                ne = new OwnersNames { EntryId = GuidGenerator.GenerateSequentialGuid(), OwnerId = ownerId, EffectiveFrom = now, Name = name};
                owner.OwnersNames.Add(ne);
                dbc.Add(owner);
                dbc.SaveChanges();
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

            //Refetch the object to make sure we're returning canonical data
            Owners ret;
            try
            {
                ret = dbc.Owners.Include(x => x.OwnersNames).Single(x => x.OwnerId.Equals(ownerId));
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
            OwnersNames activeName = ret.OwnersNames.First();

            //Return the object.
            ResponseUser ru = new ResponseUser()
            {
                id = GuidGenerator.HelperBAToString(ret.OwnerId),
                name = activeName.Name,
                country = ret.Country,
                member_since = ret.DateCreated.ToString("o"),
                tagline = ret.Tagline,
                name_history = new List<NameHistory>() { new NameHistory() { name = activeName.Name, effective_date = activeName.EffectiveFrom.ToString("o") } }
            };
            response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.Created,
                Body = JsonConvert.SerializeObject(ru),
                Headers = new Dictionary<string, string> {
                    { "Content-Type", "text/plain; charset=utf-8" },
                    { "Location", "/users/"+ GuidGenerator.HelperBAToString(ret.OwnerId) },
                    { "Link", "<" + SCHEMA_USER + ">; rel=\"describedBy\"" }
                }
            };

            return response;
        }
    }
}
