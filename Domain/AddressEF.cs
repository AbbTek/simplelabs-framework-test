using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class AddressEF
    {
        public int ID { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public DbGeometry Coordinates { get; set; }
        public virtual string TextReference { get; set; }
        public virtual DateTime Date { get; set; }
    }
}
