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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
