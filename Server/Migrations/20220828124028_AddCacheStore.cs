using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHomeWWW.Migrations
{
    public partial class AddCacheStore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CacheEntries",
                columns: table => new {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CacheEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CacheEntries_ExpireAt",
                table: "CacheEntries",
                column: "ExpireAt");

            migrationBuilder.CreateIndex(
                name: "IX_CacheEntries_Key",
                table: "CacheEntries",
                column: "Key",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CacheEntries");
        }
    }
}
