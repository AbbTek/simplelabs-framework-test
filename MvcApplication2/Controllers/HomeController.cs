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

namespace MvcApplication2.Controllers
{
    public class HomeController : Controller
    {
        private static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["FrameworkTest"].ConnectionString;
        DomainContext context = new DomainContext();

        //[OutputCache(Duration=3600)]
        public ActionResult Test(string id)
        {

            IndexPage page = new IndexPage();

            switch (id)
            {
                case "nhnormal":
                    page.Direcciones = ListNHNormal();
                    break;
                case "nhnormalcache":
                    page.Direcciones = ListNHNormalCache();
                    break;
                case "nhquerymodel":
                    page.DireccionesQM = ListNHQueryModel();
                    break;
                case "nhquerymodelcache":
                    page.DireccionesQM = ListNHQueryModelCache();
                    break;
                case "nhsql":
                    page.DireccionesQM = ListNHSql();
                    break;
                case "nhsqlcache":
                    page.DireccionesQM = ListNHSqlCache();
                    break;
                case "nhhql":
                    page.DireccionesQM = ListNHHql();
                    break;
                case "nhhqlcache":
                    page.DireccionesQM = ListNHHqlCache();
                    break;
                case "nhqueryover":
                    page.DireccionesQM = ListNHQueryOver();
                    break;
                case "nhqueryovercache":
                    page.DireccionesQM = ListNHQueryOverCache();
                    break;
                case "nhlinq":
                    page.DireccionesQM = ListNHLinq();
                    break;
                case "nhlinqcache":
                    page.DireccionesQM = ListNHLinqCache();
                    break;
                case "sql":
                    page.DireccionesQM = ListSql();
                    break;
                case "ef":
                    page.DireccionesQM2 = EFNormal();
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
            var list = await (from dic in context.Direcciones
                              select new DireccionQM2
                              {
                                  Calle = dic.Calle,
                                  Referencia = dic.Referencia
                              }).ToListAsync();
            page.DireccionesQM2 = list;
            return page;
        }

        private static IList<Direccion> ListNHNormal()
        {
            var session = SessionFactory.GetSession();
            var criteria = session.CreateCriteria<Direccion>();
            return criteria.List<Direccion>();
        }

        private static IList<Direccion> ListNHNormalCache()
        {
            var session = SessionFactory.GetSession();
            var criteria = session.CreateCriteria<Direccion>();
            criteria.SetCacheable(true);
            criteria.SetCacheRegion("long");
            return criteria.List<Direccion>();
        }

        private static IList<DireccionQM> ListNHQueryModel()
        {
            var session = SessionFactory.GetSession();
            var criteria = session.CreateCriteria<Direccion>();
            criteria.SetProjection(Projections.ProjectionList()
                    .Add(Projections.Property<Direccion>(d => d.Calle), "Calle")
                    .Add(Projections.Property<Direccion>(d => d.Referencia), "Referencia"));
            criteria.SetResultTransformer(Transformers.AliasToBean<DireccionQM>());
            return criteria.List<DireccionQM>();
        }

        private static IList<DireccionQM> ListNHQueryModelCache()
        {
            var session = SessionFactory.GetSession();
            var criteria = session.CreateCriteria<Direccion>();
            criteria.SetProjection(Projections.ProjectionList()
                    .Add(Projections.Property<Direccion>(d => d.Calle), "Calle")
                    .Add(Projections.Property<Direccion>(d => d.Referencia), "Referencia"));
            criteria.SetResultTransformer(Transformers.AliasToBean<DireccionQM>());
            criteria.SetCacheable(true);
            criteria.SetCacheRegion("long");
            return criteria.List<DireccionQM>();
        }

        private static IList<DireccionQM> ListNHSql()
        {
            var session = SessionFactory.GetSession();
            var sql = session.CreateSQLQuery("SELECT Calle, Referencia FROM Direccion")
                .AddScalar("Calle", NHibernateUtil.String)
                .AddScalar("Referencia", NHibernateUtil.Custom(typeof(GeometryType)));
            sql.SetResultTransformer(Transformers.AliasToBean<DireccionQM>());
            return sql.List<DireccionQM>();
        }

        private static IList<DireccionQM> ListNHSqlCache()
        {
            var session = SessionFactory.GetSession();
            var sql = session.CreateSQLQuery("SELECT Calle, Referencia FROM Direccion")
                .AddScalar("Calle", NHibernateUtil.String)
                .AddScalar("Referencia", NHibernateUtil.Custom(typeof(GeometryType)));
            sql.SetResultTransformer(Transformers.AliasToBean<DireccionQM>());
            sql.SetCacheable(true);
            sql.SetCacheRegion("long");
            return sql.List<DireccionQM>();
        }

        private static IList<DireccionQM> ListNHHql()
        {
            var session = SessionFactory.GetSession();
            var sql = session.CreateQuery("SELECT Calle as Calle, Referencia as Referencia FROM Direccion");
            sql.SetResultTransformer(Transformers.AliasToBean<DireccionQM>());
            return sql.List<DireccionQM>();
        }

        private static IList<DireccionQM> ListNHHqlCache()
        {
            var session = SessionFactory.GetSession();
            var sql = session.CreateQuery("SELECT Calle as Calle, Referencia as Referencia FROM Direccion");
            sql.SetResultTransformer(Transformers.AliasToBean<DireccionQM>());
            sql.SetCacheable(true);
            sql.SetCacheRegion("long");
            return sql.List<DireccionQM>();
        }

        private static IList<DireccionQM> ListNHQueryOver()
        {
            DireccionQM direcionQM = null;
            var session = SessionFactory.GetSession();
            return session.QueryOver<Direccion>()
                .SelectList(list => list
                    .Select(d => d.Calle).WithAlias(() => direcionQM.Calle)
                    .Select(d => d.Referencia).WithAlias(() => direcionQM.Referencia))
                .TransformUsing(Transformers.AliasToBean<DireccionQM>())
                .List<DireccionQM>();
        }

        private static IList<DireccionQM> ListNHQueryOverCache()
        {
            DireccionQM direcionQM = null;
            var session = SessionFactory.GetSession();
            return session.QueryOver<Direccion>()
                .SelectList(list => list
                    .Select(d => d.Calle).WithAlias(() => direcionQM.Calle)
                    .Select(d => d.Referencia).WithAlias(() => direcionQM.Referencia))
                .TransformUsing(Transformers.AliasToBean<DireccionQM>())
                .Cacheable()
                .CacheRegion("long")
                .List<DireccionQM>();
        }

        private static IList<DireccionQM> ListNHLinq()
        {
            var session = SessionFactory.GetSession();
            var l = (from d in session.Query<Direccion>()
                     select new DireccionQM() { Calle = d.Calle, Referencia = d.Referencia })
                         .ToList();
            return l;
        }

        private static IList<DireccionQM> ListNHLinqCache()
        {
            var session = SessionFactory.GetSession();
            var l = (from d in session.Query<Direccion>()
                     select new DireccionQM() { Calle = d.Calle, Referencia = d.Referencia })
                     .Cacheable()
                     .CacheRegion("long")
                     .ToList();
            return l;
        }

        private static IList<DireccionQM> ListSql()
        {
            var list = new List<DireccionQM>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(@"
                SELECT [Calle]
                      ,[Referencia]                      
                  FROM [dbo].[Direccion]                
                ", con);
                con.Open();
                var r = command.ExecuteReader();
                while (r.Read())
                {
                    list.Add(new DireccionQM()
                    {
                        Calle = Convert.ToString(r[0]),
                        Referencia = (IPoint)ToGeometry(r[1])
                    });
                }
            }
            return list;
        }

        private IList<DireccionQM2> EFNormal()
        {
            //using (var context = new DomainContext())
            //{
            var list = (from dic in context.Direcciones
                        select new DireccionQM2
                         {
                             Calle = dic.Calle,
                             Referencia = dic.Referencia
                         }).ToList();
            return list;
            //}
        }

        //[OutputCache(Duration = 10)]
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