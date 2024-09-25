using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApiServices.Migrations
{
    /// <inheritdoc />
    public partial class add_admin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "CreatedAt",
                table: "Fund",
                type: "timestamp",
                nullable: false,
                defaultValueSql: "current_timestamp()",
                oldClrType: typeof(DateTime),
                oldType: "TIMESTAMP",
                oldDefaultValueSql: "current_timestamp()");

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "Role" },
                values: new object[,]
                {
                    { new Guid("5c4fba10-bab8-4cae-9dca-7b1985327683"), "Supervisor" },
                    { new Guid("a8a01ea7-e035-4920-805f-b4f02cd5eb57"), "Administrador" },
                    { new Guid("b120b50d-4f4e-49c6-99a1-ba9821e45468"), "Asesor" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Active", "CreatedAt", "Email", "PasswordHash", "PasswordSalt", "RoleId", "Username" },
                values: new object[] { new Guid("45fb5a70-3a85-4cae-9546-48160ddc58ac"), true, new DateTime(2024, 9, 19, 22, 9, 47, 543, DateTimeKind.Utc).AddTicks(5134), "admin@cewallet.org", new byte[] { 25, 52, 68, 65, 94, 102, 7, 37, 149, 44, 66, 16, 53, 67, 251, 128, 10, 125, 95, 216, 249, 153, 139, 48, 128, 82, 108, 198, 238, 226, 96, 88 }, new byte[] { 78, 237, 238, 4, 51, 117, 148, 174, 138, 72, 90, 123, 3, 201, 102, 85 }, new Guid("a8a01ea7-e035-4920-805f-b4f02cd5eb57"), "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("5c4fba10-bab8-4cae-9dca-7b1985327683"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("b120b50d-4f4e-49c6-99a1-ba9821e45468"));

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("45fb5a70-3a85-4cae-9546-48160ddc58ac"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("a8a01ea7-e035-4920-805f-b4f02cd5eb57"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Fund",
                type: "TIMESTAMP",
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
                    { new Guid("3c80e6d9-7b23-4fc7-8a79-8aae486e2b19"), "Supervisor" },
                    { new Guid("eb84ea74-79fd-49a8-b0bf-85ea5625c994"), "Administrador" },
                    { new Guid("ee4ca322-db32-46fd-9ee9-9ed2f13c14cb"), "Asesor" }
                });
        }
    }
}
