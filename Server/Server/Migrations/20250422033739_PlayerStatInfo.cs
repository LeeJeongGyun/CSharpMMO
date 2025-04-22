using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class PlayerStatInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatInfo_Attack",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatInfo_Hp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatInfo_Level",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatInfo_MaxHp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "StatInfo_Speed",
                table: "Player",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "StatInfo_TotalExp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatInfo_Attack",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "StatInfo_Hp",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "StatInfo_Level",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "StatInfo_MaxHp",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "StatInfo_Speed",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "StatInfo_TotalExp",
                table: "Player");
        }
    }
}
