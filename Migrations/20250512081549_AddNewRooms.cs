using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DebateRoyale.Migrations
{
    public partial class AddNewRooms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1,
                column: "GeneralTopic",
                value: "Cinema, musica, tendenze e fenomeni culturali");

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "GeneralTopic", "Name" },
                values: new object[] { "Nuove tecnologie, futuro digitale e impatti sociali", "Innovazione Tecnologica" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "GeneralTopic", "Name" },
                values: new object[] { "Dibattiti su etica, esistenza e grandi domande", "Filosofia e Pensiero Critico" });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "GeneralTopic", "Name" },
                values: new object[] { "Fisica, biologia, chimica, scoperte e ricerche scientifiche", "Scienza" });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "GeneralTopic", "MaxUsers", "Name" },
                values: new object[] { 5, "Discussioni su politica, società e attualità", 0, "Attualità e Politica" });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "GeneralTopic", "MaxUsers", "Name" },
                values: new object[] { 6, "Argomentazioni su temi generali e curiosità", 0, "Domande Aperte" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "Id",
                keyValue: 1,
                column: "GeneralTopic",
                value: "Film, musica, trends...");

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
    }
}
