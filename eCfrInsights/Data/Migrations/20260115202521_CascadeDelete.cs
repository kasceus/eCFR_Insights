using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecfrInsights.Data.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CfrHierarchies_CfrHierarchies_ParentCfrReferenceNumber",
                table: "CfrHierarchies");

            migrationBuilder.AddForeignKey(
                name: "FK_CfrHierarchies_CfrHierarchies_ParentCfrReferenceNumber",
                table: "CfrHierarchies",
                column: "ParentCfrReferenceNumber",
                principalTable: "CfrHierarchies",
                principalColumn: "CfrReferenceNumber",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CfrHierarchies_CfrHierarchies_ParentCfrReferenceNumber",
                table: "CfrHierarchies");

            migrationBuilder.AddForeignKey(
                name: "FK_CfrHierarchies_CfrHierarchies_ParentCfrReferenceNumber",
                table: "CfrHierarchies",
                column: "ParentCfrReferenceNumber",
                principalTable: "CfrHierarchies",
                principalColumn: "CfrReferenceNumber",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
