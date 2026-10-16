using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Itemiza.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTextoExtraidoNota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TextoExtraido",
                table: "NotasFiscais",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TextoExtraido",
                table: "NotasFiscais");
        }
    }
}
