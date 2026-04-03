using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbMercado.Infrastructure.Shared.Migrations
{
    /// <inheritdoc />
    public partial class RefatoracaoUnidadesProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UnidadeMedida",
                table: "prdProduto",
                newName: "UnidadeMedidaFisica");

            migrationBuilder.AddColumn<string>(
                name: "DimensaoEmbalagem_UnidadeDimensao",
                table: "prdProduto",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "CM");

            migrationBuilder.AddColumn<string>(
                name: "DimensaoEmbalagem_UnidadePeso",
                table: "prdProduto",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "KG");

            migrationBuilder.AddColumn<string>(
                name: "DimensaoProduto_UnidadeDimensao",
                table: "prdProduto",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoEmbalagem",
                table: "prdProduto",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnidadeComercializacao",
                table: "prdProduto",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE prdProduto
                SET UnidadeComercializacao = UnidadeMedidaFisica,
                    TipoEmbalagem = UnidadeMedidaFisica
                """);

            migrationBuilder.Sql(
                """
                UPDATE prdProduto
                SET DimensaoProduto_UnidadeDimensao = N'CM'
                WHERE DimensaoProduto_Altura IS NOT NULL AND DimensaoProduto_Altura > 0
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DimensaoEmbalagem_UnidadeDimensao",
                table: "prdProduto");

            migrationBuilder.DropColumn(
                name: "DimensaoEmbalagem_UnidadePeso",
                table: "prdProduto");

            migrationBuilder.DropColumn(
                name: "DimensaoProduto_UnidadeDimensao",
                table: "prdProduto");

            migrationBuilder.DropColumn(
                name: "TipoEmbalagem",
                table: "prdProduto");

            migrationBuilder.DropColumn(
                name: "UnidadeComercializacao",
                table: "prdProduto");

            migrationBuilder.RenameColumn(
                name: "UnidadeMedidaFisica",
                table: "prdProduto",
                newName: "UnidadeMedida");
        }
    }
}
