using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class VariantType : ObjectGraphType<GamesMetaVariants>
    {
        public VariantType()
        {
            Field<StringGraphType>("id", resolve: _ => GuidGenerator.HelperBAToString(((GamesMetaVariants)_.Source).VariantId), description: "Variant's unique ID number");
            Field(x => x.Name).Name("name").Description("The variant's display name");
            Field(x => x.Note).Name("note").Description("A short description of the variant");
            Field(x => x.Group).Name("group").Description("Variants with the same 'group' are mutually exclusive");
        }
    }
}