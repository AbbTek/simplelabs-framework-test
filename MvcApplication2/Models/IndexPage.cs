using Domain;
using MvcApplication2.Models.QueryModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication2.Models
{
    public class IndexPage {
        public IList<AddressQM> DireccionesQM { get; set; }

        public IList<DireccionQM2> DireccionesQM2 { get; set; }
        public IList<Address> Direcciones { get; set; }
        public IList<AddressMongo> DireccionesMongo { get; set; }
        public long SessionOpened { get; set; }
        public long SessionClosed { get; set; }
    }
}
