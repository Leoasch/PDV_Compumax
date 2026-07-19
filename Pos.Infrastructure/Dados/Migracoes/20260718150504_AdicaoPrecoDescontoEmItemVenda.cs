using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pos.Infrastructure.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class AdicaoPrecoDescontoEmItemVenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecoDesconto",
                table: "ItensVenda",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecoDesconto",
                table: "ItensVenda");
        }
    }
}
