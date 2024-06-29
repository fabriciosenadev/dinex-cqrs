using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dinex.Infra.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityInvestmentTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvestmentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Applicable = table.Column<int>(type: "integer", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetUnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    AssetTransactionAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    AssetQuantity = table.Column<int>(type: "integer", nullable: false),
                    StockBrokerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentTransactions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestmentTransactions");
        }
    }
}
