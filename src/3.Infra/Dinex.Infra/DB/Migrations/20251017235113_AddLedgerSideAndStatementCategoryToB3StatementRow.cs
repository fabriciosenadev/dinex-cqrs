using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dinex.Infra.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddLedgerSideAndStatementCategoryToB3StatementRow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LedgerSide",
                table: "B3StatementRows",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatementCategory",
                table: "B3StatementRows",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LedgerSide",
                table: "B3StatementRows");

            migrationBuilder.DropColumn(
                name: "StatementCategory",
                table: "B3StatementRows");
        }
    }
}
