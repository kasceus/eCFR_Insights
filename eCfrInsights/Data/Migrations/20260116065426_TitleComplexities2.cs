using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecfrInsights.Data.Migrations
{
    /// <inheritdoc />
    public partial class TitleComplexities2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgencyStatistics_Agencies_Slug",
                table: "AgencyStatistics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CfrTitleComplexities",
                table: "CfrTitleComplexities");

            migrationBuilder.RenameColumn(
                name: "TotalCfrReferences",
                table: "AgencyStatistics",
                newName: "TotalHierarchies");

            migrationBuilder.AlterColumn<double>(
                name: "ComplexityScore",
                table: "AgencyStatistics",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "AgencyName",
                table: "AgencyStatistics",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "NormAgencies",
                table: "AgencyStatistics",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NormCorrections",
                table: "AgencyStatistics",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NormHierarchical",
                table: "AgencyStatistics",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "NormWordcount",
                table: "AgencyStatistics",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CfrTitleComplexities",
                table: "CfrTitleComplexities",
                columns: new[] { "Title", "DateComputed" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CfrTitleComplexities",
                table: "CfrTitleComplexities");

            migrationBuilder.DropColumn(
                name: "AgencyName",
                table: "AgencyStatistics");

            migrationBuilder.DropColumn(
                name: "NormAgencies",
                table: "AgencyStatistics");

            migrationBuilder.DropColumn(
                name: "NormCorrections",
                table: "AgencyStatistics");

            migrationBuilder.DropColumn(
                name: "NormHierarchical",
                table: "AgencyStatistics");

            migrationBuilder.DropColumn(
                name: "NormWordcount",
                table: "AgencyStatistics");

            migrationBuilder.RenameColumn(
                name: "TotalHierarchies",
                table: "AgencyStatistics",
                newName: "TotalCfrReferences");

            migrationBuilder.AlterColumn<int>(
                name: "ComplexityScore",
                table: "AgencyStatistics",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CfrTitleComplexities",
                table: "CfrTitleComplexities",
                column: "Title");

            migrationBuilder.AddForeignKey(
                name: "FK_AgencyStatistics_Agencies_Slug",
                table: "AgencyStatistics",
                column: "Slug",
                principalTable: "Agencies",
                principalColumn: "Slug",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
