using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Data.EF.Migrations
{
    public partial class add_fields_to_orders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorCode",
                table: "Orders",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorDescription",
                table: "Orders",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ErrorDescription",
                table: "Orders");
        }
    }
}
