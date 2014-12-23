using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain._Map
{
    public class DireccionMap : ClassMap<Direccion>
    {
        public DireccionMap()
        {
            Cache.ReadOnly().Region("Long");
            Id(x => x.ID).GeneratedBy.Identity();
            Map(x => x.Calle);
            Map(x => x.Numero);
            Map(x => x.Referencia)
                .CustomType<GeometryType>();
        }
    }
}
