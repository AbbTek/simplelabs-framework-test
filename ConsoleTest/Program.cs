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
                else if (bd == "sql" && (command == "r" || command == "r-direct"))
                {
                    if (command.Equals("r"))
                    {
                        ReadNHSQL(total);
                    }
                    else
                    {
                        ReadSQL(total);
                    }
                }
                else if (bd == "mongodb" && command == "i")
                {
                    InsertMongo(total);
                }
                else if (bd == "mongodb" && (command == "r" || command == "r-linq"))
                {
                    ReadMongo(total, command.Equals("r-linq"));
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
            MongoClient client = new MongoClient("mongodb://localhost");
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

        private static void ReadMongo(int total, bool linq)
        {
            MongoClient client = new MongoClient("mongodb://localhost"); // connect to localhost
            MongoServer server = client.GetServer();
            MongoDatabase test = server.GetDatabase("mydb");
            var collection = test.GetCollection("address");

            Random ran = new Random(DateTime.Now.Millisecond);
            Console.WriteLine();

            for (int i = 0; i < total; i++)
            {
                Console.Write("\r{0:N2}%   ", (i + 1) / Convert.ToDouble(total) * 100);

                if (linq)
                {
                    var query =
                            (from e in collection.AsQueryable<AddressMongo>()
                             where e.ZipCode == ran.Next(5000).ToString()
                             select e
                            ).Take(15);
                    var r = query.ToList();
                }
                else
                {
                    var query = Query.EQ("ZipCode", ran.Next(5000).ToString());
                    var r = collection.FindAs<AddressMongo>(query)
                        .SetLimit(15)
                        .SetFields("Street", "Coordinates", "Date")
                        .ToList();
                }
            }
            Console.WriteLine();
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

    internal class NtsGeometrySink : IGeometrySink
    {
        private IGeometry geometry;
        private int srid;
        private readonly Stack<OpenGisGeometryType> types = new Stack<OpenGisGeometryType>();
        private List<Coordinate> coordinates = new List<Coordinate>();
        private readonly List<Coordinate[]> rings = new List<Coordinate[]>();
        private readonly List<IGeometry> geometries = new List<IGeometry>();
        private bool inFigure;

        public IGeometry ConstructedGeometry
        {
            get { return this.geometry; }
        }

        private void AddCoordinate(double x, double y, double? z, double? m)
        {
            Coordinate coordinate;
            if (z.HasValue)
            {
                coordinate = new Coordinate(x, y, z.Value);
            }
            else
            {
                coordinate = new Coordinate(x, y);
            }
            this.coordinates.Add(coordinate);
        }

        #region IGeometrySink Members

        public void AddLine(double x, double y, double? z, double? m)
        {
            if (!this.inFigure)
            {
                throw new ApplicationException();
            }
            AddCoordinate(x, y, z, m);
        }

        public void BeginFigure(double x, double y, double? z, double? m)
        {
            if (this.inFigure)
            {
                throw new ApplicationException();
            }
            this.coordinates = new List<Coordinate>();
            AddCoordinate(x, y, z, m);
            this.inFigure = true;
        }

        public void BeginGeometry(OpenGisGeometryType type)
        {
            this.types.Push(type);
        }

        public void EndFigure()
        {
            OpenGisGeometryType type = this.types.Peek();
            if (type == OpenGisGeometryType.Polygon)
            {
                this.rings.Add(this.coordinates.ToArray());
            }
            this.inFigure = false;
        }

        public void EndGeometry()
        {
            IGeometry geometry = null;

            OpenGisGeometryType type = this.types.Pop();

            switch (type)
            {
                case OpenGisGeometryType.Point:
                    geometry = BuildPoint();
                    break;
                case OpenGisGeometryType.LineString:
                    geometry = BuildLineString();
                    break;
                case OpenGisGeometryType.Polygon:
                    geometry = BuildPolygon();
                    break;
                case OpenGisGeometryType.MultiPoint:
                    geometry = BuildMultiPoint();
                    break;
                case OpenGisGeometryType.MultiLineString:
                    geometry = BuildMultiLineString();
                    break;
                case OpenGisGeometryType.MultiPolygon:
                    geometry = BuildMultiPolygon();
                    break;
                case OpenGisGeometryType.GeometryCollection:
                    geometry = BuildGeometryCollection();
                    break;
            }

            if (this.types.Count == 0)
            {
                this.geometry = geometry;
                this.geometry.SRID = this.srid;
            }
            else
            {
                this.geometries.Add(geometry);
            }
        }

        private IGeometry BuildPoint()
        {
            return new Point(this.coordinates[0]);
        }

        private LineString BuildLineString()
        {
            return new LineString(this.coordinates.ToArray());
        }

        private IGeometry BuildPolygon()
        {
            if (this.rings.Count == 0)
            {
                return Polygon.Empty;
            }
            ILinearRing shell = new LinearRing(this.rings[0]);
            ILinearRing[] holes =
                this.rings.GetRange(1, this.rings.Count - 1)
                    .ConvertAll<ILinearRing>(delegate(Coordinate[] coordinates)
                    {
                        return new LinearRing(coordinates);
                    }).ToArray();
            this.rings.Clear();
            return new Polygon(shell, holes);
        }

        private IGeometry BuildMultiPoint()
        {
            IPoint[] points =
                this.geometries.ConvertAll<IPoint>(delegate(IGeometry g)
                {
                    return g as IPoint;
                }).ToArray();
            return new MultiPoint(points);
        }

        private IGeometry BuildMultiLineString()
        {
            ILineString[] lineStrings =
                this.geometries.ConvertAll<ILineString>(delegate(IGeometry g)
                {
                    return g as ILineString;
                }).ToArray();
            return new MultiLineString(lineStrings);
        }

        private IGeometry BuildMultiPolygon()
        {
            IPolygon[] polygons =
                this.geometries.ConvertAll<IPolygon>(delegate(IGeometry g)
                {
                    return g as IPolygon;
                }).ToArray();
            return new MultiPolygon(polygons);
        }

        private GeometryCollection BuildGeometryCollection()
        {
            return new GeometryCollection(this.geometries.ToArray());
        }

        public void SetSrid(int srid)
        {
            this.srid = srid;
        }

        #endregion
    }
}
