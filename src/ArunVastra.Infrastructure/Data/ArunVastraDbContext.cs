using System;
using System.Collections.Generic;
using ArunVastra.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Data;

public partial class ArunVastraDbContext : DbContext
{
    public ArunVastraDbContext(DbContextOptions<ArunVastraDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Floor> Floors { get; set; }

    public virtual DbSet<SupItem> SupItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<SaleVoucher> SaleVouchers { get; set; }

    public virtual DbSet<SaleVoucherDetail> SaleVoucherDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<State>(entity =>
        {
            entity.ToTable("STATES");

            entity.Property(e => e.Stateid).HasColumnName("STATEID");
            entity.Property(e => e.Gststatecode).HasColumnName("GSTSTATECODE");
            entity.Property(e => e.Statename)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("STATENAME");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("CITIES");

            entity.Property(e => e.Cityid).HasColumnName("CITYID");
            entity.Property(e => e.Cityname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CITYNAME");
            entity.Property(e => e.Stateid).HasColumnName("STATEID");

            entity.HasOne(d => d.State).WithMany(p => p.Cities)
                .HasForeignKey(d => d.Stateid)
                .HasConstraintName("FK_CITIES_STATES");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USERS");

            entity.HasIndex(e => e.Email, "IDXEMAIL").IsUnique();

            entity.Property(e => e.Userid).HasColumnName("USERID");
            entity.Property(e => e.Agentid).HasColumnName("AGENTID");
            entity.Property(e => e.Agentname)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("AGENTNAME");
            entity.Property(e => e.Brandname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("BRANDNAME");
            entity.Property(e => e.Btuser).HasColumnName("BTUser");
            entity.Property(e => e.Cityid).HasColumnName("CITYID");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("smalldatetime")
                .HasColumnName("CREATED");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("EMAIL");
            entity.Property(e => e.Extracharges)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("EXTRACHARGES");
            entity.Property(e => e.Firstname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("FIRSTNAME");
            entity.Property(e => e.Gender).HasColumnName("GENDER");
            entity.Property(e => e.Gstin)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("GSTIN");
            entity.Property(e => e.Isgstupdate)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("N")
                .HasColumnName("ISGSTUPDATE");
            entity.Property(e => e.Lastloginat).HasColumnName("LASTLOGINAT");
            entity.Property(e => e.Lastname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("LASTNAME");
            entity.Property(e => e.Locked).HasColumnName("LOCKED");
            entity.Property(e => e.Mobile)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("MOBILE");
            entity.Property(e => e.Passwordhash)
                .HasMaxLength(1000)
                .HasColumnName("PASSWORDHASH");
            entity.Property(e => e.Passwordmigrated).HasColumnName("PASSWORDMIGRATED");
            entity.Property(e => e.Passwordresetrequired).HasColumnName("PASSWORDRESETREQUIRED");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("PHONE");
            entity.Property(e => e.Profit).HasColumnName("PROFIT");
            entity.Property(e => e.Pwhash)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("PWHASH");
            entity.Property(e => e.Role).HasColumnName("ROLE");
            entity.Property(e => e.Updatedat).HasColumnName("UPDATEDAT");
            entity.Property(e => e.Useraddress)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("USERADDRESS");
            entity.Property(e => e.Usercode)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("USERCODE");
        });

        modelBuilder.Entity<UserRefreshToken>(entity =>
        {
            entity.ToTable("USER_REFRESH_TOKENS");

            entity.HasIndex(e => new { e.Userid, e.Revokedat, e.Expiresat }, "IX_USER_REFRESH_TOKENS_USERID_REVOKEDAT_EXPIRESAT");

            entity.HasIndex(e => e.Tokenhash, "UX_USER_REFRESH_TOKENS_TOKENHASH").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("CREATEDAT");
            entity.Property(e => e.Expiresat).HasColumnName("EXPIRESAT");
            entity.Property(e => e.Revokedat).HasColumnName("REVOKEDAT");
            entity.Property(e => e.Tokenhash)
                .HasMaxLength(500)
                .HasColumnName("TOKENHASH");
            entity.Property(e => e.Userid).HasColumnName("USERID");

            entity.HasOne(d => d.User).WithMany(p => p.UserRefreshTokens)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_REFRESH_TOKENS_USERS");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("COMPANIES");

            entity.HasKey(e => e.Compid);

            entity.Property(e => e.Compid).HasColumnName("COMPID");
            entity.Property(e => e.Userid).HasColumnName("USERID");
            entity.Property(e => e.Cityid).HasColumnName("CITYID");
            entity.Property(e => e.Compname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("COMPNAME");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("ADDRESS");
            entity.Property(e => e.Pan)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PAN");
            entity.Property(e => e.Tin)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TIN");
            entity.Property(e => e.Gstin)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GSTIN");
            entity.Property(e => e.Isactive).HasColumnName("ISACTIVE");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Floor>(entity =>
        {
            entity.ToTable("FLOORS");

            entity.HasKey(e => e.Floorid);

            entity.Property(e => e.Floorid).HasColumnName("FLOORID");
            entity.Property(e => e.Floorname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("FLOORNAME");
            entity.Property(e => e.Status)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("STATUS");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("PRODUCTS");

            entity.HasKey(e => e.Prodid);

            entity.Property(e => e.Prodid).HasColumnName("PRODID");
            entity.Property(e => e.Prodname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("PRODNAME");
        });

        modelBuilder.Entity<SupItem>(entity =>
        {
            entity.ToTable("SUPITEMS");

            entity.HasKey(e => e.Supprodid);

            entity.Property(e => e.Supprodid).HasColumnName("SUPPRODID");
            entity.Property(e => e.Prodid).HasColumnName("PRODID");
            entity.Property(e => e.Userid).HasColumnName("USERID");
            entity.Property(e => e.Compid).HasColumnName("COMPID");
            entity.Property(e => e.Barcode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("BARCODE");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("DESCRIPTION");
            entity.Property(e => e.Purchase)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("PURCHASE");
            entity.Property(e => e.Mrpnew)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("MRPNEW");
            entity.Property(e => e.Isactive).HasColumnName("ISACTIVE");
            entity.Property(e => e.Hsncode)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("HSNCODE");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.Prodid)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SaleVoucher>(entity =>
        {
            entity.ToTable("SALEVOUCHERS");

            entity.HasKey(e => e.Svid);

            entity.HasIndex(e => e.Autobillno, "AUTOBILLNOIDX").IsUnique();

            entity.Property(e => e.Svid).HasColumnName("SVID");
            entity.Property(e => e.Autobillno).HasColumnName("AUTOBILLNO");
            entity.Property(e => e.Compid).HasColumnName("COMPID");
            entity.Property(e => e.Transid).HasColumnName("TRANSID");
            entity.Property(e => e.Date)
                .HasColumnType("smalldatetime")
                .HasColumnName("DATE");
            entity.Property(e => e.Challan)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CHALLAN");
            entity.Property(e => e.Profit).HasColumnName("PROFIT");
            entity.Property(e => e.Status).HasColumnName("STATUS");
            entity.Property(e => e.Floorid).HasColumnName("FloorID");
            entity.Property(e => e.Istsynched)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("ISTSYNCHED");

            entity.HasOne(d => d.Company).WithMany()
                .HasForeignKey(d => d.Compid)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Transport).WithMany()
                .HasForeignKey(d => d.Transid)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SaleVoucherDetail>(entity =>
        {
            entity.ToTable("SALEVOUCHERDETAILS");

            entity.HasKey(e => e.Svdetailid);

            entity.Property(e => e.Svdetailid).HasColumnName("SVDETAILID");
            entity.Property(e => e.Svid).HasColumnName("SVID");
            entity.Property(e => e.Supprodid).HasColumnName("SUPPRODID");
            entity.Property(e => e.Purchase)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("PURCHASE");
            entity.Property(e => e.Mrp)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("MRP");
            entity.Property(e => e.Qty).HasColumnName("QTY");

            entity.HasOne(d => d.SaleVoucher).WithMany(p => p.Details)
                .HasForeignKey(d => d.Svid);

            entity.HasOne(d => d.SupplierProduct).WithMany()
                .HasForeignKey(d => d.Supprodid);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
