using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using queryHelp.Models;

namespace queryHelp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


        public DbSet<User> Users { get; set; } = null!;
        public DbSet<DataSource> DataSources { get; set; } = null!;
        public DbSet<QueryTemplate> QueryTemplates { get; set; } = null!;
        public DbSet<SavedQuery> SavedQueries { get; set; } = null!;
        public DbSet<QueryRun> QueryRuns { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<QueryTemplate>()
            .HasIndex(q => new { q.Name, q.OwnerId });


            modelBuilder.Entity<SavedQuery>()
            .HasIndex(sq => new { sq.Name, sq.OwnerId });


            modelBuilder.Entity<DataSource>()
            .HasIndex(ds => ds.Name)
            .IsUnique(false);


            
            modelBuilder.Entity<QueryTemplate>()
            .HasIndex(q => new { q.OwnerId, q.Name }).IsUnique();


           
            modelBuilder.Entity<User>().Property(u => u.Username).HasMaxLength(100);
            modelBuilder.Entity<User>().Property(u => u.Email).HasMaxLength(200);

        }
    }
}