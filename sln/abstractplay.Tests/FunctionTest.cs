using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using Xunit;
using Xunit.Abstractions;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using abstractplay.DB;
using abstractplay.GraphQL;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;

using abstractplay;
using abstractplay.Grids.Square;
using abstractplay.Games;

namespace abstractplay.Tests
{
    public class FunctionTest
    {
        private readonly ITestOutputHelper output;

        public FunctionTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TestGuid()
        {
            HashSet<byte[]> guids = new HashSet<byte[]>();
            Dictionary<string, int> guidlist = new Dictionary<string, int>();
            int maxnum = 100;
            for (int i=0; i<maxnum; i++)
            {
                byte[] g = GuidGenerator.GenerateSequentialGuid();
                guids.Add(g);
                guidlist[GuidGenerator.HelperBAToString(g)] = i;
            }
            Assert.Equal(maxnum, guids.Count);
            List<string> keys = guidlist.Keys.ToList();
            keys.Sort();
            for (int i=1; i<keys.Count; i++)
            {
                Assert.Equal(1, guidlist[keys[i]] - guidlist[keys[i - 1]]);
            }
        }

        [Fact]
        public void GenSchema()
        {
            var ctx = new MyContext();
            var schema = new Schema { Query = new APQuery(ctx) };
            Debug.WriteLine(new SchemaPrinter(schema).Print());
        }

        // [Fact]
        // public void TestUsersPost()
        // {
        //     TestLambdaContext context;
        //     APIGatewayProxyRequest request;
        //     APIGatewayProxyResponse response;

        //     Functions functions = new Functions();

        //     Guid sub = new Guid("f686ace4-1946-4296-a4ff-6191d7e99004");

        //     context = new TestLambdaContext();
        //     request = new APIGatewayProxyRequest();
        //     request.RequestContext = new APIGatewayProxyRequest.ProxyRequestContext();
        //     request.RequestContext.Authorizer = new APIGatewayCustomAuthorizerContext();
        //     request.RequestContext.Authorizer.Claims = new Dictionary<string, string>() { { "sub", "f686ace4-1946-4296-a4ff-6191d7e99004" } };

        //     //Complete and error free
        //     string Complete_ErrorFree = "{\"displayName\": \"Perlk�nig\", \"country\": \"ca\", \"tagline\": \"Lasciate ogni speranza, voi ch'entrate!\", \"anonymous\": false, \"consent\": true}";
        //     string Minimum_ErrorFree = "{\"displayName\": \"Perlk�nig\", \"consent\": true}";
        //     string MissingName = "{\"country\": \"ca\", \"tagline\": \"Lasciate ogni speranza, voi ch'entrate!\", \"anonymous\": false, \"consent\": true}";
        //     string LongName = "{\"displayName\": \"0123456789012345678901234567890\", \"country\": \"ca\", \"tagline\": \"Lasciate ogni speranza, voi ch'entrate!\", \"anonymous\": false, \"consent\": true}";
        //     string ShortName = "{\"displayName\": \"01\", \"country\": \"ca\", \"tagline\": \"Lasciate ogni speranza, voi ch'entrate!\", \"anonymous\": false, \"consent\": true}";
        //     string MissingConsent = "{\"displayName\": \"Perlk�nig\"}";
        //     string FalseConsent = "{\"displayName\": \"Perlk�nig\", \"consent\": false}";

        //     request.Body = MissingName;
        //     response = functions.UsersPost(request, context);
        //     Assert.Equal(400, response.StatusCode);
        //     request.Body = LongName;
        //     response = functions.UsersPost(request, context);
        //     Assert.Equal(400, response.StatusCode);
        //     request.Body = ShortName;
        //     response = functions.UsersPost(request, context);
        //     Assert.Equal(400, response.StatusCode);
        //     request.Body = MissingConsent;
        //     response = functions.UsersPost(request, context);
        //     Assert.Equal(400, response.StatusCode);
        //     request.Body = FalseConsent;
        //     response = functions.UsersPost(request, context);
        //     Assert.Equal(400, response.StatusCode);
        //     request.Body = Complete_ErrorFree;
        //     response = functions.UsersPost(request, context);
        //     Assert.Equal(201, response.StatusCode);
        //     response = functions.UsersPost(request, context);
        //     Assert.Equal(400, response.StatusCode);

