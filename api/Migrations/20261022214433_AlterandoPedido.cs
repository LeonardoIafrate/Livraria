using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AlterandoPedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Endereco_EnderecoId",
                table: "Pedido");

            migrationBuilder.AlterColumn<int>(
                name: "EnderecoId",
                table: "Pedido",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoEntrega_Bairro",
                table: "Pedido",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoEntrega_Cep",
                table: "Pedido",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoEntrega_Cidade",
                table: "Pedido",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoEntrega_Complemento",
                table: "Pedido",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnderecoEntrega_Id",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoEntrega_Numero",
                table: "Pedido",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoEntrega_Rua",
                table: "Pedido",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoEntrega_Uf",
                table: "Pedido",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Endereco_EnderecoId",
                table: "Pedido",
                column: "EnderecoId",
                principalTable: "Endereco",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Endereco_EnderecoId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "EnderecoEntrega_Bairro",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "EnderecoEntrega_Cep",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "EnderecoEntrega_Cidade",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "EnderecoEntrega_Complemento",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "EnderecoEntrega_Id",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "EnderecoEntrega_Numero",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "EnderecoEntrega_Rua",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "EnderecoEntrega_Uf",
                table: "Pedido");

            migrationBuilder.AlterColumn<int>(
                name: "EnderecoId",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Endereco_EnderecoId",
                table: "Pedido",
                column: "EnderecoId",
                principalTable: "Endereco",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
