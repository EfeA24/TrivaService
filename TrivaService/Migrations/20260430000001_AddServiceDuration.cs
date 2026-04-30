using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrivaService.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceDurationDays",
                table: "Services",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceDurationHours",
                table: "Services",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceDurationDays",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ServiceDurationHours",
                table: "Services");
        }
    }
}
