using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbMercado.Infrastructure.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddJobExecucao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dbJobExecucao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FireInstanceId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    JobNome = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    JobGrupo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TriggerNome = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TriggerGrupo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    InicioUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FimUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DuracaoMs = table.Column<long>(type: "bigint", nullable: true),
                    Sucesso = table.Column<bool>(type: "bit", nullable: true),
                    MensagemErro = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbJobExecucao", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dbJobExecucao_FireInstanceId",
                table: "dbJobExecucao",
                column: "FireInstanceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dbJobExecucao_InicioUtc",
                table: "dbJobExecucao",
                column: "InicioUtc");

            migrationBuilder.CreateIndex(
                name: "IX_dbJobExecucao_JobNome_JobGrupo_InicioUtc",
                table: "dbJobExecucao",
                columns: new[] { "JobNome", "JobGrupo", "InicioUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dbJobExecucao");
        }
    }
}