        //     //Minimal example then clean up
        //     using (MyContext dbc = new MyContext()) {
        //         Owners ret = dbc.Owners.Single(x => x.CognitoId.Equals(sub.ToByteArray()));
        //         dbc.Remove(ret);
        //         dbc.SaveChanges();

        //         request.Body = Minimum_ErrorFree;
        //         response = functions.UsersPost(request, context);
        //         Assert.Equal(201, response.StatusCode);
        //         ret = dbc.Owners.Single(x => x.CognitoId.Equals(sub.ToByteArray()));
        //         dbc.Remove(ret);
        //         dbc.SaveChanges();
        //     }
        // }

        // [Fact]
        // public void TestContextOwner()
        // {
        //     byte[] ownerId = GuidGenerator.GenerateSequentialGuid();
        //     Guid playerId = Guid.NewGuid();
        //     DateTime now = DateTime.UtcNow;
        //     now = new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
        //     Guid cognitoId = new Guid("f686ace4-1946-4296-a4ff-6191d7e99004");

        //     using (MyContext dbc = new MyContext()) {

        //         Owners owner;
        //         OwnersNames ne;

        //         owner = new Owners { OwnerId = ownerId, CognitoId = cognitoId.ToByteArray(), PlayerId = playerId.ToByteArray(), DateCreated = now, ConsentDate = now, Anonymous = false, Country = "CA", Tagline = "Lasciate ogni speranza, voi ch'entrate!" };
        //         ne = new OwnersNames { EntryId = GuidGenerator.GenerateSequentialGuid(), OwnerId = ownerId, EffectiveFrom = now, Name = "Perlkönig" };
        //         owner.OwnersNames.Add(ne);
        //         dbc.Add(owner);
        //         dbc.SaveChanges();
        //     }

        //     using (MyContext dbc = new MyContext()) {
        //         Owners ret;
        //         OwnersNames activeName;
        //         ret = dbc.Owners
        //             .Include(x => x.OwnersNames)
        //             .Single(x => x.OwnerId.Equals(ownerId));
        //         activeName = ret.OwnersNames.First();
        //         //output.WriteLine(JsonConvert.SerializeObject(ret, Formatting.Indented, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore}));

        //         List<NameHistory> nh = new List<NameHistory>();
        //         foreach (var e in ret.OwnersNames.ToArray())
        //         {
        //             NameHistory node = new NameHistory
        //             {
        //                 name = e.Name,
        //                 effective_date = e.EffectiveFrom.ToString("o")
        //             };
        //             nh.Add(node);
        //         }
        //         //Return the object.
        //         ResponseUser ru = new ResponseUser()
        //         {
        //             id = GuidGenerator.HelperBAToString(ownerId),
        //             name = activeName.Name,
        //             country = ret.Country,
        //             member_since = ret.DateCreated.ToString("o"),
        //             tagline = ret.Tagline,
        //             name_history = nh
        //         };
        //         string generated = JsonConvert.SerializeObject(ru);
        //         output.WriteLine(generated);

        //         ResponseUser testru = new ResponseUser()
        //         {
        //             id = GuidGenerator.HelperBAToString(ownerId),
        //             name = "Perlkönig",
        //             country = "CA",
        //             member_since = now.ToString("o"),
        //             tagline = "Lasciate ogni speranza, voi ch'entrate!",
        //             name_history = new List<NameHistory>()
        //             {
        //                 new NameHistory()
        //                 {
        //                     name = "Perlkönig",
        //                     effective_date = now.ToString("o")
        //                 }
        //             }
        //         };
        //         string constructed = JsonConvert.SerializeObject(testru);
        //         output.WriteLine(constructed);
        //         Assert.Equal(constructed, generated);

        //         //Clean up
        //         dbc.Remove(ret);
        //         dbc.SaveChanges();
        //     }
        // }

        // [Fact]
        // public async void TestGraphQL()
        // {
        //     byte[] ownerId = GuidGenerator.GenerateSequentialGuid();
        //     Guid playerId = Guid.NewGuid();
        //     DateTime now = DateTime.UtcNow;
        //     now = new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
        //     Guid cognitoId = new Guid("f686ace4-1946-4296-a4ff-6191d7e99004");

        //     using (MyContext dbc = new MyContext())
        //     {

        //         Owners owner;
        //         OwnersNames ne;

