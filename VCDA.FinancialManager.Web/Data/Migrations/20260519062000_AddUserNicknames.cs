using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VCDA.FinancialManager.Web.Data.Migrations
{
    /// <inheritdoc />
    [Migration("20260519062000_AddUserNicknames")]
    public partial class AddUserNicknames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedNickname",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE AspNetUsers
                SET Nickname = substr(trim(coalesce(UserName, Email, Id)), 1, 23) || '-' || substr(Id, 1, 8),
                    NormalizedNickname = upper(substr(trim(coalesce(UserName, Email, Id)), 1, 23) || '-' || substr(Id, 1, 8))
                WHERE Nickname = '' OR NormalizedNickname = '';
                """);

            migrationBuilder.CreateIndex(
                name: "NicknameIndex",
                table: "AspNetUsers",
                column: "NormalizedNickname",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "NicknameIndex",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedNickname",
                table: "AspNetUsers");
        }
    }
}
