using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simplelabs.Framework.Persistence.NHibernate;
using Domain;
using System.IO;
using NetTopologySuite.Geometries;
using System.Diagnostics;
using NHibernate.Spatial.Criterion;
using NHibernate.Criterion;
using GeoAPI.Geometries;
using System.Collections.Generic;

namespace TestProject
{
    [TestClass]
    public class SQLTest
    {
        [TestMethod]
        public void InsertTest()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            using (var session = SessionFactory.GetSessionFactory().OpenStatelessSession())
            {
                //using (var tx = session.BeginTransaction())
                //{
                    Random ran = new Random(DateTime.Now.Millisecond);

                    for (int i = 0; i < 1000000; i++)
                    {
                        Address direccion = new Address();
                        direccion.Street = Path.GetRandomFileName();
                        direccion.ZipCode = ran.Next(5000).ToString();
                        direccion.Coordinates = new Point(ran.NextDouble() + ran.Next(60, 70), ran.NextDouble() + ran.Next(20, 30));
                        direccion.Date = DateTime.Now.AddYears(-1 * ran.Next(100));
                        direccion.TextReference = Path.GetRandomFileName() + Path.GetRandomFileName() + Path.GetRandomFileName();
                        session.Insert(direccion);
                    }
                //    tx.Commit();
                //}
            }
            watch.Stop();
            Console.Out.WriteLine("Total de segundos {0:N}", watch.Elapsed.TotalSeconds);
        }

        [TestMethod]
        public void ReadTestGis()
        {
            var session = SessionFactory.GetSession();
            var criteria = session.CreateCriteria<Address>();
            var ran = new Random();
            criteria.Add(SpatialRestrictions.Within("Coordinates", GetRandomPolygon(ran)));
            criteria.SetProjection(Projections.RowCount());
            var count = criteria.UniqueResult();

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
    }
}
