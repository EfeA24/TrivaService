using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrivaService.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceIsPaymentComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPaymentComplete",
                table: "Services",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                "UPDATE [Services] SET [IsPaymentComplete] = 1 WHERE [Status] = N'Completed'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaymentComplete",
                table: "Services");
        }
    }
}
