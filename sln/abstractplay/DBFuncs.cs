using System;
using abstractplay.Games;
using abstractplay.DB;
using System.Linq;
using Amazon.Lambda.Core;

namespace abstractplay
{
    public struct NewGameInput
    {
        public Game Gameobj;
        public string Shortcode;
        public ushort ClockStart;
        public ushort ClockMax;
        public ushort ClockInc;
        public string Variants;
    }

    public static class DBFuncs
    {
        public static byte[] PlayerId2OwnerId(MyContext db, byte[] playerid)
        {
            return db.Owners.Single(x => x.PlayerId.Equals(playerid)).OwnerId;
        }

        public static string PlayerId2OwnerId(MyContext db, string playerid)
        {
            return GuidGenerator.HelperBAToString(PlayerId2OwnerId(db, GuidGenerator.HelperStringToBA(playerid)));
        }

        public static GamesData WriteGame(MyContext db, NewGameInput input)
        {
            LambdaLogger.Log("In 'WriteGame' function.");
            var newgame = new GamesData
            {
                EntryId = GuidGenerator.GenerateSequentialGuid(),
                GameMetaId = db.GamesMeta.Single(x => x.Shortcode.Equals(input.Shortcode)).GameId,
                Closed = false,
                Alert = false,
                ClockStart = input.ClockStart,
                ClockInc = input.ClockInc,
                ClockMax = input.ClockMax,
                ClockFrozen = false,
                Variants = input.Variants
            };
            db.GamesData.Add(newgame);

            LambdaLogger.Log("Here are the players recorded in the game object: " + String.Join(", ", input.Gameobj.Players));
            foreach (var p in input.Gameobj.Players)
            {
                var oid = PlayerId2OwnerId(db, GuidGenerator.HelperStringToBA(p));
                var newclock = new GamesDataClocks
                {
                    GameId = newgame.EntryId,
                    OwnerId = oid,
                    Bank = (short)newgame.ClockStart
                };
                db.GamesDataClocks.Add(newclock);
                var newplayer = new GamesDataPlayers
                {
                    GameId = newgame.EntryId,
                    OwnerId = oid,
                };
                db.GamesDataPlayers.Add(newplayer);
            }
            
            foreach (var p in input.Gameobj.Whoseturn())
            {
                var newwho = new GamesDataWhoseturn
                {
                    GameId = newgame.EntryId,
                    OwnerId = PlayerId2OwnerId(db, GuidGenerator.HelperStringToBA(p)),
                };
                db.GamesDataWhoseturn.Add(newwho);
            }

            var newstate = new GamesDataStates
            {
                StateId = GuidGenerator.GenerateSequentialGuid(),
                GameId = newgame.EntryId,
                State = input.Gameobj.Serialize(),
                RenderRep = input.Gameobj.Render()
            };
            db.GamesDataStates.Add(newstate);
            return newgame;
        }
    }
}