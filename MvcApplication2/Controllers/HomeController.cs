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
            MongoClient client = new MongoClient("mongodb://localhost"); 
            MongoServer server = client.GetServer();
            MongoDatabase test = server.GetDatabase("mydb");
            var collection = test.GetCollection("address");

            var query = Query.EQ("ZipCode", ran.Next(5000).ToString());
            return collection.FindAs<AddressMongo>(query).SetLimit(15).ToList();
        }

        private IList<AddressMongo> MongodbLinq()
        {
            MongoClient client = new MongoClient("mongodb://localhost");
            MongoServer server = client.GetServer();
            MongoDatabase test = server.GetDatabase("mydb");
            var collection = test.GetCollection("address");

            var query =
                 (from e in collection.AsQueryable<AddressMongo>()
                  where e.ZipCode == ran.Next(5000).ToString()
                  select e).Take(15);

            return query.ToList();
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