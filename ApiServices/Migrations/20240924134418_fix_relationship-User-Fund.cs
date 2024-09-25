using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApiServices.Migrations
{
    /// <inheritdoc />
    public partial class fix_relationshipUserFund : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Fund_UserId",
                table: "Fund");

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
                    { new Guid("192587e5-6ff3-4dff-b91a-9b455e5a3496"), "Administrador" },
                    { new Guid("8ea8915b-1a5c-453f-88f1-7f19332bf2a3"), "Supervisor" },
                    { new Guid("d802f83c-c2d9-49a6-bbd1-23432ae65dd2"), "Asesor" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Active", "CreatedAt", "Email", "PasswordHash", "PasswordSalt", "RoleId", "Username" },
                values: new object[] { new Guid("344ea41f-7f4a-463a-84fa-772dcd80691f"), true, new DateTime(2024, 9, 24, 13, 44, 17, 526, DateTimeKind.Utc).AddTicks(1718), "admin@cewallet.org", new byte[] { 162, 81, 34, 221, 92, 206, 79, 167, 134, 115, 83, 194, 66, 139, 164, 187, 255, 204, 162, 102, 88, 109, 102, 218, 244, 84, 55, 196, 202, 92, 76, 57 }, new byte[] { 96, 195, 183, 47, 11, 77, 20, 94, 173, 98, 24, 57, 199, 251, 203, 232 }, new Guid("192587e5-6ff3-4dff-b91a-9b455e5a3496"), "admin" });

            migrationBuilder.CreateIndex(
                name: "FundId1",
                table: "Fund_Currency",
                column: "FundId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "FundId1",
                table: "Fund_Currency");

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("8ea8915b-1a5c-453f-88f1-7f19332bf2a3"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("d802f83c-c2d9-49a6-bbd1-23432ae65dd2"));

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("344ea41f-7f4a-463a-84fa-772dcd80691f"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("192587e5-6ff3-4dff-b91a-9b455e5a3496"));

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

            migrationBuilder.CreateIndex(
                name: "IX_Fund_UserId",
                table: "Fund",
                column: "UserId",
                unique: true);
        }
    }
}
