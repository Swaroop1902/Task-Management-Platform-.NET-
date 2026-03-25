using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagement.TaskService.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssigneeId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    StatusChangedTo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_TaskId",
                table: "ActivityLogs",
                column: "TaskId");

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "AssigneeId", "CreatedAt", "Description", "DueDate", "Priority", "Status", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 3, new DateTime(2024, 3, 24, 10, 0, 0, 0, DateTimeKind.Utc), "Setup microservices", new DateTime(2024, 3, 27, 10, 0, 0, 0, DateTimeKind.Utc), "High", "In Progress", "Initial Task", new DateTime(2024, 3, 25, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 1, new DateTime(2024, 3, 20, 10, 0, 0, 0, DateTimeKind.Utc), "This task should trigger an SLA breach", new DateTime(2024, 3, 24, 10, 0, 0, 0, DateTimeKind.Utc), "Medium", "Open", "Overdue Task", new DateTime(2024, 3, 20, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 2, new DateTime(2024, 3, 23, 10, 0, 0, 0, DateTimeKind.Utc), "Build dashboard using Angular", new DateTime(2024, 3, 30, 10, 0, 0, 0, DateTimeKind.Utc), "High", "Blocked", "UI Implementation", new DateTime(2024, 3, 25, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 3, new DateTime(2024, 3, 15, 10, 0, 0, 0, DateTimeKind.Utc), "Write internal API documentation", new DateTime(2024, 3, 20, 10, 0, 0, 0, DateTimeKind.Utc), "Low", "Completed", "Documentation", new DateTime(2024, 3, 23, 10, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "ActivityLogs",
                columns: new[] { "Id", "ChangedByUserId", "StatusChangedTo", "TaskId", "Timestamp" },
                values: new object[,]
                {
                    { 1, 3, "In Progress", 1, new DateTime(2024, 3, 24, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 2, "Open", 3, new DateTime(2024, 3, 23, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 1, "Blocked", 3, new DateTime(2024, 3, 25, 12, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "Tasks");
        }
    }
}
