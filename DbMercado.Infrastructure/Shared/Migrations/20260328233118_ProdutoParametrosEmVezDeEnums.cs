using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbMercado.Infrastructure.Shared.Migrations
{
    /// <inheritdoc />
    public partial class ProdutoParametrosEmVezDeEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrigemProduto_Tipo_new",
                table: "prdProduto",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "NACIONAL");

            migrationBuilder.Sql("""
                UPDATE prdProduto SET OrigemProduto_Tipo_new = CASE OrigemProduto_Tipo
                    WHEN 1 THEN N'NACIONAL'
                    WHEN 2 THEN N'IMPORTADO'
                    ELSE N'NACIONAL'
                END
                """);

            migrationBuilder.DropColumn(
                name: "OrigemProduto_Tipo",
                table: "prdProduto");

            migrationBuilder.RenameColumn(
                name: "OrigemProduto_Tipo_new",
                table: "prdProduto",
                newName: "OrigemProduto_Tipo");

            migrationBuilder.AddColumn<string>(
                name: "DadosFiscais_Origem_new",
                table: "prdProduto",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "0");

            migrationBuilder.Sql("""
                UPDATE prdProduto SET DadosFiscais_Origem_new = CAST(DadosFiscais_Origem AS nvarchar(8))
                """);

            migrationBuilder.DropColumn(
                name: "DadosFiscais_Origem",
                table: "prdProduto");

            migrationBuilder.RenameColumn(
                name: "DadosFiscais_Origem_new",
                table: "prdProduto",
                newName: "DadosFiscais_Origem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrigemProduto_Tipo_old",
                table: "prdProduto",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql("""
                UPDATE prdProduto SET OrigemProduto_Tipo_old = CASE UPPER(LTRIM(RTRIM(OrigemProduto_Tipo)))
                    WHEN N'IMPORTADO' THEN 2
                    ELSE 1
                END
                """);

            migrationBuilder.DropColumn(
                name: "OrigemProduto_Tipo",
                table: "prdProduto");

            migrationBuilder.RenameColumn(
                name: "OrigemProduto_Tipo_old",
                table: "prdProduto",
                newName: "OrigemProduto_Tipo");

            migrationBuilder.AddColumn<byte>(
                name: "DadosFiscais_Origem_old",
                table: "prdProduto",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.Sql("""
                UPDATE prdProduto SET DadosFiscais_Origem_old = CASE
                    WHEN TRY_CAST(DadosFiscais_Origem AS tinyint) IS NOT NULL THEN CAST(DadosFiscais_Origem AS tinyint)
                    ELSE 0
                END
                """);

            migrationBuilder.DropColumn(
                name: "DadosFiscais_Origem",
                table: "prdProduto");

            migrationBuilder.RenameColumn(
                name: "DadosFiscais_Origem_old",
                table: "prdProduto",
                newName: "DadosFiscais_Origem");
        }
    }
}
