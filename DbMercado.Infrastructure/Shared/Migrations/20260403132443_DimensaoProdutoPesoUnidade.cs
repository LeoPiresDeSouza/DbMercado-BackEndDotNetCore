using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbMercado.Infrastructure.Shared.Migrations
{
    /// <inheritdoc />
    public partial class DimensaoProdutoPesoUnidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DimensaoProduto_Peso",
                table: "prdProduto",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DimensaoProduto_UnidadePeso",
                table: "prdProduto",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE prdProduto
                SET DimensaoProduto_Peso = 1,
                    DimensaoProduto_UnidadePeso = N'KG'
                WHERE DimensaoProduto_Altura IS NOT NULL AND DimensaoProduto_Altura > 0
                  AND (DimensaoProduto_Peso IS NULL OR DimensaoProduto_UnidadePeso IS NULL)
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DimensaoProduto_Peso",
                table: "prdProduto");

            migrationBuilder.DropColumn(
                name: "DimensaoProduto_UnidadePeso",
                table: "prdProduto");
        }
    }
}
