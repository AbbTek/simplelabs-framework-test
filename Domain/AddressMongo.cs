using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;
//using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class AddressMongo
    {
        public ObjectId _id { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public DateTime Date { get; set; }
        public XYPoint Coordinates { get; set; }
        public string TextReference { get; set; }
    }

    public class XYPoint
    {
        public XYPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
        public double X { get; set; }
        public double Y { get; set; }
    }
}
