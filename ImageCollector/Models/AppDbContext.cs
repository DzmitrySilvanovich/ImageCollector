using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ImageCollector.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
           // Database.EnsureDeleted();
            Database.EnsureCreated();
        }
        public DbSet<FileRecord> FileRecords { get; set; }
    }
}
