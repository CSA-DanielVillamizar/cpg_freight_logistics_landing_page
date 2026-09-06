using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPG.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteAndSyntheticFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "loads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "invoices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_loads_IsDeleted",
                table: "loads",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_IsDeleted",
                table: "invoices",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_loads_IsDeleted",
                table: "loads");

            migrationBuilder.DropIndex(
                name: "IX_invoices_IsDeleted",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "invoices");
        }
    }
}
