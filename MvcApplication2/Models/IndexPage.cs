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
        public IList<DireccionQM> DireccionesQM { get; set; }

        public IList<DireccionQM2> DireccionesQM2 { get; set; }
        public IList<Direccion> Direcciones { get; set; }
        public long SessionOpened { get; set; }
        public long SessionClosed { get; set; }
    }
}
