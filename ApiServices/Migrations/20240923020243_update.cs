using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApiServices.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("df6177fc-a8a1-444e-a752-aebc606d108f"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("f2160f45-5e08-42fc-8ed0-6dc8d189a1cc"));

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("f2c90d6e-a4e1-458f-be40-d8d1056fb2cd"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("1d39a667-1f7a-4e12-bad3-f83a52149189"));

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "Role" },
                values: new object[,]
                {
                    { new Guid("078209c7-b568-4e9f-9a98-edc0495c7ed6"), "Administrador" },
                    { new Guid("2b17c8c3-7eb1-4816-9e67-83c99becf193"), "Asesor" },
                    { new Guid("d1325d30-2af4-4135-b0ac-1a7a56205c26"), "Supervisor" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Active", "CreatedAt", "Email", "PasswordHash", "PasswordSalt", "RoleId", "Username" },
                values: new object[] { new Guid("bc7f232c-b2dc-4738-8dba-b15e55c8b409"), true, new DateTime(2024, 9, 23, 2, 2, 42, 901, DateTimeKind.Utc).AddTicks(2375), "admin@cewallet.org", new byte[] { 110, 165, 198, 12, 171, 207, 237, 147, 85, 255, 137, 44, 215, 191, 80, 52, 167, 216, 61, 144, 205, 85, 73, 236, 248, 139, 39, 158, 66, 116, 176, 86 }, new byte[] { 35, 198, 78, 197, 191, 32, 225, 242, 60, 131, 19, 149, 77, 19, 227, 138 }, new Guid("078209c7-b568-4e9f-9a98-edc0495c7ed6"), "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("2b17c8c3-7eb1-4816-9e67-83c99becf193"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("d1325d30-2af4-4135-b0ac-1a7a56205c26"));

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("bc7f232c-b2dc-4738-8dba-b15e55c8b409"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("078209c7-b568-4e9f-9a98-edc0495c7ed6"));

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "Role" },
                values: new object[,]
                {
                    { new Guid("1d39a667-1f7a-4e12-bad3-f83a52149189"), "Administrador" },
                    { new Guid("df6177fc-a8a1-444e-a752-aebc606d108f"), "Supervisor" },
                    { new Guid("f2160f45-5e08-42fc-8ed0-6dc8d189a1cc"), "Asesor" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Active", "CreatedAt", "Email", "PasswordHash", "PasswordSalt", "RoleId", "Username" },
                values: new object[] { new Guid("f2c90d6e-a4e1-458f-be40-d8d1056fb2cd"), true, new DateTime(2024, 9, 22, 0, 17, 52, 309, DateTimeKind.Utc).AddTicks(3658), "admin@cewallet.org", new byte[] { 35, 176, 167, 36, 148, 157, 224, 225, 217, 171, 197, 90, 59, 78, 48, 88, 62, 116, 114, 231, 39, 224, 152, 127, 14, 235, 239, 248, 122, 101, 203, 192 }, new byte[] { 91, 18, 22, 1, 254, 24, 127, 223, 183, 156, 246, 248, 99, 50, 194, 240 }, new Guid("1d39a667-1f7a-4e12-bad3-f83a52149189"), "admin" });
        }
    }
}
