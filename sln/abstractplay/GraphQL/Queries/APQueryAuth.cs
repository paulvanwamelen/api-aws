using System.Linq;
using Microsoft.EntityFrameworkCore;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    /*
     * This query automatically contains all the unauthenticated types.
     * Add any authenticated-only types here (e.g., `me`).
     */
    public class APQueryAuth : APQuery
    {
        public APQueryAuth(MyContext db) : base(db)
        {
            Field<MeType>(
                "me",
                description: "The currently logged-in user",
                resolve: _ =>
                {
                    var context = (UserContext)_.UserContext;
                    return db.Owners.Include(x => x.OwnersNames).Single(x => x.CognitoId.Equals(context.cognitoId));
                }
            );
        }
    }
}