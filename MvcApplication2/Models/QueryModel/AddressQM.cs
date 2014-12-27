using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication2.Models.QueryModel
{
    public class AddressQM
    {
        public AddressQM() { }
        public AddressQM(string street, IPoint coorindate, DateTime date)
        {
            this.Street = street;
            this.Coordinates = coorindate;
            this.Date = date;
        }
        public string Street { get; set; }
        public DateTime Date { get; set; }
        public IPoint Coordinates {
            get;
            set;
        }
    }
}