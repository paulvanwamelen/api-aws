using System;
using System.Collections.Generic;
using System.Text;

// http://www-cs-students.stanford.edu/~amitp/game-programming/grids/

namespace abstractplay.Grids.Square
{
    public enum Dirs : uint { N, NE, E, SE, S, SW, W, NW };

    public static class Common
    {
        public static Dictionary<Dirs, Dirs> opps = new Dictionary<Dirs, Dirs>()
        {
            {Dirs.N, Dirs.S },
            {Dirs.NE, Dirs.SW },
            {Dirs.E, Dirs.W },
            {Dirs.SE, Dirs.NW },
            {Dirs.S, Dirs.N },
            {Dirs.SW, Dirs.NE },
            {Dirs.W, Dirs.E },
            {Dirs.NW, Dirs.SE }
        };

        public static Tuple<int, int> Label2Coords(string lbl)
        {
            if (lbl.Length < 2)
            {
                throw new System.ArgumentException("The label must be at least two characters long, consisting of a lowercase letter followed by one or more digits.");
            }
            char letter = lbl.ToLower().Substring(0, 1).ToCharArray()[0];
            int x = (int)letter - (int)'a';
            if ( (x < 0) || (x > 25) )
            {
                throw new System.ArgumentException("The first character must be the letter 'a' through 'z'.");
            }

            string number = lbl.Substring(1);
            int y = Convert.ToInt32(number)-1;
            if (y < 0)
            {
                throw new System.ArgumentException("The y coordinate may not be negative.");
            }

            return new Tuple<int, int>(x, y);
        }

        public static string Coords2Label(int x, int y)
        {
            if ( (x < 0) || (x > 25) )
            {
                throw new System.ArgumentException("The x coordinate must be between 0 and 25 inclusive.");
            }
            if (y < 0)
            {
                throw new System.ArgumentException("The y coordinate cannot be negative.");
            }
            char letter = (char)(x + (int)'a');
            return letter.ToString() + (y+1).ToString();
        }
    }

    public class Face
    {
        public static Dictionary<Dirs, Tuple<int, int>> offsets = new Dictionary<Dirs, Tuple<int, int>>()
        {
            {Dirs.N, new Tuple<int, int>(0,1) },
            {Dirs.NE, new Tuple<int, int>(1,1) },
            {Dirs.E, new Tuple<int, int>(1,0) },
            {Dirs.SE, new Tuple<int, int>(1,-1) },
            {Dirs.S, new Tuple<int, int>(0,-1) },
            {Dirs.SW, new Tuple<int, int>(-1,-1) },
            {Dirs.W, new Tuple<int, int>(-1,0) },
            {Dirs.NW, new Tuple<int, int>(-1,1) }
        };
        public int x, y;

