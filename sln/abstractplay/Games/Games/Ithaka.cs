﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Diagnostics;
using abstractplay.Grids.Square;
using System.Linq;

namespace abstractplay.Games
{
    public class Ithaka : Game
    {
        private static string meta_version = "1.0.0";
        private static string meta_name = "Ithaka";
        private static string meta_description = @"A player can move a piece any number of empty spaces in a line, but they can't move the piece that was just moved OR a piece that is not orthoganally adjacent to another piece of its own colour. The winner is the player at the end of whose turn a row of three pieces of the same colour exists (either orthogonal or diagonal). A player loses if they are unable to move on their turn.";
        private static int[] meta_playercounts = new int[1] { 2 };
        private static string meta_changelog = "";
        private static string meta_state = Ithaka.meta_version;

        private SquareFixed grid;
        public char[] board;
        public int lastmoved;
        private static int[][] lines =
        {
            new int[] { 0,1,2 },
            new int[] { 1,2,3 },
            new int[] { 4,5,6 },
            new int[] { 5,6,7 },
            new int[] { 8,9,10 },
            new int[] { 9,10,11 },
            new int[] { 12,13,14 },
            new int[] { 13,14,15 },
            new int[] { 0,4,8 },
            new int[] { 4,8,12 },
            new int[] { 1,5,9 },
            new int[] { 5,9,13 },
            new int[] { 2,6,10 },
            new int[] { 6,10,14 },
            new int[] { 3,7,11 },
            new int[] { 7,11,15 },
            new int[] { 0,5,10 },
            new int[] { 5,10,15 },
            new int[] { 3,6,9 },
            new int[] { 6,9,12 },
        };
        private Dictionary<string, int> states;
        public struct Serialized
        {
            public string meta_name;
            public string meta_version;
            public string[] players;
            public int currplayer;
            public string board;
            public int lastmoved;
            public string lastmove;
            public Dictionary<string, int> states;
            public bool gameover;
            public string winner;
        };
        public struct Board
        {
            public string style;
            public int width;
            public int height;
        }
        public struct PlayerPiece
        {
            public string name;
            public int player;
        }
        public struct Rendered
        {
            public Board board;
            public Dictionary<string, PlayerPiece> legend;
            public string pieces;
        };
        private Regex re_validmove = new Regex(@"^([a-d][1-4])\-([a-d][1-4])$", RegexOptions.IgnoreCase);

        public override string Meta_state { get => meta_state; }
        public override string Meta_changelog { get => meta_changelog; }
        public override int[] Meta_playercounts { get => meta_playercounts; }
        public override string Meta_name { get => meta_name; }
        public override string Meta_description { get => meta_description; }
        public override string Meta_version { get => meta_version; }

        public Ithaka(string[] players)
        {
            if (players.Length != 2)
            {
                throw new System.ArgumentException("You must pass an array of exactly two strings representing the players.");
            }
            var set = new HashSet<string>(players);
            if (set.Count != players.Length)
            {
                throw new System.ArgumentException("The list of players must contain no duplicates.");
            }
            this.players = players;
            this.currplayer = 0;
            this.board = "YYRRY--RB--GBBGG".ToCharArray();
            this.states = new Dictionary<string, int>();
            this.lastmoved = -1;
            this.lastmove = "";
            this.grid = new SquareFixed(4, 4);
            this.chatmsgs = new List<string>();
        }

        public Ithaka(string json)
        {
            Serialized data = JsonConvert.DeserializeObject<Serialized>(json);

            if (data.meta_name != this.Meta_name)
            {
                throw new ArgumentException("The provided game state does not represent a game of Ithaka.");
            }
            if (data.meta_version != this.Meta_version)
            {
                throw new ArgumentException("The provided game state is for a different version of Ithaka.");
            }

            this.players = data.players;
            this.currplayer = data.currplayer;
            this.board = data.board.ToCharArray();
            this.lastmoved = data.lastmoved;
            this.lastmove = data.lastmove;
            this.states = data.states;
            this.grid = new SquareFixed(4, 4);
            this.winner = data.winner;
            this.gameover = data.gameover;
            this.chatmsgs = new List<string>();
        }

