using System;
using Amazon.Lambda.APIGatewayEvents;

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
            } catch (Exception e)
            {
                cognitoId = null;
            }
        }
    }
}