        public Face(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Face(Tuple<int, int> coords) : this(coords.Item1, coords.Item2) { }

        public Face(string label) : this(Common.Label2Coords(label)) { }

        public string ToLabel()
        {
            return Common.Coords2Label(this.x, this.y);
        }

        public IEnumerable<Face> Neighbours(bool diag=false)
        {
            foreach (KeyValuePair<Dirs, Tuple<int,int>> entry in offsets) 
            {
                //Even-numbered Dirs are orth
                if ( (diag) || ((uint)entry.Key % 2 == 0) )
                {
                    yield return new Face(this.x + entry.Value.Item1, this.y + entry.Value.Item2);
                }
            }
        }

        public Face Neighbour(Dirs dir, int dist=1)
        {
            if (! offsets.ContainsKey(dir))
            {
                throw new ArgumentException("The direction you requested is not recognized.");
            }
            Tuple<int, int> val = offsets[dir];
            return new Face(this.x + (val.Item1 * dist), this.y + (val.Item2 * dist));
        }

        public IEnumerable<Edge> Borders()
        {
            yield return new Edge(this.x, this.y, Dirs.W);
            yield return new Edge(this.x, this.y, Dirs.S);
            yield return new Edge(this.x + 1, this.y, Dirs.W);
            yield return new Edge(this.x, this.y + 1, Dirs.S);
        }

        public IEnumerable<Vertex> Corners()
        {
            yield return new Vertex(this.x + 1, this.y + 1);
            yield return new Vertex(this.x + 1, this.y);
            yield return new Vertex(this.x, this.y);
            yield return new Vertex(this.x, this.y + 1);
        }

        public bool OrthTo(Face obj) => ((this.x == obj.x) || (this.y == obj.y));

        public bool DiagTo(Face obj) => Math.Abs(this.x - obj.x) == Math.Abs(this.y - obj.y);

        public Dirs? DirectionTo(Face obj)
        {
            if (obj == this)
            {
                return null;
            }
            if (obj.x == this.x)
            {
                if (obj.y > this.y)
                {
                    return Dirs.N;
                } else
                {
                    return Dirs.S;
                }
            } else if (obj.y == this.y)
            {
                if (obj.x > this.x)
                {
                    return Dirs.E;
                } else
                {
                    return Dirs.W;
                }
            } else if (obj.x > this.x)
            {
                if (obj.y > this.y)
                {
                    return Dirs.NE;
                } else
                {
                    return Dirs.SE;
                }
            } else //if (obj.x < this.x)
            {
                if (obj.y > this.y)
                {
                    return Dirs.NW;
                }
                else
                {
                    return Dirs.SW;
                }
            }
        }

        public List<Face> Between(Face obj)
        {
            if ( (this == obj) || ( (!this.OrthTo(obj)) && (!this.DiagTo(obj)) ) )
            {
                throw new ArgumentException("The two Faces must be directly orthogonal or diagonal from each other.");
            }
            List<Face> lst = new List<Face>();
            Dirs dir = (Dirs)this.DirectionTo(obj);
            Face next = this.Neighbour(dir);
            while (next != obj)
            {
                lst.Add(next);
                next = next.Neighbour(dir);
            }
            return lst;
        }

        public int OrthDistance(Face obj) => Math.Abs(this.x - obj.x) + Math.Abs(this.y - obj.y);

        //OVERRIDES
        public override string ToString() => "Face<(" + this.x + ", " + this.y + ")>";

        public static bool operator ==(Face lhs, Face rhs)
        {
            if ( (lhs.x == rhs.x) && (lhs.y == rhs.y))
            {
                return true;
            }
            return false;
        }

        public static bool operator !=(Face lhs, Face rhs)
        {
            if ((lhs.x == rhs.x) && (lhs.y == rhs.y))
            {
                return false;
            }
            return true;
        }

        public override bool Equals(Object obj) => obj is Face && this == (Face)obj;

        public override int GetHashCode() => this.x ^ this.y;
    }

    public class Edge
    {
        public int x, y;
        public Dirs dir;

        public Edge(int x, int y, Dirs dir)
        {
            if ( (dir != Dirs.W) && (dir != Dirs.S) )
            {
                throw new ArgumentException("Edges must be West or South.");
            }
            this.x = x;
            this.y = y;
            this.dir = dir;
        }

        public IEnumerable<Face> Joins()
        {
            if (this.dir == Dirs.W)
            {
                yield return new Face(this.x, this.y);
                yield return new Face(this.x-1, this.y);
            } else
            {
                yield return new Face(this.x, this.y);
                yield return new Face(this.x, this.y-1);
            }
        }

        public IEnumerable<Edge> Continues()
        {
            if (this.dir == Dirs.W)
            {
                yield return new Edge(this.x, this.y + 1, Dirs.W);
                yield return new Edge(this.x, this.y - 1, Dirs.W);
            } else
            {
                yield return new Edge(this.x + 1, this.y, Dirs.S);
                yield return new Edge(this.x - 1, this.y, Dirs.S);
            }
        }

        public IEnumerable<Vertex> Endpoints()
        {
            if (this.dir == Dirs.W)
            {
                yield return new Vertex(this.x, this.y);
                yield return new Vertex(this.x, this.y+1);
            } else
            {
                yield return new Vertex(this.x, this.y);
                yield return new Vertex(this.x+1, this.y);
            }
        }

        //Overrides
        public override string ToString() => "Edge<(" + this.x + ", " + this.y + ")>";

        public static bool operator ==(Edge lhs, Edge rhs)
        {
            if ((lhs.x == rhs.x) && (lhs.y == rhs.y) && (lhs.dir == rhs.dir))
            {
                return true;
            }
            return false;
        }