        public override string Serialize()
        {
            Serialized data = new Serialized()
            {
                meta_name = this.Meta_name,
                meta_version = this.Meta_version,
                players = this.players,
                currplayer = this.currplayer,
                board = new string(this.board),
                lastmoved = this.lastmoved,
                lastmove = this.lastmove,
                states = this.states,
                gameover = this.gameover,
                winner = this.winner
            };
            return JsonConvert.SerializeObject(data);
        }

        public override string Render()
        {
            Rendered data = new Rendered()
            {
                legend = new Dictionary<string, PlayerPiece>() {
                        { "R", new PlayerPiece() { name = "piece", player = 1} },
                        { "B", new PlayerPiece() { name = "piece", player = 2} },
                        { "G", new PlayerPiece() { name = "piece", player = 3} },
                        { "Y", new PlayerPiece() { name = "piece", player = 4} }
                     },
                pieces = new string(board),
                board = new Board() { style = "squares-checkered", height = 4, width = 4 }
            };
            return JsonConvert.SerializeObject(data);
        }

        private bool CanMove(Face f)
        {
            if (!grid.ContainsFace(f))
            {
                throw new ArgumentOutOfRangeException("The face " + f.ToString() + " doesn't exist on this game board.");
            }
            char piece = board[grid.Face2FlatIdx(f)];
            if (piece == '-') { return false; }
            bool canmove = false;
            foreach (Face n in f.Neighbours())
            {
                if (! grid.ContainsFace(n)) { continue; }
                if (board[grid.Face2FlatIdx(n)] == piece)
                {
                    canmove = true;
                    break;
                }
            }
            return canmove;
        }

        private bool IsEmpty(Face f)
        {
            if (!grid.ContainsFace(f)) {
                //return false;
                throw new ArgumentOutOfRangeException("The face "+f.ToString()+" doesn't exist on this game board.");
            }
            return board[grid.Face2FlatIdx(f)] == '-';
        }

        public override IEnumerable<string> LegalMoves()
        {
            foreach (Face f in grid.Faces())
            {
                if (CanMove(f))
                {
                    foreach (Dirs dir in Enum.GetValues(typeof(Dirs)))
                    {
                        for (var dist=1; dist<=3; dist++)
                        {
                            Face n = f.Neighbour(dir, dist);
                            if (!grid.ContainsFace(n)) { break; }
                            if (board[grid.Face2FlatIdx(n)] != '-') { break; }
                            yield return f.ToLabel() + "-" + n.ToLabel();
                        }
                    }
                }
            }
        }

        public override Game Resign(string player)
        {
            Ithaka obj = new Ithaka(this.Serialize());

            if (! this.Players.Contains(player))
            {
                throw new ArgumentException("The player ID provided is not playing this game!");
            }

            obj.lastmove = null;
            obj.gameover = true;
            string winner = null;
            foreach (var p in obj.players)
            {
                if (p != player)
                {
                    winner = p;
                    break;
                }
            }
            if (winner == null)
            {
                throw new InvalidOperationException("A winner couldn't be determined after someone resigned. This should never happen!");
            }
            obj.winner = winner;
            obj.chatmsgs.Add("[u "+ player +"] resigned. The winner is [u " + obj.winner + "]!");
            return obj;
        }

        public override Game Draw()
        {
            Ithaka obj = new Ithaka(this.Serialize());
            obj.gameover = true;
            obj.winner = null;
            obj.lastmove = null;
            obj.chatmsgs.Add("The players called it a draw.");
            return obj;
        }

