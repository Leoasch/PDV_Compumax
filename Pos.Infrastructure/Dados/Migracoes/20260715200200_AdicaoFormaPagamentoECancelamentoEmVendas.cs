using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pos.Infrastructure.Dados.Migracoes
{
    /// <inheritdoc />
    public partial class AdicaoFormaPagamentoECancelamentoEmVendas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Cancelada",
                table: "Vendas",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCancelamento",
                table: "Vendas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FormaPagamento",
                table: "Vendas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cancelada",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "DataCancelamento",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "FormaPagamento",
                table: "Vendas");
        }
    }
}