        //         owner = new Owners { OwnerId = ownerId, CognitoId = cognitoId.ToByteArray(), PlayerId = playerId.ToByteArray(), DateCreated = now, ConsentDate = now, Anonymous = false, Country = "CA", Tagline = "Lasciate ogni speranza, voi ch'entrate!" };
        //         ne = new OwnersNames { EntryId = GuidGenerator.GenerateSequentialGuid(), OwnerId = ownerId, EffectiveFrom = now, Name = "Perlkönig" };
        //         owner.OwnersNames.Add(ne);
        //         dbc.Add(owner);
        //         dbc.SaveChanges();
        //     }

        //     var ctx = new MyContext();
        //     //var query = @"{
        //     //    user(id: """ + GuidGenerator.HelperBAToString(ownerId) + @""") {
        //     //        id,
        //     //        name,
        //     //        country
        //     //    }
        //     //}";
        //     var query = @"{
        //         users(country:""CA"") {
        //             id, name, country
        //         }
        //     }";
        //     //var query = @"{
        //     //    __schema {
        //     //        queryType {
        //     //            name
        //     //        }
        //     //    }
        //     //}";
        //     var schema = new Schema { Query = new APQuery(ctx) };
        //     var result = await new DocumentExecuter().ExecuteAsync(_ =>
        //     {
        //         _.Schema = schema;
        //         _.Query = query;
        //     }).ConfigureAwait(false);
        //     var json = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        //     Console.WriteLine(json);

        //     //Clean up
        //     var ret = ctx.Owners.Single(x => x.OwnerId.Equals(ownerId));
        //     ctx.Remove(ret);
        //     ctx.SaveChanges();
        // }
        // [Fact]
        // public void TestIthakaInitMethod()
        // {
        //     TestLambdaContext context;
        //     APIGatewayProxyRequest request;
        //     APIGatewayProxyResponse response;

        //     Functions functions = new Functions();

        //     //ping

        //     //metadata

        //     //init
        //     request = new APIGatewayProxyRequest();
        //     request.Body = "{\"mode\": \"init\", \"players\": [10, 20]}";
        //     context = new TestLambdaContext();
        //     response = functions.ProcessIthaka(request, context);
        //     Assert.Equal(200, response.StatusCode);
        //     dynamic body = JsonConvert.DeserializeObject(response.Body);
        //     string state = (string)body.state.ToObject(typeof(string));
        //     Ithaka g = new Ithaka(state);
        //     Assert.Equal(new string[2] { "10", "20" }, g.players);

        //     //move
        //     request = new APIGatewayProxyRequest();
        //     request.Body = "{\"mode\": \"move\", \"player\": \"20\", \"move\": \"a1-b2\", \"state\": "+JsonConvert.ToString(state)+"}";
        //     context = new TestLambdaContext();
        //     response = functions.ProcessIthaka(request, context);
        //     Assert.Equal(400, response.StatusCode);
        // }

