using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DebateRoyale.Migrations
{
    public partial class AddIdentityRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "GeneralTopic", "Name" },
                values: new object[] { "Film, musica, trends...", "Cultura Pop" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "GeneralTopic", "Name" },
                values: new object[] { "Il futuro della tecnologia", "Tecnologia" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "GeneralTopic", "Name" },
                values: new object[] { "Dibattiti filosofici", "Filosofia" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "GeneralTopic", "Name" },
                values: new object[] { "Discussioni politiche", "Politica" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "GeneralTopic", "Name" },
                values: new object[] { "Political Discussions", "Politics Arena" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "GeneralTopic", "Name" },
                values: new object[] { "Technology and Future", "Tech Sphere" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "GeneralTopic", "Name" },
                values: new object[] { "Philosophical Debates", "Philosophy Hall" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "GeneralTopic", "Name" },
                values: new object[] { "Movies, Music, and Trends", "Pop Culture Corner" });
        }
    }
}
