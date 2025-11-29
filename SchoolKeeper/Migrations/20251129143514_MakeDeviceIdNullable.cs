using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolKeeper.Migrations
{
    /// <inheritdoc />
    public partial class MakeDeviceIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incident_Device_device_id",
                table: "Incident");

            migrationBuilder.AlterColumn<int>(
                name: "device_id",
                table: "Incident",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Incident_Device_device_id",
                table: "Incident",
                column: "device_id",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incident_Device_device_id",
                table: "Incident");

            migrationBuilder.AlterColumn<int>(
                name: "device_id",
                table: "Incident",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Incident_Device_device_id",
                table: "Incident",
                column: "device_id",
                principalTable: "Device",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
