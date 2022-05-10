using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DataPairs.Entities
{
    internal class PairsContext : DbContext
    {
        private string _connectionString;
        public DbSet<PairsEntity> Pairs => Set<PairsEntity>();

        public PairsContext(string connectionString) : base()
        {
            _connectionString = connectionString;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }
}
