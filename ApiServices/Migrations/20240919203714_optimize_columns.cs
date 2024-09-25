using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApiServices.Migrations
{
    /// <inheritdoc />
    public partial class optimize_columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("13e1da55-0c8e-4794-9c6d-3c6bce4eb07f"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("2d990053-57a7-4777-9ff0-6a117e99369f"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("e7f44d52-fc3f-416f-aad7-2d0e2c969b61"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "current_timestamp()",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "current_timestamp()")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "PasswordSalt",
                table: "User",
                type: "tinyblob",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob");

            migrationBuilder.AlterColumn<byte[]>(
                name: "PasswordHash",
                table: "User",
                type: "tinyblob",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "longblob");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "current_timestamp()",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "current_timestamp()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Fund",
                type: "TIMESTAMP",
                nullable: false,
                defaultValueSql: "current_timestamp()",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "current_timestamp()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ActivityLog",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "current_timestamp()",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "current_timestamp()");

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "Role" },
                values: new object[,]
                {
                    { new Guid("3c80e6d9-7b23-4fc7-8a79-8aae486e2b19"), "Supervisor" },
                    { new Guid("eb84ea74-79fd-49a8-b0bf-85ea5625c994"), "Administrador" },
                    { new Guid("ee4ca322-db32-46fd-9ee9-9ed2f13c14cb"), "Asesor" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("3c80e6d9-7b23-4fc7-8a79-8aae486e2b19"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("eb84ea74-79fd-49a8-b0bf-85ea5625c994"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("ee4ca322-db32-46fd-9ee9-9ed2f13c14cb"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "User",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "current_timestamp()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValueSql: "current_timestamp()")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

            migrationBuilder.AlterColumn<byte[]>(
                name: "PasswordSalt",
                table: "User",
                type: "longblob",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "tinyblob");

            migrationBuilder.AlterColumn<byte[]>(
                name: "PasswordHash",
                table: "User",
                type: "longblob",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "tinyblob");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "User",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "current_timestamp()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValueSql: "current_timestamp()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Fund",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "current_timestamp()",
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP",
                oldDefaultValueSql: "current_timestamp()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ActivityLog",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "current_timestamp()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValueSql: "current_timestamp()");

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "Role" },
                values: new object[,]
                {
                    { new Guid("13e1da55-0c8e-4794-9c6d-3c6bce4eb07f"), "Administrador" },
                    { new Guid("2d990053-57a7-4777-9ff0-6a117e99369f"), "Asesor" },
                    { new Guid("e7f44d52-fc3f-416f-aad7-2d0e2c969b61"), "Supervisor" }
                });
        }
    }
}
