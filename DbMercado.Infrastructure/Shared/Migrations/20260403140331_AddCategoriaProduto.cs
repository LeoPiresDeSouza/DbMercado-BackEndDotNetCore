using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbMercado.Infrastructure.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriaProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CategoriaProdutoId",
                table: "prdProduto",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "prdCategoria",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CategoriaPaiId = table.Column<long>(type: "bigint", nullable: true),
                    Nivel = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataUltimaAlteracao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioCriacao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsuarioUltimaAlteracao = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prdCategoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prdCategoria_prdCategoria_CategoriaPaiId",
                        column: x => x.CategoriaPaiId,
                        principalTable: "prdCategoria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_prdProduto_CategoriaProdutoId",
                table: "prdProduto",
                column: "CategoriaProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_prdCategoria_CategoriaPaiId",
                table: "prdCategoria",
                column: "CategoriaPaiId");

            migrationBuilder.CreateIndex(
                name: "IX_prdCategoria_CategoriaPaiId_Ativo",
                table: "prdCategoria",
                columns: new[] { "CategoriaPaiId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_prdCategoria_Nivel",
                table: "prdCategoria",
                column: "Nivel");

            migrationBuilder.CreateIndex(
                name: "IX_prdCategoria_Slug",
                table: "prdCategoria",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_prdProduto_prdCategoria_CategoriaProdutoId",
                table: "prdProduto",
                column: "CategoriaProdutoId",
                principalTable: "prdCategoria",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_prdProduto_prdCategoria_CategoriaProdutoId",
                table: "prdProduto");

            migrationBuilder.DropTable(
                name: "prdCategoria");

            migrationBuilder.DropIndex(
                name: "IX_prdProduto_CategoriaProdutoId",
                table: "prdProduto");

            migrationBuilder.DropColumn(
                name: "CategoriaProdutoId",
                table: "prdProduto");
        }
    }
}
