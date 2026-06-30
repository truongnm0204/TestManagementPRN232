using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeacherClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherClasses_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherClasses_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherClasses_ClassId",
                table: "TeacherClasses",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherClasses_TeacherId_ClassId",
                table: "TeacherClasses",
                columns: new[] { "TeacherId", "ClassId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherClasses");
        }
    }
}
