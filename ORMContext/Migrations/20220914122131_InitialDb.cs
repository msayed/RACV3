using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ORMContext.Migrations
{
    public partial class InitialDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    OriginDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlightNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartureAirport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArrivalAirport = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlightHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InternationalStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlightNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlightSuffix = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartureAirport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArrivalAirport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduledDeparture = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstimatedDeparture = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledArrival = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstimatedArrival = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AirlineCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AirlineICAOCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OffBlock = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OnBlock = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AircraftType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TakeOff = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TouchDown = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CodeShareInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstBag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastBag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfPassengers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AircraftSubType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Registration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CallSign = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FleetIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DayOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlightType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlighStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdultCount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChildCount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CrewCount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalPlannedPaxCount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransitPaxCount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocalPaxCount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransferPaxCount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeatCapacity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TakeOffFromOutStation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActualStartBoaringTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CalculatedTakeOffTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastActionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActionCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientUniqueKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CargoDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgentInfo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlightId = table.Column<long>(type: "bigint", nullable: false),
                    Sent = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightHistories_Flights_FlightId",
                        column: x => x.FlightId,
                        principalTable: "Flights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlightHistories_FlightId",
                table: "FlightHistories",
                column: "FlightId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlightHistories");

            migrationBuilder.DropTable(
                name: "Flights");
        }
    }
}
