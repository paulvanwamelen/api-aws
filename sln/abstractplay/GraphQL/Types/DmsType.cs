using GraphQL.Types;

using abstractplay.DB;

namespace abstractplay.GraphQL
{
    public class DmsType : ObjectGraphType<Dms> 
    {
        public DmsType()
        {
            Field<StringGraphType>("id", description: "This entry's unique ID number", resolve: _ => GuidGenerator.HelperBAToString(((Dms)_.Source).EntryId));
            Field<UserType>(
                "sender",
                description: "The user who sent the message",
                resolve: _ => ((Dms)_.Source).Sender
            );
            Field<UserType>(
                "receiver",
                description: "The user who received the message",
                resolve: _ => ((Dms)_.Source).Receiver
            );
            Field<DateTimeGraphType>(
                "timestamp",
                "The date and time the message was sent",
                resolve: _ => ((Dms)_.Source).DateSent
            );
            Field(x => x.Message).Name("message").Description("The text of the message");
        }
    }
}
