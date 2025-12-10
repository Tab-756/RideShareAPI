using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RideShareAPI.Migrations
{
    /// <inheritdoc />
    public partial class RideRequestPickup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Dropoff",
                table: "RideRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Pickup",
                table: "RideRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dropoff",
                table: "RideRequests");

            migrationBuilder.DropColumn(
                name: "Pickup",
                table: "RideRequests");
        }
    }
}
