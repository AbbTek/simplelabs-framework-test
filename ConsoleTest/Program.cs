using Domain;
using MongoDB.Driver;
using NetTopologySuite.Geometries;
using NHibernate.Transform;
using Simplelabs.Framework.Persistence.NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using System.Data.SqlClient;
using GeoAPI.Geometries;
using Microsoft.SqlServer.Types;
using NHibernate.Spatial.Criterion;
using NHibernate.Criterion;
using MongoDB.Bson;

namespace ConsoleTest
{
    class Program
    {
        private static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["FrameworkTest"].ConnectionString;

        static void Main(string[] args)
        {
            var bd = args[0];
            var command = args[1];
            var total = Convert.ToInt32(args[2]);

            if (bd == "sql" || bd == "sql-bulk" || bd == "mongodb")
            {
                var watch = new Stopwatch();
                watch.Start();

                if (bd == "sql-bulk" && command == "i")
                {
                    InsertSQLBulk(total);
                }
                else if (bd == "sql" && command == "i")
                {
                    InsertSQL(total);
                }
                else if (bd == "sql" && (command == "r" || command == "r-direct" || command == "r-gis"))
                {
                    if (command.Equals("r"))
                    {
                        ReadNHSQL(total);
                    }
                    else if (command.Equals("r-direct"))
                    {
                        ReadSQL(total);
                    }
                    else
                    {
                        ReadSQlGis(total);
                    }
                }
                else if (bd == "mongodb" && command == "i")
                {
                    InsertMongo(total);
                }
                else if (bd == "mongodb" && (command == "r" || command == "r-linq" || command == "r-gis"))
                {
                    if (command.Equals("r"))
                    {
                        ReadMongo(total);
                    }
                    else if (command.Equals("r-linq"))
                    {
                        ReadMongoLinq(total);
                    }
                    else
                    {
                        ReadMongoGis(total);
                    }
                }

                watch.Stop();
                Console.WriteLine("{0:N2} seconds - {1:N2} minutes for {2:N0} items"
                    , watch.Elapsed.TotalSeconds
                    , watch.Elapsed.TotalMinutes
                    , total);
            }
        }

        private static void InsertMongo(int total)
        {
            MongoClient client = new MongoClient(Utils.MongoDbConnection);
            MongoServer server = client.GetServer();
            MongoDatabase test = server.GetDatabase("mydb");
            var result = test.GetCollection("address");
            Random ran = new Random(DateTime.Now.Millisecond);
            Console.WriteLine();
            for (int i = 0; i < total; i++)
            {
                Console.Write("\r{0:N2}%   ", (i + 1) / Convert.ToDouble(total) * 100);
                AddressMongo direccion = new AddressMongo();
                direccion.Street = Path.GetRandomFileName();
                direccion.ZipCode = ran.Next(5000).ToString();
                direccion.Coordinates = new Domain.XYPoint(ran.NextDouble() + ran.Next(60, 70), ran.NextDouble() + ran.Next(20, 30));
                direccion.Date = DateTime.Now.AddYears(-1 * ran.Next(100));
                direccion.TextReference = Path.GetRandomFileName() + Path.GetRandomFileName() + Path.GetRandomFileName();
                result.Insert<AddressMongo>(direccion);
            }
            Console.WriteLine();
        }

        private static void InsertSQLBulk(int total)
        {
            using (var session = SessionFactory.GetSessionFactory().OpenStatelessSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    Random ran = new Random(DateTime.Now.Millisecond);
                    Console.WriteLine();
                    for (int i = 0; i < total; i++)
                    {
                        Console.Write("\r{0:N2}%   ", (i + 1) / Convert.ToDouble(total) * 100);
                        Address direccion = new Address();
                        direccion.Street = Path.GetRandomFileName();
                        direccion.ZipCode = ran.Next(5000).ToString();
                        direccion.Coordinates = new Point(ran.NextDouble() + ran.Next(60, 70), ran.NextDouble() + ran.Next(20, 30));
                        direccion.Date = DateTime.Now.AddYears(-1 * ran.Next(100));
                        direccion.TextReference = Path.GetRandomFileName() + Path.GetRandomFileName() + Path.GetRandomFileName();
                        session.Insert(direccion);
                    }
                    Console.WriteLine();
                    tx.Commit();
                }
            }
        }

