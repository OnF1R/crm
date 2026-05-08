using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Tasks.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTaskRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_subtasks_tasks_TaskItemId",
                table: "subtasks");

            migrationBuilder.DropForeignKey(
                name: "FK_task_dependencies_tasks_TaskItemId",
                table: "task_dependencies");

            migrationBuilder.DropIndex(
                name: "IX_task_dependencies_TaskItemId",
                table: "task_dependencies");

            migrationBuilder.DropIndex(
                name: "IX_subtasks_TaskItemId",
                table: "subtasks");

            migrationBuilder.DropColumn(
                name: "TaskItemId",
                table: "task_dependencies");

            migrationBuilder.DropColumn(
                name: "TaskItemId",
                table: "subtasks");

            migrationBuilder.CreateIndex(
                name: "IX_subtasks_ParentTaskId",
                table: "subtasks",
                column: "ParentTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_subtasks_tasks_ParentTaskId",
                table: "subtasks",
                column: "ParentTaskId",
                principalTable: "tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_task_dependencies_tasks_PredecessorTaskId",
                table: "task_dependencies",
                column: "PredecessorTaskId",
                principalTable: "tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_task_dependencies_tasks_SuccessorTaskId",
                table: "task_dependencies",
                column: "SuccessorTaskId",
                principalTable: "tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_subtasks_tasks_ParentTaskId",
                table: "subtasks");

            migrationBuilder.DropForeignKey(
                name: "FK_task_dependencies_tasks_PredecessorTaskId",
                table: "task_dependencies");

            migrationBuilder.DropForeignKey(
                name: "FK_task_dependencies_tasks_SuccessorTaskId",
                table: "task_dependencies");

            migrationBuilder.DropIndex(
                name: "IX_subtasks_ParentTaskId",
                table: "subtasks");

            migrationBuilder.AddColumn<Guid>(
                name: "TaskItemId",
                table: "task_dependencies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TaskItemId",
                table: "subtasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_task_dependencies_TaskItemId",
                table: "task_dependencies",
                column: "TaskItemId");

            migrationBuilder.CreateIndex(
                name: "IX_subtasks_TaskItemId",
                table: "subtasks",
                column: "TaskItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_subtasks_tasks_TaskItemId",
                table: "subtasks",
                column: "TaskItemId",
                principalTable: "tasks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_task_dependencies_tasks_TaskItemId",
                table: "task_dependencies",
                column: "TaskItemId",
                principalTable: "tasks",
                principalColumn: "Id");
        }
    }
}
