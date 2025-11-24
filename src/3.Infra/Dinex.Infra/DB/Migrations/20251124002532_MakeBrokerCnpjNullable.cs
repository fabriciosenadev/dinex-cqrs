using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dinex.Infra.DB.Migrations
{
    /// <inheritdoc />
    public partial class MakeBrokerCnpjNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                table: "Brokers",
                type: "character varying(14)",
                unicode: false,
                maxLength: 14,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldUnicode: false,
                oldMaxLength: 14);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                table: "Brokers",
                type: "character varying(14)",
                unicode: false,
                maxLength: 14,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldUnicode: false,
                oldMaxLength: 14,
                oldNullable: true);
        }
    }
}
