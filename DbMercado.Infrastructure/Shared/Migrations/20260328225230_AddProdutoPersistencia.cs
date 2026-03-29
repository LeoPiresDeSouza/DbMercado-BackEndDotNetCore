using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbMercado.Infrastructure.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddProdutoPersistencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prdProduto",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Modelo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Gtin = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    UnidadeMedida = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    DimensaoProduto_Altura = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DimensaoProduto_Largura = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DimensaoProduto_Comprimento = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DimensaoEmbalagem_Altura = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DimensaoEmbalagem_Largura = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DimensaoEmbalagem_Comprimento = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DimensaoEmbalagem_Peso = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    OrigemProduto_Tipo = table.Column<int>(type: "int", nullable: false),
                    OrigemProduto_PaisOrigem = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DadosFiscais_Ncm = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    DadosFiscais_Cest = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    DadosFiscais_Origem = table.Column<byte>(type: "tinyint", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataUltimaAlteracao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCriacao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioUltimaAlteracao = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prdProduto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "prdProdutoAtributo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    ProdutoId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prdProdutoAtributo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prdProdutoAtributo_prdProduto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "prdProduto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prdSku",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProdutoId = table.Column<long>(type: "bigint", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataUltimaAlteracao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCriacao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioUltimaAlteracao = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prdSku", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prdSku_prdProduto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "prdProduto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_prdProdutoAtributo_ProdutoId",
                table: "prdProdutoAtributo",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_prdSku_ProdutoId_Codigo",
                table: "prdSku",
                columns: new[] { "ProdutoId", "Codigo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prdProdutoAtributo");

            migrationBuilder.DropTable(
                name: "prdSku");

            migrationBuilder.DropTable(
                name: "prdProduto");
        }
    }
}
