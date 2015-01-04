using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using Domain;
using GeoAPI.Geometries;
using MongoDB.Driver.Linq;
using System.Linq;
using System.IO;
using System.Diagnostics;
using MongoDB.Driver.Builders;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using MongoDB.Bson;

namespace TestProject
{
    [TestClass]
    public class MongodbTest
    {
        [TestMethod]
        public void ReadTest()
        {
            MongoClient client = new MongoClient(Utils.MongoDbConnection); // connect to localhost
            MongoServer server = client.GetServer();
            MongoDatabase test = server.GetDatabase("mydb");
            var collection = test.GetCollection("address");

            Random ran = new Random(DateTime.Now.Millisecond);

            var query =
                 (from e in collection.AsQueryable<AddressMongo>()
                  where e.ZipCode == "454"
                  select new { Street = e.Street, X = e.Coordinates.X, Y = e.Coordinates.Y, e.Date }
                  ).Take(50);

            var l = query.ToList();

            var query1 = Query.EQ("ZipCode", "456");     
            var r = collection.FindAs<AddressMongo>(query1).SetLimit(50).SetFields("Street" , "Coordinates", "Date").ToList();

        }

        [TestMethod]
        public void ReadGisTest()
        {
            MongoClient client = new MongoClient(Utils.MongoDbConnection); // connect to localhost
            MongoServer server = client.GetServer();
            MongoDatabase test = server.GetDatabase("mydb");
            var collection = test.GetCollection("address");

            Random ran = new Random(DateTime.Now.Millisecond);

            var poly = GetRandomPolygon(ran);
            var points = new BsonArray();
            
            foreach (var item in poly.Coordinates)
            {
                points.Add(new BsonArray(new[] { item.X, item.Y }));
            }

            BsonDocument polygon = new BsonDocument
            {
               { "type", "Polygon"},
               { "coordinates", new BsonArray() {{points}}},
            };

            BsonDocument gemotry = new BsonDocument { 
                { "$geometry" , polygon}
            };

            BsonDocument geoWithin = new BsonDocument { 
                { "$geoWithin" , gemotry}
            };

            var r = collection.FindAs<AddressMongo>(new QueryDocument(){ {"Coordinates" , geoWithin}})
                .Count();
            Assert.AreNotEqual(0, r);
        }

        [TestMethod]
        public void InsertTest()
        {
            var watch = new Stopwatch();
            watch.Start();
            //MongoClient client = new MongoClient("mongodb://10.211.55.2"); // connect to localhost
            MongoClient client = new MongoClient(Utils.MongoDbConnection);
            MongoServer server = client.GetServer();
            MongoDatabase test = server.GetDatabase("mydb");
            var result = test.GetCollection("address");
            //var r = result.FindAs<DireccionMongo>()
            Random ran = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < 1000000; i++)
            {
                AddressMongo direccion = new AddressMongo();
                direccion.Street = Path.GetRandomFileName();
                direccion.ZipCode = ran.Next(5000).ToString();
                direccion.Coordinates = new Domain.XYPoint(ran.NextDouble() + ran.Next(60, 70), ran.NextDouble() + ran.Next(20, 30));
                direccion.Date = DateTime.Now.AddYears(-1 * ran.Next(100));
                direccion.TextReference = Path.GetRandomFileName() + Path.GetRandomFileName() + Path.GetRandomFileName();
                result.Insert<AddressMongo>(direccion);
            }
            watch.Stop();
            Console.Out.WriteLine("Total de segundos {0:N}", watch.Elapsed.TotalSeconds);

            //result.Drop();
        }

        private static IPolygon GetRandomPolygon(Random r)
        {
            var initX = r.Next(60, 70);
            var initY = r.Next(20, 30);

            var coodinates = new List<Coordinate>();
            coodinates.Add(new Coordinate(initX, initY));
            coodinates.Add(new Coordinate(initX - 0.5, initY));
            coodinates.Add(new Coordinate(initX - 0.5, initY - 0.5));
            coodinates.Add(new Coordinate(initX, initY - 0.5));
            coodinates.Add(new Coordinate(initX, initY));
            return new Polygon(new LinearRing(coodinates.ToArray()));
        }

        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            BsonClassMap.RegisterClassMap<AddressMongo>(c => c.AutoMap());
        }
    }
}
