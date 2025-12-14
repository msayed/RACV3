using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ORMContext.Migrations
{
    public partial class BookedSeatsCol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookedSeats",
                table: "FlightHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookedSeats",
                table: "FlightHistories");
        }
    }
}
