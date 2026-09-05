using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CPG.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LoadBoardFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeliveryAtUtc",
                table: "loads",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "DestinationCity",
                table: "loads",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DestinationState",
                table: "loads",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DistanceMiles",
                table: "loads",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EquipmentType",
                table: "loads",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginCity",
                table: "loads",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginState",
                table: "loads",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PickupAtUtc",
                table: "loads",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<decimal>(
                name: "RateUsd",
                table: "loads",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ShipperName",
                table: "loads",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpecialInstructions",
                table: "loads",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetTemperatureF",
                table: "loads",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_loads_AssignedCarrierId",
                table: "loads",
                column: "AssignedCarrierId");

            migrationBuilder.AddForeignKey(
                name: "FK_loads_carriers_AssignedCarrierId",
                table: "loads",
                column: "AssignedCarrierId",
                principalTable: "carriers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_loads_carriers_AssignedCarrierId",
                table: "loads");

            migrationBuilder.DropIndex(
                name: "IX_loads_AssignedCarrierId",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "DeliveryAtUtc",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "DestinationCity",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "DestinationState",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "DistanceMiles",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "EquipmentType",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "OriginCity",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "OriginState",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "PickupAtUtc",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "RateUsd",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "ShipperName",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "SpecialInstructions",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "TargetTemperatureF",
                table: "loads");
        }
    }
}
