using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoboChemist.TemplateService.Model.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "object_keey",
                table: "templates",
                newName: "object_key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "object_key",
                table: "templates",
                newName: "object_keey");
        }
    }
}
