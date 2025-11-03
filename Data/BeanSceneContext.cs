using System;
using System.Collections.Generic;
using BeanScene.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BeanScene.Web.Data;

public partial class BeanSceneContext : DbContext
{
    public BeanSceneContext()
    {
    }

    public BeanSceneContext(DbContextOptions<BeanSceneContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Area> Areas { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<RestaurantTable> RestaurantTables { get; set; }

    public virtual DbSet<SittingSchedule> SittingSchedules { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.AreaId).HasName("PK__Area__70B82028FEDCE2CC");

            entity.ToTable("Area");

            entity.HasIndex(e => e.AreaName, "UQ__Area__8EB6AF57AFF687E8").IsUnique();

            entity.Property(e => e.AreaId).HasColumnName("AreaID");
            entity.Property(e => e.AreaName)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.ReservationId).HasName("PK__Reservat__B7EE5F0404326B6B");

            entity.ToTable("Reservation");

            entity.HasIndex(e => new { e.SittingId, e.StartTime }, "IX_Reservation_Sitting_Start");

            entity.HasIndex(e => e.Status, "IX_Reservation_Status");

            entity.Property(e => e.ReservationId).HasColumnName("ReservationID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Notes)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ReservationSource)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SittingId).HasColumnName("SittingID");
            entity.Property(e => e.StartTime).HasPrecision(0);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Sitting).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.SittingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reservation_Sitting");

            entity.HasMany(d => d.RestaurantTables).WithMany(p => p.ReservationTables)
                .UsingEntity<Dictionary<string, object>>(
                    "ReservationTable",
                    r => r.HasOne<RestaurantTable>().WithMany()
                        .HasForeignKey("RestaurantTableId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ReservationTable_Table"),
                    l => l.HasOne<Reservation>().WithMany()
                        .HasForeignKey("ReservationTableId")
                        .HasConstraintName("FK_ReservationTable_Reservation"),
                    j =>
                    {
                        j.HasKey("ReservationTableId", "RestaurantTableId");
                        j.ToTable("ReservationTable");
                        j.HasIndex(new[] { "RestaurantTableId" }, "IX_ReservationTable_Table");
                        j.IndexerProperty<int>("ReservationTableId").HasColumnName("ReservationTableID");
                        j.IndexerProperty<int>("RestaurantTableId").HasColumnName("RestaurantTableID");
                    });
        });

        modelBuilder.Entity<RestaurantTable>(entity =>
        {
            entity.HasKey(e => e.RestaurantTableId).HasName("PK__Restaura__15D937BA28FD3CE3");

            entity.ToTable("RestaurantTable");

            entity.HasIndex(e => new { e.AreaId, e.TableName }, "UQ_Area_Table").IsUnique();

            entity.Property(e => e.RestaurantTableId).HasColumnName("RestaurantTableID");
            entity.Property(e => e.AreaId).HasColumnName("AreaID");
            entity.Property(e => e.TableName)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.Area).WithMany(p => p.RestaurantTables)
                .HasForeignKey(d => d.AreaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Table_Area");
        });

        modelBuilder.Entity<SittingSchedule>(entity =>
        {
            entity.HasKey(e => e.SittingScheduleId).HasName("PK__SittingS__AE09170F976F16D2");

            entity.ToTable("SittingSchedule");

            entity.HasIndex(e => new { e.Stype, e.StartDateTime, e.EndDateTime }, "UQ_Sitting_Window").IsUnique();

            entity.Property(e => e.SittingScheduleId).HasColumnName("SittingScheduleID");
            entity.Property(e => e.EndDateTime).HasPrecision(0);
            entity.Property(e => e.Scapacity).HasColumnName("SCapacity");
            entity.Property(e => e.StartDateTime).HasPrecision(0);
            entity.Property(e => e.Status)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.Stype)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("SType");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
