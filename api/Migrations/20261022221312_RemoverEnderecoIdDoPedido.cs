using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class RemoverEnderecoIdDoPedido : Migration
    {
        /// <inheritdoc />
        /// 
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            UPDATE p
            SET p.EnderecoEntrega_Cep = e.Cep,
                p.EnderecoEntrega_Rua = e.Rua,
                p.EnderecoEntrega_Numero = e.Numero,
                p.EnderecoEntrega_Complemento = e.Complemento,
                p.EnderecoEntrega_Bairro = e.Bairro,
                p.EnderecoEntrega_Cidade = e.Cidade,
                p.EnderecoEntrega_Uf = e.Uf
            FROM Pedido p
            INNER JOIN Endereco e ON e.Id = p.EnderecoId;");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Endereco_EnderecoId",
                table: "Pedido");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_EnderecoId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "EnderecoEntrega_Id",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "EnderecoId",
                table: "Pedido");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EnderecoEntrega_Id",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EnderecoId",
                table: "Pedido",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_EnderecoId",
                table: "Pedido",
                column: "EnderecoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Endereco_EnderecoId",
                table: "Pedido",
                column: "EnderecoId",
                principalTable: "Endereco",
                principalColumn: "Id");
        }
    }
}
