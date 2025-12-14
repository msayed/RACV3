using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ORMContext.Migrations
{
    public partial class UpdateAtAndFlightDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FlightDate",
                table: "FlightHistories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateAt",
                table: "FlightHistories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlightDate",
                table: "FlightHistories");

            migrationBuilder.DropColumn(
                name: "UpdateAt",
                table: "FlightHistories");
        }
    }
}
