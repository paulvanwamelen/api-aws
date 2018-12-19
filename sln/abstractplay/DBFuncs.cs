using System;
using abstractplay.Games;
using abstractplay.DB;
using System.Linq;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

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

    public struct UpdateGameInput
    {
        public Game Gameobj;
        public GamesData Gamerec;
        public byte[] Mover;
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

        public static GamesData NewGame(MyContext db, NewGameInput input)
        {
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

        public static void UpdateGame(MyContext db, UpdateGameInput input)
        {
            //Add new state
            var newstate = new GamesDataStates
            {
                StateId = GuidGenerator.GenerateSequentialGuid(),
                GameId = input.Gamerec.EntryId,
                State = input.Gameobj.Serialize(),
                RenderRep = input.Gameobj.Render(),
                Timestamp = DateTime.UtcNow
            };
            db.GamesDataStates.Add(newstate);

            //Add chat
            if (! String.IsNullOrEmpty(input.Gameobj.ChatMsgs))
            {
                var newchat = new GamesDataChats
                {
                    ChatId = GuidGenerator.GenerateSequentialGuid(),
                    GameId = input.Gamerec.EntryId,
                    OwnerId = null,
                    Message = input.Gameobj.ChatMsgs
                };
                db.GamesDataChats.Add(newchat);
            }

            //Update clocks
            var clock = db.GamesDataClocks.Single(x => x.GameId.Equals(input.Gamerec.EntryId) && x.OwnerId.Equals(input.Mover));
            var newbank = clock.Bank + input.Gamerec.ClockInc;
            if (newbank > input.Gamerec.ClockMax)
            {
                newbank = input.Gamerec.ClockMax;
            }
            clock.Bank = (short)newbank;

            //Update whoseturn
            db.GamesDataWhoseturn.RemoveRange(db.GamesDataWhoseturn.Where(x => x.GameId.Equals(input.Gamerec.EntryId)));
            foreach (var p in input.Gameobj.Whoseturn())
            {
                var node = new GamesDataWhoseturn
                {
                    GameId = input.Gamerec.EntryId,
                    OwnerId = GuidGenerator.HelperStringToBA(PlayerId2OwnerId(db, p))
                };
                db.GamesDataWhoseturn.Add(node);
            }

            //Check for end of game
            if (input.Gameobj.Gameover)
            {
                input.Gamerec.Closed = true;

                //Archive the game
                List<GamesDataStates> allstates = db.GamesDataStates.Where(x => x.GameId.Equals(input.Gamerec.EntryId)).ToList();
                allstates.Add(newstate);
                JObject rec = JObject.FromObject(new
                    {
                        header = new
                        {
                            reportId = GuidGenerator.HelperBAToString(input.Gamerec.EntryId),
                            game = new
                            {
                                id = GuidGenerator.HelperBAToString(input.Gamerec.GameMetaId),
                                name = input.Gamerec.GameMeta.Name,
                                variants = String.IsNullOrWhiteSpace(input.Gamerec.Variants) ? new string[0] : input.Gamerec.Variants.Split('\n'),
                            },
                            dates = new
                            {
                                start = allstates.First().Timestamp,
                                end = allstates.Last().Timestamp
                            },
                            //Add `event` one day
                            timeControl = String.Join('/', new string[] {input.Gamerec.ClockStart.ToString(), input.Gamerec.ClockInc.ToString(), input.Gamerec.ClockMax.ToString()}),
                            players = input.Gameobj.Players,
                            results = input.Gameobj.Results()
                            //Add `termination` one day
                        },
                        moves = input.Gameobj.MovesArchive(allstates.Select(x => x.State).ToArray())
                    }
                );

                GamesArchive archive = new GamesArchive
                {
                    ReportId = input.Gamerec.EntryId,
                    Json = rec.ToString()
                };
                db.GamesArchive.Add(archive);
            }
        }
    }
}