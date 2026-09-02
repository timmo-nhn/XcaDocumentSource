using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace XcaXds.Source.Migrations.Statistics
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAccessEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubjectIdHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ResourceIdHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Success = table.Column<int>(type: "integer", nullable: false),
                    SubjectOrganizationCode = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SubjectOrganizationCodeSystem = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SubjectOrganizationDisplayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SubjectOrganizationName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SubjectChildOrganizationCode = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SubjectChildOrganizationCodeSystem = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SubjectChildOrganizationDisplayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SubjectChildOrganizationName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AccessTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Action = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AccessBasis = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ElapsedTimeMillis = table.Column<long>(type: "bigint", nullable: false),
                    ResponseStatusCode = table.Column<int>(type: "integer", nullable: false),
                    SessionId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Issuer = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DocumentConfidentialityCodesJson = table.Column<string>(type: "text", nullable: true),
                    SourceHostName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SourceHomeCommunityId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    SourceRepositoryUniqueId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IssuesJson = table.Column<string>(type: "text", nullable: true),
                    UploadedEntries = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccessEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccessEntries_AccessTime",
                table: "UserAccessEntries",
                column: "AccessTime");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccessEntries_Action",
                table: "UserAccessEntries",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccessEntries_SessionId",
                table: "UserAccessEntries",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAccessEntries");
        }
    }
}
