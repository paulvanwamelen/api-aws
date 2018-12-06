using System;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.SimpleNotificationService;

namespace abstractplay.GraphQL
{
    public class UserContext
    {
        public byte[] cognitoId = null;

        public UserContext(APIGatewayProxyRequest request)
        {
            string sub;
            try
            {
                sub = request.RequestContext.Authorizer.Claims["sub"];
                cognitoId = new Guid(sub).ToByteArray();
            } 
            catch
            {
                cognitoId = null;
            }
        }
    }
}
