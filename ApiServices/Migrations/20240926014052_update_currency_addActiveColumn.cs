using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ApiServices.Migrations
{
    /// <inheritdoc />
    public partial class update_currency_addActiveColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Currency",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Activity",
                table: "ActivityLog",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                collation: "utf8mb4_bin",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_bin");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "Active",
                table: "Currency");

            migrationBuilder.AlterColumn<string>(
                name: "Activity",
                table: "ActivityLog",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                collation: "utf8mb4_bin",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_bin");

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
        }
    }
}
