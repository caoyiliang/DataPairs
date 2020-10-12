using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataPairs.Entities
{
    internal class PairsContext : DbContext
    {
        private string _connectionString;
        public DbSet<PairsEntity> Pairs { get; set; }

        public PairsContext(string connectionString) : base()
        {
            _connectionString = connectionString;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateVersion();
            return base.SaveChangesAsync(cancellationToken);
        }
        private void UpdateVersion()
        {
            foreach (var entity in this.ChangeTracker.Entries())
            {
                if (entity.State == EntityState.Modified)
                {
                    if (entity.Entity is IVersion v)
                        v.VersionNum++;
                }
            }
        }
    }
}
