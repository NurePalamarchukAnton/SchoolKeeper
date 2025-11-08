using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolKeeper.Migrations
{
    /// <inheritdoc />
    public partial class chipichipichapachapa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "User",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "School",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Rept",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Incident",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Device",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserIncident",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ReptIncident",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserIncident");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ReptIncident");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "User",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "School",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Rept",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Incident",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Device",
                newName: "id");
        }
    }
}
