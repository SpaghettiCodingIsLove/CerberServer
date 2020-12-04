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

        public virtual DbSet<Operator> Operators { get; set; }
        public virtual DbSet<Organisation> Organisations { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Operator>(entity =>
            {
                entity.ToTable("Operator");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.OrganisationId).HasColumnName("Organisation_Id");

                entity.Property(e => e.UserId).HasColumnName("User_Id");

                entity.HasOne(d => d.Organisation)
                    .WithMany(p => p.Operators)
                    .HasForeignKey(d => d.OrganisationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Operator_Organisation");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Operators)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Operator_User");
            });

            modelBuilder.Entity<Organisation>(entity =>
            {
                entity.ToTable("Organisation");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Logo).IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.OrganisationKey)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Organisation_Key");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Email).IsRequired();

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("First_Name");

                entity.Property(e => e.Image)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.Login)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.OrganisationId).HasColumnName("Organisation_Id");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.SecondName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("Second_Name");

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
