using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class NameHistoryType : ObjectGraphType<OwnersNames> 
    {
        public NameHistoryType()
        {
            Field(x => x.Name).Name("name").Description("The display name");
            Field(x => x.EffectiveFrom).Name("effectiveFrom").Description("The date this name was effective from");
        }
    }
}