using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Web;

namespace MvcApplication2.Models.QueryModel
{
    public class DireccionQM2
    {
        public DireccionQM2() { }
        public string Calle { get; set; }
        public DbGeometry Referencia { get; set; }
        public DateTime Fecha { get; set; }

    }
}