        private static void InsertSQL(int total)
        {
            using (var session = SessionFactory.GetSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    Random ran = new Random(DateTime.Now.Millisecond);
                    Console.WriteLine();
                    for (int i = 0; i < total; i++)
                    {
                        Console.Write("\r{0:N2}%   ", (i + 1) / Convert.ToDouble(total) * 100);
                        Address direccion = new Address();
                        direccion.Street = Path.GetRandomFileName();
                        direccion.ZipCode = ran.Next(5000).ToString();
                        direccion.Coordinates = new Point(ran.NextDouble() + ran.Next(60, 70), ran.NextDouble() + ran.Next(20, 30));
                        direccion.Date = DateTime.Now.AddYears(-1 * ran.Next(100));
                        direccion.TextReference = Path.GetRandomFileName() + Path.GetRandomFileName() + Path.GetRandomFileName();
                        session.Save(direccion);
                    }
                    Console.WriteLine();
                    tx.Commit();
                }
            }
        }

        private static void ReadNHSQL(int total)
        {
            using (var session = SessionFactory.GetSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    Random ran = new Random(DateTime.Now.Millisecond);
                    Console.WriteLine();
                    for (int i = 0; i < total; i++)
                    {
                        Console.Write("\r{0:N2}%   ", (i + 1) / Convert.ToDouble(total) * 100);
                        var sql = session.CreateQuery("SELECT Street, Coordinates, Date FROM Address Where ZipCode = :ZipCode");
                        sql.SetMaxResults(15);
                        sql.SetString("ZipCode", ran.Next(5000).ToString());
                        var l = sql.List();
                    }
                    Console.WriteLine();
                    tx.Commit();
                }
            }
        }

        private static void ReadSQL(int total)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(@"
                SELECT Top(15) [Street]
                      ,[Coordinates]       
                      ,[Date]               
                  FROM [dbo].[Address]
                  WHERE [ZipCode] = @ZipCode                
                ", con);
                con.Open();
                command.Parameters.Add(new SqlParameter("ZipCode", ""));
                Console.WriteLine();
                Random ran = new Random(DateTime.Now.Second);
                for (int i = 0; i < total; i++)
                {
                    Console.Write("\r{0:N2}%   ", (i + 1) / Convert.ToDouble(total) * 100);
                    command.Parameters[0].Value = ran.Next(5000).ToString();
                    var r = command.ExecuteReader();
                    while (r.Read())
                    {
                        var Calle = Convert.ToString(r[0]);
                        var Referencia = (IPoint)ToGeometry(r[1]);
                        var Fecha = Convert.ToDateTime(r[2]);
                    }
                    r.Close();
                }
                Console.WriteLine();
            }
        }

        private static void ReadMongo(int total)
        {
            MongoClient client = new MongoClient(Utils.MongoDbConnection); // connect to localhost
            MongoServer server = client.GetServer();
            MongoDatabase test = server.GetDatabase("mydb");
            var collection = test.GetCollection("address");

            Random ran = new Random(DateTime.Now.Millisecond);
            Console.WriteLine();

            for (int i = 0; i < total; i++)
            {
                Console.Write("\r{0:N2}%   ", (i + 1) / Convert.ToDouble(total) * 100);

                    var query = Query.EQ("ZipCode", ran.Next(5000).ToString());
                    var r = collection.FindAs<AddressMongo>(query)
                        .SetLimit(15)
                        .SetFields("Street", "Coordinates", "Date")
                        .ToList();
            }
            Console.WriteLine();
        }

        private static void ReadMongoLinq(int total)
        {
            MongoClient client = new MongoClient(Utils.MongoDbConnection); // connect to localhost
            MongoServer server = client.GetServer();
            MongoDatabase test = server.GetDatabase("mydb");
            var collection = test.GetCollection("address");

            Random ran = new Random(DateTime.Now.Millisecond);
            Console.WriteLine();

            for (int i = 0; i < total; i++)
            {
                Console.Write("\r{0:N2}%   ", (i + 1) / Convert.ToDouble(total) * 100);


                var query =
                        (from e in collection.AsQueryable<AddressMongo>()
                         where e.ZipCode == ran.Next(5000).ToString()
                         select e
                        ).Take(15);
                var r = query.ToList();
            }
            Console.WriteLine();
        }

        public static void ReadMongoGis(int total)
        {
            MongoClient client = new MongoClient(Utils.MongoDbConnection); // connect to localhost
            MongoServer server = client.GetServer();
            MongoDatabase test = server.GetDatabase("mydb");
            var collection = test.GetCollection("address");

            Random ran = new Random(DateTime.Now.Millisecond);
            Console.WriteLine();

            for (int i = 0; i < total; i++)
            {
                Console.Write("\r{0:N2}%   ", (i + 1) / Convert.ToDouble(total) * 100);

                var poly = GetRandomPolygon(ran);
                var points = new BsonArray();

                foreach (var item in poly.Coordinates)
                {
                    points.Add(new BsonArray(new[] { item.X, item.Y }));
                }

                BsonDocument polygon = new BsonDocument
            {
               { "type", "Polygon"},
               { "coordinates", new BsonArray() {{points}}},
            };

                BsonDocument gemotry = new BsonDocument { 
                { "$geometry" , polygon}
            };

                BsonDocument geoWithin = new BsonDocument { 
                { "$geoWithin" , gemotry}
            };

                var r = collection.FindAs<AddressMongo>(new QueryDocument() { { "Coordinates", geoWithin } })
                    .Count();
            }
            Console.WriteLine();
        }

        private static void ReadSQlGis(int total)
        {
            Console.WriteLine();
            Random ran = new Random(DateTime.Now.Second);
            for (int i = 0; i < total; i++)
            {
                Console.Write("\r{0:N2}%   ", (i + 1) / Convert.ToDouble(total) * 100);
                var session = SessionFactory.GetSession();
                var criteria = session.CreateCriteria<Address>();
                criteria.Add(SpatialRestrictions.Within("Coordinates", GetRandomPolygon(ran)));
                criteria.SetProjection(Projections.RowCount());
                var count = criteria.UniqueResult();
            }
            Console.WriteLine();
        }

        private static IPolygon GetRandomPolygon(Random r)
        {
            var initX = r.Next(60, 70);
            var initY = r.Next(20, 30);

            var coodinates = new List<Coordinate>();
            coodinates.Add(new Coordinate(initX, initY));
            coodinates.Add(new Coordinate(initX - 0.5, initY));
            coodinates.Add(new Coordinate(initX - 0.5, initY - 0.5));
            coodinates.Add(new Coordinate(initX, initY - 0.5));
            coodinates.Add(new Coordinate(initX, initY));
            return new Polygon(new LinearRing(coodinates.ToArray()));
        }

        protected static IGeometry ToGeometry(object value)
        {
            SqlGeometry sqlGeometry = value as SqlGeometry;

            if (sqlGeometry == null || sqlGeometry.IsNull)
            {
                return null;
            }

            NtsGeometrySink builder = new NtsGeometrySink();
            sqlGeometry.Populate(builder);
            return builder.ConstructedGeometry;
        }
    }
}
