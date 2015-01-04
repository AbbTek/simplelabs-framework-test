using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
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
        [BsonElement("type")]
        public string Type
        {
            get
            {
                return "Point";
            }
        }

        [BsonElement("coordinates")]
        public double[] Coordinates
        {
            get;
            set;
        }

        public XYPoint(double x, double y)
        {
            Coordinates = new[] {x,y};
        }

        [BsonIgnore]
        public double X { get { return Coordinates[0]; } }

        [BsonIgnore]
        public double Y { get { return Coordinates[1]; } }
    }
}
