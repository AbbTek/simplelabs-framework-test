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

namespace TestProject
{
    [TestClass]
    public class MongodbTest
    {
        [TestMethod]
        public void ReadTest()
        {
            MongoClient client = new MongoClient("mongodb://localhost"); // connect to localhost
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
        public void InsertTest()
        {
            var watch = new Stopwatch();
            watch.Start();
            //MongoClient client = new MongoClient("mongodb://10.211.55.2"); // connect to localhost
            MongoClient client = new MongoClient("mongodb://localhost");
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

        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            BsonClassMap.RegisterClassMap<AddressMongo>(c => c.AutoMap());
        }
    }
}
