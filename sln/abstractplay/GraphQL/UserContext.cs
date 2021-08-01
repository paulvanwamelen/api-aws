using System;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace abstractplay.GraphQL
{
    public class UserContext
    {
        public byte[] CognitoId { get; private set; }
        public string Username { get; private set; }
        public string EMail { get; private set; }

        public UserContext(APIGatewayProxyRequest request)
        {
            string sub;
            try
            {
                sub = request.RequestContext.Authorizer.Claims["sub"];
                // LambdaLogger.Log(sub);
                // This shuffles the bytes in a weird way first, so if you use GuidGenerator.HelperStringToBA on it you don't
                // get the same GUID/UUID back...
                CognitoId = new Guid(sub).ToByteArray();
                /*
                for (int i = 0; i < 16; i++)
                    LambdaLogger.Log(CognitoId[i].ToString());
                LambdaLogger.Log((new Guid(CognitoId)).ToString());
                */
                Username = request.RequestContext.Authorizer.Claims["cognito:username"];
                EMail = request.RequestContext.Authorizer.Claims["email"];
            }
            catch (Exception e)
            {
                LambdaLogger.Log("Failed to parse request.RequestContext.Authorizer.Claims");
                LambdaLogger.Log(e.Message);
                LambdaLogger.Log(Newtonsoft.Json.JsonConvert.SerializeObject(request.RequestContext, Newtonsoft.Json.Formatting.Indented));
                CognitoId = null;
            }
        }
    }
}
