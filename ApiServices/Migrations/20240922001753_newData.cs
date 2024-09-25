using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApiServices.Migrations
{
    /// <inheritdoc />
    public partial class newData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_Fund_UserId",
                table: "Fund",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Fund_User_UserId",
                table: "Fund",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fund_User_UserId",
                table: "Fund");

            migrationBuilder.DropIndex(
                name: "IX_Fund_UserId",
                table: "Fund");

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
                    { new Guid("5c4fba10-bab8-4cae-9dca-7b1985327683"), "Supervisor" },
                    { new Guid("a8a01ea7-e035-4920-805f-b4f02cd5eb57"), "Administrador" },
                    { new Guid("b120b50d-4f4e-49c6-99a1-ba9821e45468"), "Asesor" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Active", "CreatedAt", "Email", "PasswordHash", "PasswordSalt", "RoleId", "Username" },
                values: new object[] { new Guid("45fb5a70-3a85-4cae-9546-48160ddc58ac"), true, new DateTime(2024, 9, 19, 22, 9, 47, 543, DateTimeKind.Utc).AddTicks(5134), "admin@cewallet.org", new byte[] { 25, 52, 68, 65, 94, 102, 7, 37, 149, 44, 66, 16, 53, 67, 251, 128, 10, 125, 95, 216, 249, 153, 139, 48, 128, 82, 108, 198, 238, 226, 96, 88 }, new byte[] { 78, 237, 238, 4, 51, 117, 148, 174, 138, 72, 90, 123, 3, 201, 102, 85 }, new Guid("a8a01ea7-e035-4920-805f-b4f02cd5eb57"), "admin" });
        }
    }
}
