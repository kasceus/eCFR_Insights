using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecfrInsights.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agencies",
                columns: table => new
                {
                    Slug = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ShortName = table.Column<string>(type: "TEXT", nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    SortableName = table.Column<string>(type: "TEXT", nullable: false),
                    ParentSlug = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencies", x => x.Slug);
                    table.ForeignKey(
                        name: "FK_Agencies_Agencies_ParentSlug",
                        column: x => x.ParentSlug,
                        principalTable: "Agencies",
                        principalColumn: "Slug",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CfrTitleHistories",
                columns: table => new
                {
                    Number = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LatestAmendedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LatestIssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpToDateAsOf = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reserved = table.Column<bool>(type: "INTEGER", nullable: false),
                    SectionCount = table.Column<int>(type: "INTEGER", nullable: false),
                    XmlDocumentHash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CfrTitleHistories", x => x.Number);
                });

            migrationBuilder.CreateTable(
                name: "CfrTitles",
                columns: table => new
                {
                    Number = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LatestAmendedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LatestIssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpToDateAsOf = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reserved = table.Column<bool>(type: "INTEGER", nullable: false),
                    SectionCount = table.Column<int>(type: "INTEGER", nullable: false),
                    XmlDocumentHash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CfrTitles", x => x.Number);
                });

            migrationBuilder.CreateTable(
                name: "SyncStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LastSyncedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgencyStatistics",
                columns: table => new
                {
                    Slug = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ForDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalCfrReferences = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalWords = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalSubAgencies = table.Column<int>(type: "INTEGER", nullable: false),
                    ComplexityScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyStatistics", x => new { x.Slug, x.ForDate });
                    table.ForeignKey(
                        name: "FK_AgencyStatistics_Agencies_Slug",
                        column: x => x.Slug,
                        principalTable: "Agencies",
                        principalColumn: "Slug",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CfrHierarchies",
                columns: table => new
                {
                    CfrReferenceNumber = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    ParentCfrReferenceNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Authority = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    Source = table.Column<string>(type: "TEXT", nullable: true),
                    CfrReferenceTitle = table.Column<string>(type: "TEXT", nullable: true),
                    TitleNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Chapter = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    Subtitle = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    Part = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    Subchapter = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    Subpart = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    Section = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    Appendix = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    LatestAmendedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LatestIssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpToDateAsOf = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reserved = table.Column<bool>(type: "INTEGER", nullable: false),
                    AgencySlug = table.Column<string>(type: "TEXT", nullable: true),
                    ReferenceContent = table.Column<string>(type: "TEXT", nullable: true),
                    Citation = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CfrHierarchies", x => x.CfrReferenceNumber);
                    table.ForeignKey(
                        name: "FK_CfrHierarchies_CfrHierarchies_ParentCfrReferenceNumber",
                        column: x => x.ParentCfrReferenceNumber,
                        principalTable: "CfrHierarchies",
                        principalColumn: "CfrReferenceNumber",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CfrHierarchies_CfrTitles_TitleNumber",
                        column: x => x.TitleNumber,
                        principalTable: "CfrTitles",
                        principalColumn: "Number",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Corrections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CorrectiveAction = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorCorrected = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorOccurred = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FrCitation = table.Column<string>(type: "TEXT", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayInToc = table.Column<bool>(type: "INTEGER", nullable: false),
                    Title = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Corrections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Corrections_CfrTitles_Title",
                        column: x => x.Title,
                        principalTable: "CfrTitles",
                        principalColumn: "Number",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgencyHierarchies",
                columns: table => new
                {
                    Slug = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: false),
                    CfrReferenceNumber = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyHierarchies", x => new { x.Slug, x.CfrReferenceNumber });
                    table.ForeignKey(
                        name: "FK_AgencyHierarchies_Agencies_Slug",
                        column: x => x.Slug,
                        principalTable: "Agencies",
                        principalColumn: "Slug",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgencyHierarchies_CfrHierarchies_CfrReferenceNumber",
                        column: x => x.CfrReferenceNumber,
                        principalTable: "CfrHierarchies",
                        principalColumn: "CfrReferenceNumber",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CfrHierarchyHistories",
                columns: table => new
                {
                    CfrReferenceNumber = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    ParentCfrReferenceNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Authority = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    Source = table.Column<string>(type: "TEXT", nullable: true),
                    CfrReferenceTitle = table.Column<string>(type: "TEXT", nullable: true),
                    TitleNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Chapter = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    Subtitle = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    Part = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    Subchapter = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    Subpart = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    Section = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    Appendix = table.Column<string>(type: "nvarchar", maxLength: 255, nullable: true),
                    LatestAmendedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LatestIssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpToDateAsOf = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reserved = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReferenceContent = table.Column<string>(type: "TEXT", nullable: true),
                    Citation = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CfrHierarchyHistories", x => x.CfrReferenceNumber);
                    table.ForeignKey(
                        name: "FK_CfrHierarchyHistories_CfrHierarchies_CfrReferenceNumber",
                        column: x => x.CfrReferenceNumber,
                        principalTable: "CfrHierarchies",
                        principalColumn: "CfrReferenceNumber",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CfrHierarchyHistories_CfrHierarchyHistories_ParentCfrReferenceNumber",
                        column: x => x.ParentCfrReferenceNumber,
                        principalTable: "CfrHierarchyHistories",
                        principalColumn: "CfrReferenceNumber",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CfrHierarchyHistories_CfrTitleHistories_TitleNumber",
                        column: x => x.TitleNumber,
                        principalTable: "CfrTitleHistories",
                        principalColumn: "Number",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agencies_ParentSlug",
                table: "Agencies",
                column: "ParentSlug");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyHierarchies_CfrReferenceNumber",
                table: "AgencyHierarchies",
                column: "CfrReferenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CfrHierarchies_AgencySlug",
                table: "CfrHierarchies",
                column: "AgencySlug");

            migrationBuilder.CreateIndex(
                name: "IX_CfrHierarchies_ParentCfrReferenceNumber",
                table: "CfrHierarchies",
                column: "ParentCfrReferenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CfrHierarchies_TitleNumber",
                table: "CfrHierarchies",
                column: "TitleNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CfrHierarchyHistories_ParentCfrReferenceNumber",
                table: "CfrHierarchyHistories",
                column: "ParentCfrReferenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CfrHierarchyHistories_TitleNumber",
                table: "CfrHierarchyHistories",
                column: "TitleNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Corrections_Title",
                table: "Corrections",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Corrections_Year",
                table: "Corrections",
                column: "Year");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyHierarchies");

            migrationBuilder.DropTable(
                name: "AgencyStatistics");

            migrationBuilder.DropTable(
                name: "CfrHierarchyHistories");

            migrationBuilder.DropTable(
                name: "Corrections");

            migrationBuilder.DropTable(
                name: "SyncStatuses");

            migrationBuilder.DropTable(
                name: "Agencies");

            migrationBuilder.DropTable(
                name: "CfrHierarchies");

            migrationBuilder.DropTable(
                name: "CfrTitleHistories");

            migrationBuilder.DropTable(
                name: "CfrTitles");
        }
    }
}
