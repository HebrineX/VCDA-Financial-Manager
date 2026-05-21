using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VCDA.FinancialManager.Web.Migrations
{
    public partial class AddTransactionImportOrigin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsImported",
                table: "Transacciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsImported",
                table: "Transacciones");
        }
    }
}
