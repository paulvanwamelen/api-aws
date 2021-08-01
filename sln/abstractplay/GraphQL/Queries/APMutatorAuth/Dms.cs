using System;
using GraphQL;
using GraphQL.Types;
using System.Linq;

using abstractplay.DB;
using abstractplay.Games;
using Amazon.Lambda.Core;

namespace abstractplay.GraphQL
{
    /*
     * This is the mutator for authenticated users.
     */
    public partial class APMutatorAuth : ObjectGraphType
    {
        private void Dms(MyContext db)
        {
            Field<DmsType>(
                "sendDm",
                description: "Send a new direct message",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<SendDmInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<SendDmDTO>("input");

                    var user = db.Owners.SingleOrDefault(x => x.CognitoId.Equals(context.CognitoId));
                    if (user == null)
                    {
                        throw new ExecutionError("You don't appear to have a user account! Only registered users can send messages.");
                    }

                    byte[] binaryid;
                    try
                    {
                        binaryid = GuidGenerator.HelperStringToBA(input.recipient);
                    }
                    catch
                    {
                        throw new ExecutionError("The recipient ID you provided is malformed. Please verify and try again.");
                    }

                    //Does this game id exist?
                    var receiver = db.Owners.SingleOrDefault(x => x.OwnerId.Equals(binaryid));
                    if (receiver == null)
                    {
                        throw new ExecutionError("Could not find the requested recipient.");
                    }

                    if (receiver.OwnerId == user.OwnerId)
                    {
                        throw new ExecutionError("You can't send messages to yourself.");
                    }

                    var dm = new Dms
                    {
                        EntryId = GuidGenerator.GenerateSequentialGuid(),
                        SenderId = user.OwnerId,
                        Sender = user,
                        ReceiverId = binaryid,
                        Receiver = receiver,
                        DateSent = DateTime.UtcNow,
                        Message = input.message,
                        Read = false
                    };
                    db.Dms.Add(dm);
                    db.SaveChanges();
                    return dm;
                }
            );
            Field<ListGraphType<DmsType>>(
                "deleteDms",
                description: "Delete direct messages",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<DeleteDmsInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<DeleteDmsDTO>("input");

                    var user = db.Owners.SingleOrDefault(x => x.CognitoId.Equals(context.CognitoId));
                    if (user == null)
                    {
                        throw new ExecutionError("You don't appear to have a user account! Only registered users can use the messaging system.");
                    }

                    var binids = input.ids.Select(x => GuidGenerator.HelperStringToBA(x));
                    db.Dms.RemoveRange(db.Dms.Where(x => x.ReceiverId.Equals(user.OwnerId) && binids.Contains(x.EntryId)));
                    db.SaveChanges();
                    return db.Dms.Where(x => x.ReceiverId.Equals(user.OwnerId)).ToArray();
                }
            );
            Field<ListGraphType<DmsType>>(
                "readDms",
                description: "Mark direct messages as read",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<ReadDmsInputType>> {Name = "input"}
                ),
                resolve: _ => {
                    var context = (UserContext)_.UserContext;
                    var input = _.GetArgument<ReadDmsDTO>("input");

                    var user = db.Owners.SingleOrDefault(x => x.CognitoId.Equals(context.CognitoId));
                    if (user == null)
                    {
                        throw new ExecutionError("You don't appear to have a user account! Only registered users can use the messaging system.");
                    }

                    var binids = input.ids.Select(x => GuidGenerator.HelperStringToBA(x));
                    foreach (var rec in db.Dms.Where(x => x.ReceiverId.Equals(user.OwnerId) && binids.Contains(x.EntryId)))
                    {
                        rec.Read = true;
                    }
                    db.SaveChanges();
                    return db.Dms.Where(x => x.ReceiverId.Equals(user.OwnerId) && x.Read.Equals(false)).ToArray();
                }
            );
        }
    }
}
