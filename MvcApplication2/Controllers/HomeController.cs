using Domain;
using GeoAPI.Geometries;
using Microsoft.SqlServer.Types;
using MvcApplication2.Models;
using MvcApplication2.Models.QueryModel;
using NetTopologySuite.Geometries;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Spatial.Type;
using NHibernate.Transform;
using Simplelabs.Framework.Persistence.NHibernate;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Linq;
using NHibernate.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using NHibernate.Spatial.Criterion;


namespace MvcApplication2.Controllers
{
    public class HomeController : Controller
    {
        private static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["FrameworkTest"].ConnectionString;
        DomainContext context = new DomainContext();
        private static Random ran = new Random();

        //[OutputCache(Duration=3600)]
        public ActionResult Test(string id)
        {

            IndexPage page = new IndexPage();

            switch (id)
            {
                case "nhnormal":
                    page.Direcciones = ListNHNormal();
                    break;
                case "nhquerymodel":
                    page.DireccionesQM = ListNHQueryModel();
                    break;
                case "nhsql":
                    page.DireccionesQM = ListNHSql();
                    break;
                case "nhhql":
                    page.DireccionesQM = ListNHHql();
                    break;
                case "nhqueryover":
                    page.DireccionesQM = ListNHQueryOver();
                    break;
                case "nhlinq":
                    page.DireccionesQM = ListNHLinq();
                    break;
                case "sql":
                    page.DireccionesQM = ListSql();
                    break;
                case "ef":
                    page.DireccionesQM2 = EFNormal();
                    break;
                case "mongodb":
                    page.DireccionesMongo = Mongodb();
                    break;
                case "mongodblinq":
                    page.DireccionesMongo = MongodbLinq();
                    break;
                case "mongodbgis":
                    page.DireccionesMongo = MongodbGis(true, true);
                    break;
                case "mongodbgis-v":
                    page.DireccionesMongo = MongodbGis(true, false);
                    break;
                case "mongodbgis-i":
                    page.DireccionesMongo = MongodbGis(false, true);
                    break;
                case "mongodbgis-iv":
                    page.DireccionesMongo = MongodbGis(false, false);
                    break;
                case "nhgis":
                    page.Direcciones = NHGis(true, true);
                    break;
                case "nhgis-v":
                    page.Direcciones = NHGis(true, false);
                    break;
                case "nhgis-i":
                    page.Direcciones = NHGis(false, true);
                    break;
                case "nhgis-iv":
                    page.Direcciones = NHGis(false, false);
                    break;
                default:
                    break;
            }
            return View(page);
        }

        public async Task<ActionResult> TestAsync()
        {
            return View("Test", await List());
        }

        public async Task<IndexPage> List()
        {
            IndexPage page = new IndexPage();
            var list = await (from dic in context.Address
                              select new DireccionQM2
                              {
                                  Calle = dic.Street,
                                  Referencia = dic.Coordinates
                              })
                              .Take(15)
                              .ToListAsync();
            page.DireccionesQM2 = list;
            return page;
        }

        private static IList<Address> ListNHNormal()
        {
            var session = SessionFactory.GetSession();
            var criteria = session.CreateCriteria<Address>();
            criteria.SetMaxResults(15);
            criteria.Add(Expression.Eq(Projections.Property<Address>(m => m.ZipCode), ran.Next(5000).ToString()));
            return criteria.List<Address>();
        }

        private static IList<AddressQM> ListNHQueryModel()
        {
            var session = SessionFactory.GetSession();
            var criteria = session.CreateCriteria<Address>();
            criteria.SetProjection(Projections.ProjectionList()
                    .Add(Projections.Property<Address>(d => d.Street), "Street")
                    .Add(Projections.Property<Address>(d => d.Coordinates), "Coordinates")
                    .Add(Projections.Property<Address>(d => d.Date), "Date"));
            criteria.SetResultTransformer(Transformers.AliasToBean<AddressQM>());
            criteria.SetMaxResults(15);
            criteria.Add(Expression.Eq(Projections.Property<Address>(m => m.ZipCode), ran.Next(5000).ToString()));
            return criteria.List<AddressQM>();
        }

        private static IList<AddressQM> ListNHSql()
        {
            var session = SessionFactory.GetSession();
            var sql = session.CreateSQLQuery("SELECT Street, Coordinates, Date FROM Address Where ZipCode = :ZipCode")
                .AddScalar("Street", NHibernateUtil.String)
                .AddScalar("Coordinates", NHibernateUtil.Custom(typeof(GeometryType)))
                .AddScalar("Date", NHibernateUtil.Date);
            sql.SetResultTransformer(Transformers.AliasToBean<AddressQM>());
            sql.SetString("ZipCode", ran.Next(5000).ToString());
            sql.SetMaxResults(15);
            return sql.List<AddressQM>();
        }

        private static IList<AddressQM> ListNHHql()
        {
            var session = SessionFactory.GetSession();
            var sql = session.CreateQuery("SELECT Street as Street, Coordinates as Coordinates, Date as Date FROM Address Where ZipCode = :ZipCode");
            sql.SetResultTransformer(Transformers.AliasToBean<AddressQM>());
            sql.SetMaxResults(15);
            sql.SetString("ZipCode", ran.Next(5000).ToString());
            return sql.List<AddressQM>();
        }

