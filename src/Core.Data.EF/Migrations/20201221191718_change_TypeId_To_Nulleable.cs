using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Data.EF.Migrations
{
    public partial class change_TypeId_To_Nulleable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sellers_DocumentTypes_CADocTypeId",
                table: "Sellers");

            migrationBuilder.AlterColumn<int>(
                name: "CADocTypeId",
                table: "Sellers",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Sellers_DocumentTypes_CADocTypeId",
                table: "Sellers",
                column: "CADocTypeId",
                principalTable: "DocumentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sellers_DocumentTypes_CADocTypeId",
                table: "Sellers");

            migrationBuilder.AlterColumn<int>(
                name: "CADocTypeId",
                table: "Sellers",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sellers_DocumentTypes_CADocTypeId",
                table: "Sellers",
                column: "CADocTypeId",
                principalTable: "DocumentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
