using Microsoft.EntityFrameworkCore;

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
                .HasPartitionKey(nameof(Family.LastName))
                .OwnsMany(f => f.Parents);

            builder.Entity<Family>()
                .OwnsMany(f => f.Children)
                    .OwnsMany(c => c.Pets);

            builder.Entity<Family>()
                .OwnsOne(f => f.Address);
        }
    }
}