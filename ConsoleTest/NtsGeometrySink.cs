using GeoAPI.Geometries;
using Microsoft.SqlServer.Types;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
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