        public override Game Move(string player, string move)
        {
            move = move.ToLower();
            if (gameover)
            {
                throw new InvalidOperationException("The game is over, so no moves can be made.");
            }
            if (player != players[currplayer])
            {
                throw new ArgumentOutOfRangeException("It's not your turn! Current player: " + players[currplayer] + ". Attempted: " + player + ".");
            }
            Match m = re_validmove.Match(move);
            if (! m.Success)
            {
                throw new ArgumentException("Invalid move format.");
            }
            string from = m.Groups[1].ToString();
            Face ffrom = new Face(from);
            int fromidx = grid.Face2FlatIdx(ffrom);
            if (board[fromidx] == '-')
            {
                throw new ArgumentException("There's no piece at "+from+".");
            }
            if (fromidx == lastmoved)
            {
                throw new ArgumentException("You can't move the piece that was last moved.");
            }
            if (!CanMove(ffrom))
            {
                throw new ArgumentException("The piece at " + from + " cannot move. It must be orthogonally adjacent to at least one other piece of its colour.");
            }
            string to = m.Groups[2].ToString();
            Face fto = new Face(to);
            int toidx = grid.Face2FlatIdx(fto);
            if (! IsEmpty(fto))
            {
                throw new ArgumentException("You can't move onto an occupied space.");
            }
            if ( (! ffrom.OrthTo(fto)) && (! ffrom.DiagTo(fto)) )
            {
                throw new ArgumentException("You can only move pieces in a straight line.");
            }
            foreach (Face f in ffrom.Between(fto))
            {
                if (board[grid.Face2FlatIdx(f)] != '-')
                {
                    throw new ArgumentException("You can't jump over other pieces.");
                }
            }
            //LEGAL MOVE!
            Ithaka obj = new Ithaka(this.Serialize());

            obj.board[toidx] = obj.board[fromidx];
            obj.board[fromidx] = '-';

            //store lastmove and lastmoved
            obj.lastmove = move;
            obj.lastmoved = toidx;

            //Store state
            string state = new string(board);
            if (obj.states.ContainsKey(state))
            {
                obj.states[state]++;
            } else
            {
                obj.states[state] = 1;
            }
            if (obj.states[state] > 1) {
                obj.chatmsgs.Add("The current board state has been seen "+states[state].ToString()+" times. The player who repeats a position for the third time loses.");
            }

            //Check for EOG

            //repeat states
            if (obj.states[state] >= 3)
            {
                obj.gameover = true;
                obj.winner = obj.players[(currplayer + 1) % obj.players.Length];
                obj.chatmsgs.Add("The game has ended for the following reason: REPEATED BOARD POSITION. The winner is [u " + obj.winner + "]!");
            }
            //no more moves
            else if (new HashSet<string>(LegalMoves()).Count == 0)
            {
                obj.gameover = true;
                obj.winner = obj.players[currplayer];
                obj.chatmsgs.Add("The game has ended for the following reason: NO MOVES LEFT. The winner is [u " + obj.winner + "]!");
            }
            //line of 3
            else
            {
                bool isline = false;
                foreach (var line in lines)
                {
                    if ( (obj.board[line[0]] == obj.board[line[1]]) && (obj.board[line[1]] == obj.board[line[2]]) && (obj.board[line[0]] != '-') )
                    {
                        isline = true;
                        break;
                    }
                }
                if (isline)
                {
                    obj.gameover = true;
                    obj.winner = obj.players[currplayer];
                    obj.chatmsgs.Add("The game has ended for the following reason: THREE IN A ROW. The winner is [u " + obj.winner + "]!");
                }
            }

            //Update currplayer
            obj.currplayer = (obj.currplayer + 1) % obj.players.Length;

            return obj;
        }

        public override IEnumerable<IEnumerable<Object>> MovesArchive(string[] states)
        {
            List<List<Object>> retlst  = new List<List<object>>();
            Ithaka[] objs = states.Select(x => new Ithaka(x)).ToArray();
            for (var i=0; i<states.Length-1; i++)
            {
                var s1 = objs[i];
                var s2 = objs[i+1];
                if (s2.lastmove == null)
                {
                    break;
                }
                var node = new
                {
                    player = s1.Whoseturn()[0],
                    steps = new string[] {s2.lastmove}
                };
                retlst.Add(new List<Object>() {node});
            }
            return retlst;
        }
    }
}
