using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Address
    {
        public virtual int ID { get; set; }
        public virtual string Street { get; set; }
        public virtual string ZipCode { get; set; }
        public virtual IPoint Coordinates { get; set; }
        public virtual string TextReference { get; set; }
        public virtual DateTime Date { get; set; }
    }
}
