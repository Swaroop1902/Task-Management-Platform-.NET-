using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.UserService.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("INSERT INTO Users (Id, Username, PasswordHash, Role) VALUES (1, 'admin', '$2a$11$ghvKM208pBmjdIBrJnXlGuiSNzwRpPwHPOARhYSd.m9GS2zmoPDb.', 'Admin') ON DUPLICATE KEY UPDATE Username=VALUES(Username), PasswordHash=VALUES(PasswordHash), Role=VALUES(Role);");
            migrationBuilder.Sql("INSERT INTO Users (Id, Username, PasswordHash, Role) VALUES (2, 'manager', '$2a$11$ghvKM208pBmjdIBrJnXlGuiSNzwRpPwHPOARhYSd.m9GS2zmoPDb.', 'Manager') ON DUPLICATE KEY UPDATE Username=VALUES(Username), PasswordHash=VALUES(PasswordHash), Role=VALUES(Role);");
            migrationBuilder.Sql("INSERT INTO Users (Id, Username, PasswordHash, Role) VALUES (3, 'engineer', '$2a$11$ghvKM208pBmjdIBrJnXlGuiSNzwRpPwHPOARhYSd.m9GS2zmoPDb.', 'Engineer') ON DUPLICATE KEY UPDATE Username=VALUES(Username), PasswordHash=VALUES(PasswordHash), Role=VALUES(Role);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
