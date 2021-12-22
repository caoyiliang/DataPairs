using Microsoft.EntityFrameworkCore;

namespace DataPairs.Entities
{
    internal class PairsContext : DbContext
    {
        private string _connectionString;
        public DbSet<PairsEntity> Pairs { get; set; }

        public PairsContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlite(new SqliteConnectionStringBuilder(_connectionString)
            //{
            //    Mode = SqliteOpenMode.ReadWriteCreate,
            //    Password = "cd+8KpaWULi/W/jJNT3flg=="
            //}.ToString());
            optionsBuilder.UseSqlite(_connectionString);
        }
    }
}
