using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain._Map
{
    public class AddressMap : ClassMap<Address>
    {
        public AddressMap()
        {
            Cache.ReadOnly().Region("Long");
            Id(x => x.ID).GeneratedBy.Identity();
            Map(x => x.Street);
            Map(x => x.ZipCode);
            Map(x => x.Date);
            Map(x => x.TextReference);
            Map(x => x.Coordinates)
                .CustomType<GeometryType>();
        }
    }
}
