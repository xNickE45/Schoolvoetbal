using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schoolvoetbalapi.Migrations
{
    /// <inheritdoc />
    public partial class NewModels2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TourneyId",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tourneys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tourneys", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_TourneyId",
                table: "Matches",
                column: "TourneyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Tourneys_TourneyId",
                table: "Matches",
                column: "TourneyId",
                principalTable: "Tourneys",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Tourneys_TourneyId",
                table: "Matches");

            migrationBuilder.DropTable(
                name: "Tourneys");

            migrationBuilder.DropIndex(
                name: "IX_Matches_TourneyId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TourneyId",
                table: "Matches");
        }
    }
}
