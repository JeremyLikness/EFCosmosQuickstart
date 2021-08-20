using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Linq;

namespace todo
{
    public class FamilyContext : DbContext
    {
        public FamilyContext()
        {
        }

        public FamilyContext(DbContextOptions<FamilyContext> options)
            : base(options)
            {
            }

        public DbSet<Family> Families { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Family>()
                .HasPartitionKey(nameof(Family.LastName));
        }
    }
}