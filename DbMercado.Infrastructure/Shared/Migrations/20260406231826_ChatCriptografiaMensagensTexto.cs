using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbMercado.Infrastructure.Shared.Migrations
{
    /// <summary>
    /// Etapa 6 (chat): criptografia AES-256-GCM via ValueConverter no AppDbContext; sem mudança de schema (colunas já nvarchar(max)).
    /// </summary>
    public partial class ChatCriptografiaMensagensTexto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