        [Fact]
        public void TestGrids()
        {
            //Common
            Assert.Equal(new Tuple<int, int>(0, 0), Common.Label2Coords("a1"));
            Assert.Equal(new Tuple<int, int>(25, 122), Common.Label2Coords("z123"));
            Assert.Equal("a1", Common.Coords2Label(0, 0));
            Assert.Equal("z123", Common.Coords2Label(25, 122));

            Action shortlbl = () => Common.Label2Coords("a");
            Assert.Throws<ArgumentException>(shortlbl);
            Action badlet = () => Common.Label2Coords("11");
            Assert.Throws<ArgumentException>(badlet);
            Action negy = () => Common.Label2Coords("a-20");
            Assert.Throws<ArgumentException>(negy);
            Action badnum = () => Common.Label2Coords("aa");
            Assert.Throws<FormatException>(badnum);
            Action smallx = () => Common.Coords2Label(-1, 0);
            Assert.Throws<ArgumentException>(smallx);
            Action bigx = () => Common.Coords2Label(100, 0);
            Assert.Throws<ArgumentException>(bigx);
            Action smally = () => Common.Coords2Label(0, -1);
            Assert.Throws<ArgumentException>(smally);

            //Face
            Assert.Equal(new Face(0, 0), new Face(0, 0));
            Assert.NotEqual(new Face(0, 0), new Face(0, 1));
            Assert.Equal(new Face(0, 0), new Face("a1"));
            HashSet<Face> nodiags = new HashSet<Face>()
            {
                new Face(0,1),
                new Face(1,0),
                new Face(0,-1),
                new Face(-1,0)
            };
            HashSet<Face> withdiags = new HashSet<Face>()
            {
                new Face(0,1),
                new Face(1,1),
                new Face(1,0),
                new Face(1,-1),
                new Face(0,-1),
                new Face(-1,-1),
                new Face(-1,0),
                new Face(-1, 1)
            };
            Face baseface = new Face(0, 0);
            Assert.Equal(baseface.Neighbours(false), nodiags);
            Assert.Equal(baseface.Neighbours(true), withdiags);
            Assert.Equal(baseface.Neighbour(Dirs.N), new Face(0, 1));
            Assert.Equal(baseface.Neighbour(Dirs.NE, 2), new Face(2, 2));
            Assert.Equal(baseface.Neighbour(Dirs.E, 2), new Face(2, 0));
            Assert.Equal(baseface.Neighbour(Dirs.SE, 1), new Face(1, -1));
            Assert.Equal(baseface.Neighbour(Dirs.S), new Face(0, -1));
            Assert.Equal(baseface.Neighbour(Dirs.SW, 2), new Face(-2, -2));
            Assert.Equal(baseface.Neighbour(Dirs.W, 2), new Face(-2, 0));
            Assert.Equal(baseface.Neighbour(Dirs.NW), new Face(-1, 1));
            Assert.True(baseface.OrthTo(new Face(0, 1)));
            Assert.True(baseface.OrthTo(new Face(1, 0)));
            Assert.False(baseface.OrthTo(new Face(2, 5)));
            Assert.True(baseface.DiagTo(new Face(1, 1)));
            Assert.True(baseface.DiagTo(new Face(-4, -4)));
            Assert.False(baseface.DiagTo(new Face(0, 1)));
            Assert.False(baseface.DiagTo(new Face(2, 5)));
            Assert.Null(baseface.DirectionTo(baseface));
            Assert.Equal(Dirs.N, baseface.DirectionTo(new Face(0, 10)));
            Assert.Equal(Dirs.NE, baseface.DirectionTo(new Face(2, 5)));
            Assert.Equal(Dirs.E, baseface.DirectionTo(new Face(5, 0)));
            Assert.Equal(Dirs.SE, baseface.DirectionTo(new Face(3, -3)));
            Assert.Equal(Dirs.S, baseface.DirectionTo(new Face(0, -3)));
            Assert.Equal(Dirs.SW, baseface.DirectionTo(new Face(-3, -5)));
            Assert.Equal(Dirs.W, baseface.DirectionTo(new Face(-5, 0)));
            Assert.Equal(Dirs.NW, baseface.DirectionTo(new Face(-1, 1)));

            //Action nullobj = () => baseface.Between(null);
            //Assert.Throws<ArgumentException>(nullobj);
            Action eqobj = () => baseface.Between(baseface);
            Assert.Throws<ArgumentException>(eqobj);
            Action eccentric = () => baseface.Between(new Face(1, 20));
            Assert.Throws<ArgumentException>(eccentric);
            List<Face> comp = new List<Face>()
            {
                new Face(1,1),
                new Face(2,2),
                new Face(3,3)
            };
            Assert.Equal(comp, baseface.Between(new Face(4, 4)));

            //Edge

            //Vertex

            //SquareFixed
            SquareFixed basegrid = new SquareFixed(2, 2);
            Assert.Equal(
                new HashSet<Face>() { new Face(0, 0), new Face(0, 1), new Face(1, 0), new Face(1, 1) },
                basegrid.Faces()
            );
            Assert.Equal(
                new HashSet<Vertex>() { new Vertex(0,0), new Vertex(1, 0), new Vertex(2, 0), new Vertex(0, 1), new Vertex(1, 1), new Vertex(2, 1), new Vertex(0, 2), new Vertex(1, 2), new Vertex(2, 2) },
                basegrid.Vertices()
            );
            Assert.Equal(
                new HashSet<Edge>() { new Edge(0, 0, Dirs.W), new Edge(0, 0, Dirs.S), new Edge(0, 1, Dirs.W), new Edge(0, 1, Dirs.S), new Edge(1, 0, Dirs.W), new Edge(1, 0, Dirs.S), new Edge(1, 1, Dirs.W), new Edge(1, 1, Dirs.S), new Edge(0, 2, Dirs.S), new Edge(1, 2, Dirs.S), new Edge(2, 1, Dirs.W), new Edge(2, 0, Dirs.W) },
                basegrid.Edges()
            );
            Assert.Equal((double)0, basegrid.Face2FlatIdx(new Face(0, 0)));
            Assert.Equal((double)1, basegrid.Face2FlatIdx(new Face(1, 0)));
            Assert.Equal((double)2, basegrid.Face2FlatIdx(new Face(0, 1)));
            Assert.Equal((double)3, basegrid.Face2FlatIdx(new Face(1, 1)));
            Action badface = () => basegrid.Face2FlatIdx(new Face(2, 2));
            Assert.Throws<ArgumentException>(badface);
            Assert.Equal(new Face(0, 0), basegrid.FlatIdx2Face(0));
            Assert.Equal(new Face(1, 0), basegrid.FlatIdx2Face(1));
            Assert.Equal(new Face(0, 1), basegrid.FlatIdx2Face(2));
            Assert.Equal(new Face(1, 1), basegrid.FlatIdx2Face(3));
            Assert.True(basegrid.ContainsFace(new Face(1, 1)));
            Assert.False(basegrid.ContainsFace(new Face(10, 10)));
        }

