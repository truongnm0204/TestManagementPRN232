using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddClassCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Classes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_CreatedBy",
                table: "Classes",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Users_CreatedBy",
                table: "Classes",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Users_CreatedBy",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_CreatedBy",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Classes");
        }
    }
}
