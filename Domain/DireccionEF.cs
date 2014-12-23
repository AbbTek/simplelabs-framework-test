using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class DireccionEF
    {
        public int ID { get; set; }
        public string Calle { get; set; }
        public string Numero { get; set; }
        public DbGeometry Referencia { get; set; }
    }
}
