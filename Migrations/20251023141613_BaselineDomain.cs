using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeanScene.Web.Migrations
{
    /// <inheritdoc />
    public partial class BaselineDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Area",
                columns: table => new
                {
                    AreaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AreaName = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Area__70B82028FEDCE2CC", x => x.AreaID);
                });

            migrationBuilder.CreateTable(
                name: "SittingSchedule",
                columns: table => new
                {
                    SittingScheduleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SType = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    SCapacity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SittingS__AE09170F976F16D2", x => x.SittingScheduleID);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantTable",
                columns: table => new
                {
                    RestaurantTableID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AreaID = table.Column<int>(type: "int", nullable: false),
                    TableName = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Seats = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Restaura__15D937BA28FD3CE3", x => x.RestaurantTableID);
                    table.ForeignKey(
                        name: "FK_Table_Area",
                        column: x => x.AreaID,
                        principalTable: "Area",
                        principalColumn: "AreaID");
                });

            migrationBuilder.CreateTable(
                name: "Reservation",
                columns: table => new
                {
                    ReservationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SittingID = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    StartTime = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    NumOfGuests = table.Column<int>(type: "int", nullable: false),
                    ReservationSource = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Reservat__B7EE5F0404326B6B", x => x.ReservationID);
                    table.ForeignKey(
                        name: "FK_Reservation_Sitting",
                        column: x => x.SittingID,
                        principalTable: "SittingSchedule",
                        principalColumn: "SittingScheduleID");
                });

            migrationBuilder.CreateTable(
                name: "ReservationTable",
                columns: table => new
                {
                    ReservationTableID = table.Column<int>(type: "int", nullable: false),
                    RestaurantTableID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationTable", x => new { x.ReservationTableID, x.RestaurantTableID });
                    table.ForeignKey(
                        name: "FK_ReservationTable_Reservation",
                        column: x => x.ReservationTableID,
                        principalTable: "Reservation",
                        principalColumn: "ReservationID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReservationTable_Table",
                        column: x => x.RestaurantTableID,
                        principalTable: "RestaurantTable",
                        principalColumn: "RestaurantTableID");
                });

            migrationBuilder.CreateIndex(
                name: "UQ__Area__8EB6AF57AFF687E8",
                table: "Area",
                column: "AreaName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_Sitting_Start",
                table: "Reservation",
                columns: new[] { "SittingID", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_Status",
                table: "Reservation",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationTable_Table",
                table: "ReservationTable",
                column: "RestaurantTableID");

            migrationBuilder.CreateIndex(
                name: "UQ_Area_Table",
                table: "RestaurantTable",
                columns: new[] { "AreaID", "TableName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Sitting_Window",
                table: "SittingSchedule",
                columns: new[] { "SType", "StartDateTime", "EndDateTime" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservationTable");

            migrationBuilder.DropTable(
                name: "Reservation");

            migrationBuilder.DropTable(
                name: "RestaurantTable");

            migrationBuilder.DropTable(
                name: "SittingSchedule");

            migrationBuilder.DropTable(
                name: "Area");
        }
    }
}
