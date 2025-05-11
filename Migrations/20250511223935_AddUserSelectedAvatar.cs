using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DebateRoyale.Migrations
{
    public partial class AddUserSelectedAvatar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedAvatar",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedAvatar",
                table: "AspNetUsers");
        }
    }
}
