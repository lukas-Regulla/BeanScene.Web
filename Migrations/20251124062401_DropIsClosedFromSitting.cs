using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeanScene.Web.Migrations
{
    public partial class DropIsClosedFromSitting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FK so we can modify table
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationTable_Table",
                table: "ReservationTable");

            // Remove IsClosed column
            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "SittingSchedule");

            // Re-add the foreign key cleanly
            migrationBuilder.AddForeignKey(
                name: "FK_ReservationTable_Table",
                table: "ReservationTable",
                column: "RestaurantTableID",
                principalTable: "RestaurantTable",
                principalColumn: "RestaurantTableID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationTable_Table",
                table: "ReservationTable");

            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "SittingSchedule",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationTable_Table",
                table: "ReservationTable",
                column: "RestaurantTableID",
                principalTable: "RestaurantTable",
                principalColumn: "RestaurantTableID");
        }
    }
}
