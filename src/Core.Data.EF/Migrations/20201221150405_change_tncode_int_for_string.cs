using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Data.EF.Migrations
{
    public partial class change_tncode_int_for_string : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TNCode",
                table: "Sellers",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TNCode",
                table: "Sellers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
