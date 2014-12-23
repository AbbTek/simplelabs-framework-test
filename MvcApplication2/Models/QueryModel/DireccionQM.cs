using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication2.Models.QueryModel
{
    public class DireccionQM
    {
        public DireccionQM() { }
        public DireccionQM(string calle, IPoint referencia)
        {
            this.Calle = calle;
            this.Referencia = referencia;
        }
        public string Calle { get; set; }

        private IPoint referencia { get; set; }
        public IPoint Referencia {
            get
            {
                if (referencia == null)
                    return new Point(X, Y);
                return referencia;
            }
            set
            {
                this.referencia = value;
            }
        }

        public double X { get; set; }
        public double Y { get; set; }
    }
}