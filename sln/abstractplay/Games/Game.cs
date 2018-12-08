using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace abstractplay.Games
{
    public abstract class Game
    {
        public string[] players;
        public int currplayer;
        public string lastmove;
        public bool gameover = false;
        public string winner;
        public List<string> chatmsgs;

        public abstract string Meta_version { get; }
        public abstract string Meta_description { get; }
        public abstract int[] Meta_playercounts { get; }
        public abstract string Meta_changelog { get; }
        public abstract string Meta_state { get; }
        public abstract string Meta_name { get; }

        public abstract Game Init(string[] players);
        public abstract Game Init(string json);
        public abstract Game Move(string player, string move);

        public virtual string Whoseturn()
        {
            return this.players[this.currplayer];
        }

        public virtual IEnumerable<string> LegalMoves()
        {
            throw new NotImplementedException("This game cannot generate a list of legal moves.");
        }
    }
}
