using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotaAnalytics.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchPlayersNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeroName",
                table: "match_player_stats");

            migrationBuilder.AddColumn<int>(
                name: "Hero",
                table: "match_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_match_player_stats_matches_MatchId",
                table: "match_player_stats",
                column: "MatchId",
                principalTable: "matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_match_player_stats_matches_MatchId",
                table: "match_player_stats");

            migrationBuilder.DropColumn(
                name: "Hero",
                table: "match_player_stats");

            migrationBuilder.AddColumn<string>(
                name: "HeroName",
                table: "match_player_stats",
                type: "text",
                nullable: true);
        }
    }
}
