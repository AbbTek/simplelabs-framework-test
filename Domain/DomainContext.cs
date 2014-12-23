using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class DomainContext : DbContext
    {
        public DomainContext() : base("FrameworkTest") { }

        public DbSet<DireccionEF> Direcciones { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<DireccionEF>()
                .ToTable("Direccion");
        }
    }
}
