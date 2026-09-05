using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DotaAnalytics.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLaneEfficiencyToDouble : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "LaneEfficiency",
                table: "match_player_stats",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "LaneEfficiency",
                table: "match_player_stats",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}
