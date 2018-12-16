using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace abstractplay.Games
{
    public abstract class Game
    {
        protected string[] players;
        public string[] Players { get => players; }
        protected int currplayer;
        public int Currplayer { get => currplayer; }
        protected string lastmove;
        protected bool gameover = false;
        public bool Gameover { get => gameover; }
        protected string winner;
        public string Winner { get => winner; }
        protected List<string> chatmsgs;
        public string ChatMsgs { get => String.Join("\n\n", chatmsgs); }

        public abstract string Meta_version { get; }
        public abstract string Meta_description { get; }
        public abstract int[] Meta_playercounts { get; }
        public abstract string Meta_changelog { get; }
        public abstract string Meta_state { get; }
        public abstract string Meta_name { get; }

        public abstract Game Move(string player, string move);

        public abstract string Serialize();
        public abstract string Render();

        public virtual string[] Whoseturn()
        {
            string[] turns = new string[0];
            if (! this.Gameover)
            {
                turns = new string[] {this.players[this.currplayer]};
            }
            return turns;
        }

        public virtual IEnumerable<string> LegalMoves()
        {
            throw new NotImplementedException("This game cannot generate a list of legal moves.");
        }

        public abstract Game Resign(string player);
    }
}
