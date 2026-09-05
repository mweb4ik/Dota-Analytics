using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotaAnalytics.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingPlayerStatsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Denies",
                table: "match_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HeroDamage",
                table: "match_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HeroHealing",
                table: "match_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LaneEfficiency",
                table: "match_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "match_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NetWorth",
                table: "match_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TowerDamage",
                table: "match_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "XpPerMin",
                table: "match_player_stats",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Denies",
                table: "match_player_stats");

            migrationBuilder.DropColumn(
                name: "HeroDamage",
                table: "match_player_stats");

            migrationBuilder.DropColumn(
                name: "HeroHealing",
                table: "match_player_stats");

            migrationBuilder.DropColumn(
                name: "LaneEfficiency",
                table: "match_player_stats");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "match_player_stats");

            migrationBuilder.DropColumn(
                name: "NetWorth",
                table: "match_player_stats");

            migrationBuilder.DropColumn(
                name: "TowerDamage",
                table: "match_player_stats");

            migrationBuilder.DropColumn(
                name: "XpPerMin",
                table: "match_player_stats");
        }
    }
}
