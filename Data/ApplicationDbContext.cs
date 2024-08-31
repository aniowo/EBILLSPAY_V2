using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace EBILLSPAY_V2.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<EbillsPay> EbillsPay { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=GTCollection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EbillsPay>(entity =>
        {
            entity
                .HasKey(e => e.TransId);

            entity.Property(e => e.AccountToDebit)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BankName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.BillerId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("BillerID");
            entity.Property(e => e.BillerName)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.CollectionAccNumber)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CurrApprStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CurrApprStatus");
            entity.Property(e => e.ApprovedBy)
               .HasMaxLength(200)
               .IsUnicode(false)
               .HasColumnName("ApprovedBy");
            entity.Property(e => e.InitiatedBy)
             .HasMaxLength(200)
             .IsUnicode(false)
             .HasColumnName("InitiatedBy");
            entity.Property(e => e.TransId)
                .ValueGeneratedOnAdd()
                .HasColumnName("TransId");
            entity.Property(e => e.Fee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ProductID");
            entity.Property(e => e.TellerID)
               .HasMaxLength(100)
               .IsUnicode(false)
               .HasColumnName("TellerID");
            entity.Property(e => e.BranchCode)
               .HasMaxLength(100)
               .IsUnicode(false)
               .HasColumnName("BranchCode");
            entity.Property(e => e.ProductName)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.SessionId)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("SessionID");
            entity.Property(e => e.TransDate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TransStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FieldList)
               .HasMaxLength(4000)
               .IsUnicode(false)
               .HasColumnName("FieldList"); ;
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
