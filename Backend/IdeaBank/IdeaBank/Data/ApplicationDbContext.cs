using IdeaBank.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection.Emit;

namespace IdeaBank.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Added constructor to accept configuration options from Program.cs / Startup.cs
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Idea> Ideas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Define how the collection serializes/deserializes to the database
            var tagsConverter = new ValueConverter<List<string>, string>(
                v => v != null ? string.Join(",", v) : string.Empty,
                v => !string.IsNullOrEmpty(v)
                    ? v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    : new List<string>()
            );

            // 2. Define the ValueComparer to fix the validation warning
            var tagsComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1 == null && c2 == null || c1 != null && c2 != null && c1.SequenceEqual(c2),
                c => c != null ? c.Aggregate(0, (a, v) => HashCode.Combine(a, v != null ? v.GetHashCode() : 0)) : 0,
                c => c != null ? c.ToList() : new List<string>()
            );

            // 3. Apply both to the property
            modelBuilder.Entity<Idea>()
                .Property(e => e.Tags)
                .HasConversion(tagsConverter)
                .Metadata
                .SetValueComparer(tagsComparer);
        }
    }
}
