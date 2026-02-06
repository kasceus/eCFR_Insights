using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecfrInsights.Data.Migrations
{
    /// <inheritdoc />
    public partial class TitleComplexities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CfrTitleComplexities",
                columns: table => new
                {
                    Title = table.Column<int>(type: "INTEGER", nullable: false),
                    HierarchicalCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAgencies = table.Column<int>(type: "INTEGER", nullable: false),
                    Wordcount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalCorrections = table.Column<int>(type: "INTEGER", nullable: false),
                    NormHierarchical = table.Column<double>(type: "REAL", precision: 18, scale: 4, nullable: false),
                    NormAgencies = table.Column<double>(type: "REAL", precision: 18, scale: 4, nullable: false),
                    NormWordcount = table.Column<double>(type: "REAL", precision: 18, scale: 4, nullable: false),
                    NormCorrections = table.Column<double>(type: "REAL", precision: 18, scale: 4, nullable: false),
                    ComplexityScore = table.Column<double>(type: "REAL", precision: 18, scale: 4, nullable: false),
                    DateComputed = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CfrTitleComplexities", x => x.Title);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CfrTitleComplexities");
        }
    }
}
