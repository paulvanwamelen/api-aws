using System;

namespace abstractplay.Games
{
    public class GameFactory
    {
        public static Game CreateGame(string gameid)
        {
            Game obj;
            switch (gameid)
            {
                case "ithaka":
                    obj = new Ithaka();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("The game id you passed is not recognized.");
            }
            return obj;
        }
    }
}