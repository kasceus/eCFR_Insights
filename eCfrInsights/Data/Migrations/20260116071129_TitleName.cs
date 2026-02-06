using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecfrInsights.Data.Migrations
{
    /// <inheritdoc />
    public partial class TitleName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TitleText",
                table: "CfrTitleComplexities",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateComputed",
                table: "AgencyStatistics",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TitleText",
                table: "CfrTitleComplexities");

            migrationBuilder.DropColumn(
                name: "DateComputed",
                table: "AgencyStatistics");
        }
    }
}
