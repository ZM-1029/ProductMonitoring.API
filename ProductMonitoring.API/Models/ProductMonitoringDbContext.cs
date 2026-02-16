using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProductMonitoring.API.Models;

public partial class ProductMonitoringDbContext : DbContext
{
    public ProductMonitoringDbContext()
    {
    }

    public ProductMonitoringDbContext(DbContextOptions<ProductMonitoringDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BitAddressCause> BitAddressCauses { get; set; }
    public virtual DbSet<PartMaster> PartMaster { get; set; }

    public virtual DbSet<BitAddressErrorManual> BitAddressErrorManuals { get; set; }

    public virtual DbSet<BitAddressMaster> BitAddressMasters { get; set; }

    public virtual DbSet<BitAddressRemedy> BitAddressRemedies { get; set; }

    public virtual DbSet<BitCategory> BitCategories { get; set; }
    public virtual DbSet<SolutionHistory> SolutionHistories { get; set; }


  /*  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=zoumaapp.com;Port=5432;Database=ProductMonitoringDB;Username=postgres;Password=zoumapg!@#admin;");
*/
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PartMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PartMaster_pkey");

            entity.ToTable("PartMaster");

            entity.Property(e => e.Id).HasIdentityOptions(null, null, null, 9999999999999L, null, null);
            entity.Property(e => e.Description).HasColumnType("character varying");
           // entity.Property(e => e.IsExistingSolution).HasDefaultValue(true);
        });

        modelBuilder.Entity<SolutionHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SolutionHistoryTable_pkey");

            entity.ToTable("SolutionHistory");

            entity.Property(e => e.Id).HasIdentityOptions(null, null, null, 9999999999999L, null, null);
            entity.Property(e => e.Description).HasColumnType("character varying");
            entity.Property(e => e.IsExistingSolution).HasDefaultValue(true);
        });

        modelBuilder.Entity<BitAddressCause>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BitAddressCause_pkey");

            entity.ToTable("BitAddressCause");

            entity.Property(e => e.Id).HasIdentityOptions(null, null, null, 9999999999999L, null, null);
            entity.Property(e => e.Cause).HasColumnType("character varying");
        });

        modelBuilder.Entity<BitAddressErrorManual>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BitAddressErrorManual_pkey");

            entity .ToTable("BitAddressErrorManual");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasIdentityOptions(null, null, null, 99999999999999L, null, null);
            entity.Property(e => e.ManualUrl).HasColumnType("character varying");
        });

        modelBuilder.Entity<BitAddressMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BitAddressMaster_pkey");

            entity.ToTable("BitAddressMaster");

            entity.Property(e => e.Id).HasIdentityOptions(null, null, null, 9999999999999L, null, null);
            entity.Property(e => e.Code).HasColumnType("character varying");
            entity.Property(e => e.Message).HasColumnType("character varying");
        });

        modelBuilder.Entity<BitAddressRemedy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BitAddressRemedy_pkey");

            entity.ToTable("BitAddressRemedy");

            entity.Property(e => e.Id).HasIdentityOptions(null, null, null, 99999999999999L, null, null);
            entity.Property(e => e.Remedy).HasColumnType("character varying");
        });

        modelBuilder.Entity<BitCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BitCategory_pkey");

            entity.ToTable("BitCategory");

            entity.Property(e => e.Id).HasIdentityOptions(null, null, null, 99999999999999L, null, null);
            entity.Property(e => e.Description).HasColumnType("character varying");
            entity.Property(e => e.Name).HasColumnType("character varying");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