        public static bool operator !=(Edge lhs, Edge rhs)
        {
            if ((lhs.x == rhs.x) && (lhs.y == rhs.y) && (lhs.dir == rhs.dir))
            {
                return false;
            }
            return true;
        }

        public override bool Equals(Object obj) => obj is Edge && this == (Edge)obj;

        public override int GetHashCode() => this.x ^ this.y ^ (int)this.dir;
    }

    public class Vertex : Face
    {
        public Vertex (int x, int y) : base(x,y) { }
        public override string ToString() => "Vertex<(" + this.x + ", " + this.y + ")>";

        public IEnumerable<Face> Touches()
        {
            yield return new Face(this.x, this.y);
            yield return new Face(this.x, this.y - 1);
            yield return new Face(this.x - 1, this.y - 1);
            yield return new Face(this.x - 1, this.y);
        }

        public IEnumerable<Edge> Protrudes()
        {
            yield return new Edge(this.x, this.y, Dirs.W);
            yield return new Edge(this.x, this.y, Dirs.S);
            yield return new Edge(this.x, this.y - 1, Dirs.W);
            yield return new Edge(this.x - 1, this.y, Dirs.S);
        }

        public IEnumerable<Vertex> Adjacent()
        {
            yield return new Vertex(this.x, this.y + 1);
            yield return new Vertex(this.x + 1, this.y);
            yield return new Vertex(this.x, this.y - 1);
            yield return new Vertex(this.x - 1, this.y);
        }
    }

    public class Square
    {

    }

    public class SquareFixed
    {
        public uint width, height;

        public SquareFixed(uint width, uint height)
            : base()
        {
            this.width = width;
            this.height = height;
        }

        public IEnumerable<Face> Faces()
        {
            for (int x=0; x<this.width; x++)
            {
                for (int y=0; y<this.height; y++)
                {
                    yield return new Face(x, y);
                }
            }
        }

        public int Face2FlatIdx(Face f)
        {
            if ( (f.x >= this.width) || (f.y >= this.height) )
            {
                throw new ArgumentException("The Face lies outside the defined grid.");
            }
            return (int)(f.y * this.width) + f.x;
        }

        public Face FlatIdx2Face(uint idx)
        {
            uint y = idx / this.width;
            uint x = idx % this.width;
            return new Face((int)x, (int)y);
        }

        public HashSet<Vertex> Vertices()
        {
            HashSet<Vertex> set = new HashSet<Vertex>();
            foreach (Face f in this.Faces())
            {
                set.UnionWith(f.Corners());
            }
            return set;
        }

        public HashSet<Edge> Edges()
        {
            HashSet<Edge> set = new HashSet<Edge>();
            foreach (Face f in this.Faces())
            {
                set.UnionWith(f.Borders());
            }
            return set;
        }

        public IEnumerable<Face> Row(uint rownum)
        {
            if (rownum > this.height)
            {
                throw new ArgumentException("The requested row doesn't exist.");
            }
            for (int x=0; x<this.width; x++)
            {
                yield return new Face(x, (int)rownum);
            }
        }

        public IEnumerable<List<Face>> Rows()
        {
            for (uint y=0; y<this.height; y++)
            {
                yield return new List<Face>(this.Row(y));
            }
        }

        public IEnumerable<Face> Col(uint colnum)
        {
            if (colnum > this.height)
            {
                throw new ArgumentException("The requested col doesn't exist.");
            }
            for (int x = 0; x < this.width; x++)
            {
                yield return new Face(x, (int)colnum);
            }
        }

        public IEnumerable<List<Face>> Cols()
        {
            for (uint x = 0; x < this.width; x++)
            {
                yield return new List<Face>(this.Col(x));
            }
        }

        public IEnumerable<List<Face>> Diags()
        {
            List<Face> lst = new List<Face>();
            for (int i=0; i<this.height; i++)
            {
                lst.Add(new Face(i, i));
            }
            yield return lst;

            lst = new List<Face>();
            for (int i=0; i<this.height; i++)
            {
                lst.Add(new Face(i, (int)this.height - 1 - i));
            }
            yield return lst;
        }

        public bool ContainsFace(Face obj)
        {
            if ( (obj.x >= 0) && (obj.x < this.width) && (obj.y >= 0) && (obj.y < this.height) )
            {
                return true;
            }
            return false;
        }
    }
}
