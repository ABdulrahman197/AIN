using AIN.Application.Interfaces.IRepos;
using AIN.Core.Entites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AIN.Core.Enums.enums;

namespace AIN.Infrastructrue.Context
{
    public class AinDbContext : DbContext
    {
        public AinDbContext(DbContextOptions<AinDbContext> options) : base(options)
        {
        }

        public DbSet<UserAccount> Users => Set<UserAccount>();
        public DbSet<Authority> Authorities => Set<Authority>();
        public DbSet<Report> Reports => Set<Report>();
        public DbSet<Attachment> Attachments => Set<Attachment>();
        public DbSet<Like> Likes => Set<Like>();
        public DbSet<Comment> Comments => Set<Comment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Email).HasMaxLength(256);
                entity.Property(x => x.DisplayName).HasMaxLength(128);
                entity.Property(x => x.PasswordHash).HasMaxLength(512);
                entity.HasMany(x => x.Reports)
                    .WithOne(r => r.Reporter)
                    .HasForeignKey(r => r.ReporterId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Authority>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(200);
                entity.Property(x => x.Department).HasMaxLength(200);
                entity.Property(x => x.ContactEmail).HasMaxLength(256);
                entity.Property(x => x.ContactPhone).HasMaxLength(50);
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Title).HasMaxLength(200);
                entity.Property(x => x.Description).HasMaxLength(4000);
                entity.HasOne(r => r.RoutedAuthority)
                    .WithMany(a => a.AssignedReports)
                    .HasForeignKey(r => r.RoutedAuthorityId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.FileName).HasMaxLength(512);
                entity.Property(x => x.ContentType).HasMaxLength(128);
                entity.Property(x => x.StoragePath).HasMaxLength(1024);
                entity.HasOne(a => a.Report)
                    .WithMany(r => r.Attachments)
                    .HasForeignKey(a => a.ReportId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(l => l.Report)
                    .WithMany(r => r.Likes)
                    .HasForeignKey(l => l.ReportId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(l => l.User)
                    .WithMany()
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(l => new { l.UserId, l.ReportId }).IsUnique();
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Content).HasMaxLength(1000);
                entity.HasOne(c => c.Report)
                    .WithMany(r => r.Comments)
                    .HasForeignKey(c => c.ReportId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            base.OnModelCreating(modelBuilder);
        }
    }


}
