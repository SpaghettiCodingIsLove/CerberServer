using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace CerberServer.Data
{
    public partial class CerberContext : DbContext
    {
        public CerberContext()
        {
        }

        public CerberContext(DbContextOptions<CerberContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Organisation> Organisations { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Organisation>(entity =>
            {
                entity.ToTable("Organisation");

                entity.Property(e => e.Logo).IsUnicode(false);

                entity.Property(e => e.Name).IsRequired();

                entity.Property(e => e.OrganisationKey)
                    .IsUnicode(false)
                    .HasColumnName("Organisation_Key");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshToken");

                entity.Property(e => e.Expires).HasColumnType("datetime");

                entity.Property(e => e.Token)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.UserId).HasColumnName("User_Id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.RefreshTokens)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RefreshToken_User");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Email).IsRequired();

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("First_Name");

                entity.Property(e => e.Image)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnName("Last_Name");

                entity.Property(e => e.Login).IsRequired();

                entity.Property(e => e.OrganisationId).HasColumnName("Organisation_Id");

                entity.Property(e => e.Password).IsRequired();

                entity.HasOne(d => d.Organisation)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.OrganisationId)
                    .HasConstraintName("FK_User_Organisation");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
