using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

using abstractplay.DB;
using abstractplay.Games;

namespace abstractplay.GraphQL
{
    /*
     * This is the mutator for authenticated users.
     */
    public partial class APMutatorAuth : ObjectGraphType
    {
        public APMutatorAuth(MyContext db)
        {
            Profiles(db);
            Challenges(db);
            GamesData(db);
        }
    }
}