using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schoolvoetbalapi.Migrations
{
    /// <inheritdoc />
    public partial class MatchTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateIndex(
            //    name: "IX_Matches_Team1Id",
            //    table: "Matches",
            //    column: "Team1Id");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Matches_Team2Id",
            //    table: "Matches",
            //    column: "Team2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Teams_Team1Id",
                table: "Matches",
                column: "Team1Id",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Teams_Team2Id",
                table: "Matches",
                column: "Team2Id",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Teams_Team1Id",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Teams_Team2Id",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_Team1Id",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_Team2Id",
                table: "Matches");
        }
    }
}
