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
    [Migration("20240926014052_update_currency_addActiveColumn")]
    partial class update_currency_addActiveColumn
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
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

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

                    b.Property<bool>("Active")
                        .HasColumnType("tinyint(1)");

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
                        .HasColumnType("timestamp")
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

                    b.HasIndex(new[] { "FundId" }, "FundId")
                        .HasDatabaseName("FundId1");

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
                            Id = new Guid("853a4901-542c-4ee7-8cd0-7c2b0064ea00"),
                            Name = "Asesor"
                        },
                        new
                        {
                            Id = new Guid("4a2cfbcc-dcd9-43f8-8414-b9e8a5b49385"),
                            Name = "Supervisor"
                        },
                        new
                        {
                            Id = new Guid("de9c8d25-8493-4a2b-ac24-a54c566cb76a"),
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

                    b.HasData(
                        new
                        {
                            Id = new Guid("e20c96e2-dac7-4eca-ab7a-aa533da10380"),
                            Active = true,
                            CreatedAt = new DateTime(2024, 9, 26, 1, 40, 51, 762, DateTimeKind.Utc).AddTicks(85),
                            Email = "admin@cewallet.org",
                            PasswordHash = new byte[] { 55, 43, 241, 231, 50, 126, 150, 155, 200, 163, 206, 12, 58, 100, 44, 253, 83, 54, 247, 160, 218, 11, 28, 148, 3, 72, 174, 189, 239, 159, 4, 18 },
                            PasswordSalt = new byte[] { 141, 159, 219, 189, 24, 247, 197, 109, 186, 86, 78, 156, 51, 84, 88, 26 },
                            RoleId = new Guid("de9c8d25-8493-4a2b-ac24-a54c566cb76a"),
                            UpdatedAt = new DateTime(2024, 9, 26, 1, 40, 51, 762, DateTimeKind.Utc).AddTicks(84),
                            Username = "admin"
                        });
                });

            modelBuilder.Entity("ApiServices.Models.Fund", b =>
                {
                    b.HasOne("ApiServices.Models.User", "User")
                        .WithMany("Funds")
                        .HasForeignKey("UserId");

                    b.Navigation("User");
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

            modelBuilder.Entity("ApiServices.Models.User", b =>
                {
                    b.Navigation("Funds");
                });
#pragma warning restore 612, 618
        }
    }
}
