using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class OneTeacherPerClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeacherClasses_ClassId",
                table: "TeacherClasses");

            migrationBuilder.DropIndex(
                name: "IX_TeacherClasses_TeacherId_ClassId",
                table: "TeacherClasses");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherClasses_ClassId",
                table: "TeacherClasses",
                column: "ClassId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherClasses_TeacherId",
                table: "TeacherClasses",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeacherClasses_ClassId",
                table: "TeacherClasses");

            migrationBuilder.DropIndex(
                name: "IX_TeacherClasses_TeacherId",
                table: "TeacherClasses");

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
    }
}
