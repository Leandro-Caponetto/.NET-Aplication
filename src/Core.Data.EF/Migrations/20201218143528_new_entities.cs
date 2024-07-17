using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Core.Data.EF.Migrations
{
    public partial class new_entities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Provinces",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    CACode = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShippingTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TNOrderStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TNOrderStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sellers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TNCode = table.Column<int>(nullable: false),
                    TNAccessToken = table.Column<string>(maxLength: 50, nullable: false),
                    TNTokenType = table.Column<string>(maxLength: 50, nullable: false),
                    TNUserId = table.Column<string>(nullable: true),
                    TNScope = table.Column<string>(nullable: true),
                    CAUserId = table.Column<string>(nullable: true),
                    CAUserDate = table.Column<DateTime>(nullable: false),
                    CADocTypeId = table.Column<int>(nullable: false),
                    CADocNro = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(nullable: false),
                    UpdatedOn = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sellers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sellers_DocumentTypes_CADocTypeId",
                        column: x => x.CADocTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CAReceiverBranchOffices",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Address = table.Column<string>(nullable: true),
                    CPA = table.Column<string>(nullable: true),
                    AddressNro = table.Column<int>(nullable: false),
                    Floor = table.Column<string>(nullable: true),
                    Locality = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    ProvinceId = table.Column<int>(nullable: false),
                    Country = table.Column<string>(maxLength: 50, nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    Latitude = table.Column<decimal>(nullable: false),
                    Longitude = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAReceiverBranchOffices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CAReceiverBranchOffices_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SellerId = table.Column<Guid>(nullable: false),
                    TNOrderId = table.Column<int>(nullable: false),
                    TNOrderStatusId = table.Column<int>(nullable: false),
                    TNCreatedOn = table.Column<DateTimeOffset>(nullable: false),
                    TNUpdatedOn = table.Column<DateTimeOffset>(nullable: false),
                    ShippingTypeId = table.Column<int>(nullable: false),
                    CAOrderId = table.Column<string>(nullable: true),
                    SenderName = table.Column<string>(nullable: true),
                    SenderPhone = table.Column<string>(nullable: true),
                    SenderCellPhone = table.Column<string>(nullable: true),
                    SenderStreet = table.Column<string>(nullable: true),
                    SenderHeight = table.Column<int>(nullable: false),
                    SenderFloor = table.Column<string>(nullable: true),
                    SenderDpto = table.Column<string>(nullable: true),
                    SenderLocality = table.Column<string>(nullable: true),
                    SenderProvinceId = table.Column<int>(nullable: false),
                    SenderPostalCode = table.Column<string>(nullable: true),
                    ReceiverName = table.Column<string>(nullable: true),
                    ReceiverPhone = table.Column<string>(nullable: true),
                    ReceiverCellPhone = table.Column<string>(nullable: true),
                    ReceiverMail = table.Column<string>(nullable: true),
                    ReceiverCASucursal = table.Column<string>(nullable: true),
                    ReceiverStreet = table.Column<string>(nullable: true),
                    ReceiverHeight = table.Column<int>(nullable: false),
                    ReceiverFloor = table.Column<string>(nullable: true),
                    ReceiverDpto = table.Column<string>(nullable: true),
                    ReceiverLocality = table.Column<string>(nullable: true),
                    ReceiverProvinceId = table.Column<int>(nullable: false),
                    ReceiverPostalCode = table.Column<string>(nullable: true),
                    TNTotalWeight = table.Column<decimal>(nullable: false),
                    TNTotalPrice = table.Column<decimal>(nullable: false),
                    TotalHeight = table.Column<decimal>(nullable: false),
                    TotalWidth = table.Column<decimal>(nullable: false),
                    TotalDepth = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Provinces_ReceiverProvinceId",
                        column: x => x.ReceiverProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Sellers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Sellers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Provinces_SenderProvinceId",
                        column: x => x.SenderProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_ShippingTypes_ShippingTypeId",
                        column: x => x.ShippingTypeId,
                        principalTable: "ShippingTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_TNOrderStatuses_TNOrderStatusId",
                        column: x => x.TNOrderStatusId,
                        principalTable: "TNOrderStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OrderId = table.Column<Guid>(nullable: false),
                    TNProductId = table.Column<int>(nullable: false),
                    TNProductName = table.Column<string>(nullable: true),
                    TNPrice = table.Column<decimal>(nullable: false),
                    TNQuantity = table.Column<int>(nullable: false),
                    TNWeight = table.Column<decimal>(nullable: false),
                    TNWidth = table.Column<decimal>(nullable: false),
                    TNHeight = table.Column<decimal>(nullable: false),
                    TNDepth = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderProducts_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CAReceiverBranchOffices_ProvinceId",
                table: "CAReceiverBranchOffices",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProducts_OrderId",
                table: "OrderProducts",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ReceiverProvinceId",
                table: "Orders",
                column: "ReceiverProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SellerId",
                table: "Orders",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SenderProvinceId",
                table: "Orders",
                column: "SenderProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingTypeId",
                table: "Orders",
                column: "ShippingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TNOrderStatusId",
                table: "Orders",
                column: "TNOrderStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Sellers_CADocTypeId",
                table: "Sellers",
                column: "CADocTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CAReceiverBranchOffices");

            migrationBuilder.DropTable(
                name: "OrderProducts");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Provinces");

            migrationBuilder.DropTable(
                name: "Sellers");

            migrationBuilder.DropTable(
                name: "ShippingTypes");

            migrationBuilder.DropTable(
                name: "TNOrderStatuses");

            migrationBuilder.DropTable(
                name: "DocumentTypes");
        }
    }
}
