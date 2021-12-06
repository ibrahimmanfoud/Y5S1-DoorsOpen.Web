using Microsoft.EntityFrameworkCore.Migrations;

namespace DoorsOpen.Migrations
{
    public partial class GenderNeutral : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GenderNeutralRestroomsAvailable",
                table: "Buildings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GenderNeutralRestroomsAvailable",
                table: "Buildings");
        }
    }
}
