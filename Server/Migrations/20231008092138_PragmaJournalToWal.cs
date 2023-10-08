using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHomeWWW.Migrations
{
    /// <inheritdoc />
    public partial class PragmaJournalToWal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("PRAGMA journal_mode = 'wal'", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("PRAGMA journal_mode = 'delete'", true);
        }
    }
}