        [Fact]
        public void TestGameFactory()
        {
            Action a = () => GameFactory.CreateGame("", new string[0], new string[0]);
            Assert.Throws<System.ArgumentOutOfRangeException>(a);

            Dictionary<string,string> validgames = new Dictionary<string, string>() {
                {"ithaka", "Ithaka"}
            };
            foreach (var pair in validgames)
            {
                Assert.Equal(pair.Value, GameFactory.CreateGame(pair.Key, new string[] {"10", "20"}, new string[0]).Meta_name);
            }
        }

        [Fact]
        public void TestIthakaClassMethod()
        {
            //constructor
            Action a = () => new Ithaka(new string[2] { "10", "10" });
            Action b = () => new Ithaka(new string[1] { "10" });
            Assert.Throws<System.ArgumentException>(a);
            Assert.Throws<System.ArgumentException>(b);

            //LegalMoves
            Game basegame = new Ithaka(new string[2] { "10", "20" });
            HashSet<string> moves = new HashSet<string>()
            {
                "a1-b2", "a1-c3",
                "b1-b2", "b1-b3", "b1-c2",
                "c1-b2", "c1-c2", "c1-c3",
                "d1-c2", "d1-b3",
                "a2-b2", "a2-c2", "a2-b3",
                "d2-c2", "d2-b2", "d2-c3",
                "a3-b2", "a3-b3", "a3-c3",
                "d3-c2", "d3-c3", "d3-b3",
                "a4-b3", "a4-c2",
                "b4-b3", "b4-b2", "b4-c3",
                "c4-b3", "c4-c3", "c4-c2",
                "d4-c3", "d4-b2"
            };
            HashSet<string> actual = new HashSet<string>(basegame.LegalMoves());
            Assert.Equal(moves, actual);

            //Move
            Action wrongplayer = () => basegame.Move("20", "a1-b2");
            Assert.Throws<ArgumentOutOfRangeException>(wrongplayer);
            Action badmoveform = () => basegame.Move("10", "asdf");
            Assert.Throws<ArgumentException>(badmoveform);
            Action fromoor = () => basegame.Move("10", "z1-b2");
            Action fromempty = () => basegame.Move("10", "b2-b3");
            Assert.Throws<ArgumentException>(fromoor);
            Assert.Throws<ArgumentException>(fromempty);
            Action tooor = () => basegame.Move("10", "a1-z10");
            Action toocc = () => basegame.Move("10", "a1-b1");
            Assert.Throws<ArgumentException>(tooor);
            Assert.Throws<ArgumentException>(toocc);
            basegame = basegame.Move("10", "a1-b2");
            Assert.Equal(1, basegame.Currplayer);
            Assert.Equal("-YRRYY-RB--GBBGG", new string(((Ithaka)basegame).board));
            Assert.False(basegame.Gameover);
            basegame = basegame.Move("20", "a3-c3");
            Assert.False(basegame.Gameover);
            basegame = basegame.Move("10", "b1-c2");
            Assert.Equal("--RRYYYR--BGBBGG", new string(((Ithaka)basegame).board));
            Assert.True(basegame.Gameover);
            Assert.Equal("10", basegame.Winner);
        }
    }
}
