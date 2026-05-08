using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Tasks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskDependencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "task_dependencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PredecessorTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    SuccessorTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TaskItemId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_dependencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_dependencies_tasks_TaskItemId",
                        column: x => x.TaskItemId,
                        principalTable: "tasks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_task_dependencies_PredecessorTaskId",
                table: "task_dependencies",
                column: "PredecessorTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_task_dependencies_SuccessorTaskId",
                table: "task_dependencies",
                column: "SuccessorTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_task_dependencies_TaskItemId",
                table: "task_dependencies",
                column: "TaskItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task_dependencies");
        }
    }
}