        private static IList<AddressQM> ListNHQueryOver()
        {
            AddressQM direcionQM = null;
            var session = SessionFactory.GetSession();
            return session.QueryOver<Address>()
                .SelectList(list => list
                    .Select(d => d.Street).WithAlias(() => direcionQM.Street)
                    .Select(d => d.Coordinates).WithAlias(() => direcionQM.Coordinates))
                .TransformUsing(Transformers.AliasToBean<AddressQM>())
                .Take(15)
                .List<AddressQM>();
        }

        private static IList<AddressQM> ListNHLinq()
        {
            var session = SessionFactory.GetSession();
            var l = (from d in session.Query<Address>()
                     select new AddressQM() { Street = d.Street, Coordinates = d.Coordinates })
                     .Take(15)
                     .ToList();
            return l;
        }

        private static IList<AddressQM> ListSql()
        {
            var list = new List<AddressQM>();
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
                command.Parameters.Add(new SqlParameter("ZipCode", ran.Next(5000).ToString()));
                var r = command.ExecuteReader();
                while (r.Read())
                {
                    list.Add(new AddressQM()
                    {
                        Street = Convert.ToString(r[0]),
                        Coordinates = (IPoint)ToGeometry(r[1]),
                        Date = Convert.ToDateTime(r[2])
                    });
                }
            }
            return list;
        }

private IList<Address> NHGis(bool useSquare, bool regularSize)
{
    Random ran = new Random(DateTime.Now.Second);
    var session = SessionFactory.GetSession();
    var criteria = session.CreateCriteria<Address>();
    criteria.Add(SpatialRestrictions.Within("Coordinates", useSquare ?
        GetRandomSquare(ran, regularSize) : GetRandomIrregularPolygon(ran, regularSize)));
    return criteria.List<Address>();
}

        private IList<DireccionQM2> EFNormal()
        {
            var param = ran.Next(5000).ToString();
            var list = (from dic in context.Address
                        where dic.ZipCode == param
                        select new DireccionQM2
                         {
                             Calle = dic.Street,
                             Referencia = dic.Coordinates,
                             Fecha = dic.Date
                         }).Take(15)
                         .ToList();
            return list;
        }

        private IList<AddressMongo> Mongodb()
        {
            MongoClient client = new MongoClient(Utils.MongoDbConnection);
            MongoServer server = client.GetServer();
            MongoDatabase test = server.GetDatabase("mydb");
            var collection = test.GetCollection("address");

            var query = Query.EQ("ZipCode", ran.Next(5000).ToString());
            return collection.FindAs<AddressMongo>(query).SetLimit(15).ToList();
        }

        private IList<AddressMongo> MongodbLinq()
        {
            MongoClient client = new MongoClient(Utils.MongoDbConnection);
            MongoServer server = client.GetServer();
            MongoDatabase test = server.GetDatabase("mydb");
            var collection = test.GetCollection("address");

            var query =
                 (from e in collection.AsQueryable<AddressMongo>()
                  where e.ZipCode == ran.Next(5000).ToString()
                  select e).Take(15);

            return query.ToList();
        }

private IList<AddressMongo> MongodbGis(bool useSquare, bool regularSize)
{
    MongoClient client = new MongoClient(Utils.MongoDbConnection);
    MongoServer server = client.GetServer();
    MongoDatabase test = server.GetDatabase("mydb");
    var collection = test.GetCollection("address");

    Random ran = new Random(DateTime.Now.Millisecond);

    var poly = useSquare ? GetRandomSquare(ran, regularSize) :
        GetRandomIrregularPolygon(ran, regularSize);
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

    return collection.FindAs<AddressMongo>(new QueryDocument() { { "Coordinates", geoWithin } })
        .ToList();
}

        public ActionResult Index()
        {
            return View();
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

private static IPolygon GetRandomSquare(Random r, bool regularSize)
{
    var initX = r.Next(60, 70) + r.NextDouble();
    var initY = r.Next(20, 30) + r.NextDouble();
    var distance = r.NextDouble();

    if (regularSize)
        distance = 0.2;

    var coodinates = new List<Coordinate>();
    coodinates.Add(new Coordinate(initX, initY));
    coodinates.Add(new Coordinate(initX - distance, initY));
    coodinates.Add(new Coordinate(initX - distance, initY - distance));
    coodinates.Add(new Coordinate(initX, initY - distance));
    coodinates.Add(new Coordinate(initX, initY));
    return new Polygon(new LinearRing(coodinates.ToArray()));
}

private static IPolygon GetRandomIrregularPolygon(Random r, bool regularSize)
{
    var initX = r.Next(60, 70) + r.NextDouble();
    var initY = r.Next(20, 30) + r.NextDouble();

    var distance = r.NextDouble();
    distance = distance > 0.5 ? 0.5 : distance;

    if (regularSize)
        distance = 0.15;

    var coodinates = new List<Coordinate>();
    coodinates.Add(new Coordinate(initX, initY));
    coodinates.Add(new Coordinate(initX + distance * 2, initY - distance * 0.5));
    coodinates.Add(new Coordinate(initX + distance * 3.05, initY - distance * 2));
    coodinates.Add(new Coordinate(initX + distance * 1.5, initY - distance * 1.01));
    coodinates.Add(new Coordinate(initX + distance * 1.05, initY - distance * 2.01));
    coodinates.Add(new Coordinate(initX + distance * 2.05, initY - distance * 3.5));
    coodinates.Add(new Coordinate(initX - distance * 0.5, initY - distance * 4));
    coodinates.Add(new Coordinate(initX, initY));
    return new Polygon(new LinearRing(coodinates.ToArray()));
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