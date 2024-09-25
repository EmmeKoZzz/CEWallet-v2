﻿// <auto-generated />
using System;
using ApiServices.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ApiServices.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240919203714_optimize_columns")]
    partial class optimize_columns
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("utf8mb4_bin")
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.HasCharSet(modelBuilder, "utf8mb4");
            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("ApiServices.Models.ActivityLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Activity")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<double>("Amount")
                        .HasColumnType("double");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp")
                        .HasDefaultValueSql("current_timestamp()");

                    b.Property<Guid?>("CurrencyId")
                        .HasColumnType("char(36)");

                    b.Property<string>("Details")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<Guid>("FundId")
                        .HasColumnType("char(36)");

                    b.Property<string>("TransactionType")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "CurrencyId" }, "CurrencyId");

                    b.HasIndex(new[] { "FundId" }, "FundId");

                    b.HasIndex(new[] { "UserId" }, "UserId");

                    b.ToTable("ActivityLog");
                });

            modelBuilder.Entity("ApiServices.Models.Currency", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("ValueUrl")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "Name" }, "Name")
                        .IsUnique();

                    b.ToTable("Currency");
                });

            modelBuilder.Entity("ApiServices.Models.Fund", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<bool>("Active")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TIMESTAMP")
                        .HasDefaultValueSql("current_timestamp()");

                    b.Property<string>("LocationUrl")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "UserId" }, "UserId")
                        .HasDatabaseName("UserId1");

                    b.ToTable("Fund");
                });

            modelBuilder.Entity("ApiServices.Models.FundCurrency", b =>
                {
                    b.Property<Guid>("FundId")
                        .HasColumnType("char(36)");

                    b.Property<Guid>("CurrencyId")
                        .HasColumnType("char(36)");

                    b.Property<double>("Amount")
                        .HasColumnType("double");

                    b.HasKey("FundId", "CurrencyId")
                        .HasName("PRIMARY")
                        .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                    b.HasIndex(new[] { "CurrencyId" }, "CurrencyId")
                        .HasDatabaseName("CurrencyId1");

                    b.ToTable("Fund_Currency");
                });

            modelBuilder.Entity("ApiServices.Models.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)")
                        .HasColumnName("Role");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "Name" }, "Name")
                        .IsUnique()
                        .HasDatabaseName("Name1");

                    b.ToTable("Role");

                    b.HasData(
                        new
                        {
                            Id = new Guid("ee4ca322-db32-46fd-9ee9-9ed2f13c14cb"),
                            Name = "Asesor"
                        },
                        new
                        {
                            Id = new Guid("3c80e6d9-7b23-4fc7-8a79-8aae486e2b19"),
                            Name = "Supervisor"
                        },
                        new
                        {
                            Id = new Guid("eb84ea74-79fd-49a8-b0bf-85ea5625c994"),
                            Name = "Administrador"
                        });
                });

            modelBuilder.Entity("ApiServices.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<bool>("Active")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp")
                        .HasDefaultValueSql("current_timestamp()");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("tinyblob");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("tinyblob");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("UpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp")
                        .HasDefaultValueSql("current_timestamp()");

                    MySqlPropertyBuilderExtensions.UseMySqlComputedColumn(b.Property<DateTime>("UpdatedAt"));

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "Email" }, "Email")
                        .IsUnique();

                    b.HasIndex(new[] { "RoleId" }, "RoleId");

                    b.HasIndex(new[] { "Username" }, "Username")
                        .IsUnique();

                    b.ToTable("User");
                });

            modelBuilder.Entity("ApiServices.Models.FundCurrency", b =>
                {
                    b.HasOne("ApiServices.Models.Currency", "Currency")
                        .WithMany("FundCurrencies")
                        .HasForeignKey("CurrencyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("Fund_Currency_ibfk_2");

                    b.HasOne("ApiServices.Models.Fund", "Fund")
                        .WithMany("FundCurrencies")
                        .HasForeignKey("FundId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("Fund_Currency_ibfk_1");

                    b.Navigation("Currency");

                    b.Navigation("Fund");
                });

            modelBuilder.Entity("ApiServices.Models.User", b =>
                {
                    b.HasOne("ApiServices.Models.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("User_ibfk_1");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("ApiServices.Models.Currency", b =>
                {
                    b.Navigation("FundCurrencies");
                });

            modelBuilder.Entity("ApiServices.Models.Fund", b =>
                {
                    b.Navigation("FundCurrencies");
                });

            modelBuilder.Entity("ApiServices.Models.Role", b =>
                {
                    b.Navigation("Users");
                });
#pragma warning restore 612, 618
        }
    }
}