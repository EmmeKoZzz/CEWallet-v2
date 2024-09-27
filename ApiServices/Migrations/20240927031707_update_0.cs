using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApiServices.Migrations
{
    /// <inheritdoc />
    public partial class update_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("4a2cfbcc-dcd9-43f8-8414-b9e8a5b49385"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("853a4901-542c-4ee7-8cd0-7c2b0064ea00"));

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("e20c96e2-dac7-4eca-ab7a-aa533da10380"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("de9c8d25-8493-4a2b-ac24-a54c566cb76a"));

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "Role" },
                values: new object[,]
                {
                    { new Guid("0df9f77c-9698-46e9-a17e-6974b9a13b33"), "Administrador" },
                    { new Guid("0e89f422-615c-449e-b751-ebc85e3830d4"), "Asesor" },
                    { new Guid("c373c833-29ee-48d3-b75d-113a4decbe05"), "Supervisor" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Active", "CreatedAt", "Email", "PasswordHash", "PasswordSalt", "RoleId", "Username" },
                values: new object[] { new Guid("aab43ece-fe9a-40aa-9f2c-389c690de5ea"), true, new DateTime(2024, 9, 27, 3, 17, 6, 387, DateTimeKind.Utc).AddTicks(7574), "admin@cewallet.org", new byte[] { 138, 207, 85, 42, 198, 36, 169, 249, 176, 62, 20, 64, 97, 130, 98, 45, 110, 158, 191, 243, 129, 108, 219, 102, 21, 200, 90, 81, 251, 36, 192, 152 }, new byte[] { 9, 161, 62, 108, 128, 20, 44, 20, 60, 239, 95, 9, 179, 253, 73, 148 }, new Guid("0df9f77c-9698-46e9-a17e-6974b9a13b33"), "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("0e89f422-615c-449e-b751-ebc85e3830d4"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("c373c833-29ee-48d3-b75d-113a4decbe05"));

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("aab43ece-fe9a-40aa-9f2c-389c690de5ea"));

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: new Guid("0df9f77c-9698-46e9-a17e-6974b9a13b33"));

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "Id", "Role" },
                values: new object[,]
                {
                    { new Guid("4a2cfbcc-dcd9-43f8-8414-b9e8a5b49385"), "Supervisor" },
                    { new Guid("853a4901-542c-4ee7-8cd0-7c2b0064ea00"), "Asesor" },
                    { new Guid("de9c8d25-8493-4a2b-ac24-a54c566cb76a"), "Administrador" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Active", "CreatedAt", "Email", "PasswordHash", "PasswordSalt", "RoleId", "Username" },
                values: new object[] { new Guid("e20c96e2-dac7-4eca-ab7a-aa533da10380"), true, new DateTime(2024, 9, 26, 1, 40, 51, 762, DateTimeKind.Utc).AddTicks(85), "admin@cewallet.org", new byte[] { 55, 43, 241, 231, 50, 126, 150, 155, 200, 163, 206, 12, 58, 100, 44, 253, 83, 54, 247, 160, 218, 11, 28, 148, 3, 72, 174, 189, 239, 159, 4, 18 }, new byte[] { 141, 159, 219, 189, 24, 247, 197, 109, 186, 86, 78, 156, 51, 84, 88, 26 }, new Guid("de9c8d25-8493-4a2b-ac24-a54c566cb76a"), "admin" });
        }
    }
}
