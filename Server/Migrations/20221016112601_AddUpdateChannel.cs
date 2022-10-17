using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHomeWWW.Migrations
{
    public partial class AddUpdateChannel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UpdateChannel",
                table: "Sensors",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateChannel",
                table: "Sensors");
        }
    }
}
