using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecfrInsights.Data.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CfrHierarchyHistories_CfrHierarchyHistories_ParentCfrReferenceNumber",
                table: "CfrHierarchyHistories");

            migrationBuilder.AddForeignKey(
                name: "FK_CfrHierarchyHistories_CfrHierarchyHistories_ParentCfrReferenceNumber",
                table: "CfrHierarchyHistories",
                column: "ParentCfrReferenceNumber",
                principalTable: "CfrHierarchyHistories",
                principalColumn: "CfrReferenceNumber",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CfrHierarchyHistories_CfrHierarchyHistories_ParentCfrReferenceNumber",
                table: "CfrHierarchyHistories");

            migrationBuilder.AddForeignKey(
                name: "FK_CfrHierarchyHistories_CfrHierarchyHistories_ParentCfrReferenceNumber",
                table: "CfrHierarchyHistories",
                column: "ParentCfrReferenceNumber",
                principalTable: "CfrHierarchyHistories",
                principalColumn: "CfrReferenceNumber",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
