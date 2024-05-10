using Microsoft.EntityFrameworkCore;

namespace DataPairs.Entities
{
    internal class PairsContext(string connectionString) : DbContext
    {
        public DbSet<PairsEntity> Pairs => Set<PairsEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connectionString);
        }
    }
}
