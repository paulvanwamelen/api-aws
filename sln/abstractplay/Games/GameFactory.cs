using System;

namespace abstractplay.Games
{
    public class GameFactory
    {
        public static Game CreateGame(string gameid, string[] players, string[] variants)
        {
            Game obj;
            switch (gameid)
            {
                case "ithaka":
                    obj = new Ithaka(players);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("The game id you passed is not recognized.");
            }
            return obj;
        }
    }
}