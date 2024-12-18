using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Schoolvoetbalapi.Migrations
{
    /// <inheritdoc />
    public partial class AutoIncrement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Tourneys_TourneyId",
                table: "Matches");

            migrationBuilder.AlterColumn<int>(
                name: "TourneyId",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Tourneys_TourneyId",
                table: "Matches",
                column: "TourneyId",
                principalTable: "Tourneys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Tourneys_TourneyId",
                table: "Matches");

            migrationBuilder.AlterColumn<int>(
                name: "TourneyId",
                table: "Matches",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Tourneys_TourneyId",
                table: "Matches",
                column: "TourneyId",
                principalTable: "Tourneys",
                principalColumn: "Id");
        }
    }
}
