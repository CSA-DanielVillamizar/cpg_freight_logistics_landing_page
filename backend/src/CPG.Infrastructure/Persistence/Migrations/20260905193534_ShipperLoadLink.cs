using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPG.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ShipperLoadLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PodBlobUri",
                table: "loads",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShipperUserId",
                table: "loads",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_loads_ShipperUserId",
                table: "loads",
                column: "ShipperUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_loads_ShipperUserId",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "PodBlobUri",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "ShipperUserId",
                table: "loads");
        }
    }
}
