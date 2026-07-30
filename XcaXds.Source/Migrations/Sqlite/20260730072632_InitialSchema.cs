using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XcaXds.Source.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistryObjects",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    AS_AssociationType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    AS_SubmissionSetStatus = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    AS_SourceObjectId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    AS_TargetObjectId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_MimeType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_Hash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_RepositoryUniqueId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_Size = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_ObjectType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_AvailabilityStatus = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_HomeCommunityId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_UniqueId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_LanguageCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_CreationTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DE_ServiceStartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DE_ServiceStopTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DE_SourcePatientInfoPatientId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_SourcePatientInfoPatientSystem = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_SourcePatientInfoFirstName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_SourcePatientInfoLastName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_SourcePatientInfoBirthTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DE_SourcePatientInfoGender = table.Column<string>(type: "TEXT", nullable: true),
                    DE_ClassCode_Code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_ClassCode_CodeSystem = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_ClassCode_Discriminator = table.Column<string>(type: "TEXT", nullable: true),
                    DE_ClassCode_DisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_ClassCode_Id = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_EventCodeList_Code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_EventCodeList_CodeSystem = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_EventCodeList_Discriminator = table.Column<string>(type: "TEXT", nullable: true),
                    DE_EventCodeList_DisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_EventCodeList_Id = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_FormatCode_Code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_FormatCode_CodeSystem = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_FormatCode_Discriminator = table.Column<string>(type: "TEXT", nullable: true),
                    DE_FormatCode_DisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_FormatCode_Id = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_HealthCareFacilityTypeCode_Code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_HealthCareFacilityTypeCode_CodeSystem = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_HealthCareFacilityTypeCode_Discriminator = table.Column<string>(type: "TEXT", nullable: true),
                    DE_HealthCareFacilityTypeCode_DisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_HealthCareFacilityTypeCode_Id = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_LegalAuthenticator_Discriminator = table.Column<string>(type: "TEXT", nullable: true),
                    DE_LegalAuthenticator_FirstName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_LegalAuthenticator_Id = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_LegalAuthenticator_IdSystem = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_LegalAuthenticator_LastName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_PracticeSettingCode_Code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_PracticeSettingCode_CodeSystem = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_PracticeSettingCode_Discriminator = table.Column<string>(type: "TEXT", nullable: true),
                    DE_PracticeSettingCode_DisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_PracticeSettingCode_Id = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_TypeCode_Code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_TypeCode_CodeSystem = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_TypeCode_Discriminator = table.Column<string>(type: "TEXT", nullable: true),
                    DE_TypeCode_DisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DE_TypeCode_Id = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SS_AvailabilityStatus = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SS_HomeCommunityId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SS_Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SS_SubmissionTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SS_SourceId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SS_UniqueId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistryObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistryObjects_RegistryObjects_AS_SourceObjectId",
                        column: x => x.AS_SourceObjectId,
                        principalTable: "RegistryObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RegistryObjects_RegistryObjects_AS_TargetObjectId",
                        column: x => x.AS_TargetObjectId,
                        principalTable: "RegistryObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DocumentEntry_Authors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DocumentEntryId = table.Column<string>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    OrganizationAssigningAuthority = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    OrganizationName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DepartmentId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DepartmentAssigningAuthority = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DepartmentName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PersonId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PersonAssigningAuthority = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PersonFirstName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PersonLastName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    RoleCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    RoleCodeSystem = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    RoleDisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SpecialityCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SpecialityCodeSystem = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SpecialityDisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentEntry_Authors", x => new { x.DocumentEntryId, x.Id });
                    table.ForeignKey(
                        name: "FK_DocumentEntry_Authors_RegistryObjects_DocumentEntryId",
                        column: x => x.DocumentEntryId,
                        principalTable: "RegistryObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentEntry_ConfidentialityCodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    DocumentEntryId = table.Column<string>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CodeSystem = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentEntry_ConfidentialityCodes", x => new { x.DocumentEntryId, x.Id });
                    table.ForeignKey(
                        name: "FK_DocumentEntry_ConfidentialityCodes_RegistryObjects_DocumentEntryId",
                        column: x => x.DocumentEntryId,
                        principalTable: "RegistryObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionSet_Authors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SubmissionSetId = table.Column<string>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    OrganizationAssigningAuthority = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    OrganizationName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DepartmentId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DepartmentAssigningAuthority = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    DepartmentName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PersonId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PersonAssigningAuthority = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PersonFirstName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    PersonLastName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    RoleCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    RoleCodeSystem = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    RoleDisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SpecialityCode = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SpecialityCodeSystem = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SpecialityDisplayName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionSet_Authors", x => new { x.SubmissionSetId, x.Id });
                    table.ForeignKey(
                        name: "FK_SubmissionSet_Authors_RegistryObjects_SubmissionSetId",
                        column: x => x.SubmissionSetId,
                        principalTable: "RegistryObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistryObjects_AS_SourceObjectId",
                table: "RegistryObjects",
                column: "AS_SourceObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistryObjects_AS_TargetObjectId",
                table: "RegistryObjects",
                column: "AS_TargetObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistryObjects_DE_HomeCommunityId",
                table: "RegistryObjects",
                column: "DE_HomeCommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistryObjects_DE_RepositoryUniqueId",
                table: "RegistryObjects",
                column: "DE_RepositoryUniqueId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistryObjects_DE_SourcePatientInfoPatientId_DE_SourcePatientInfoPatientSystem",
                table: "RegistryObjects",
                columns: new[] { "DE_SourcePatientInfoPatientId", "DE_SourcePatientInfoPatientSystem" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistryObjects_DE_UniqueId",
                table: "RegistryObjects",
                column: "DE_UniqueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistryObjects_Id",
                table: "RegistryObjects",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentEntry_Authors");

            migrationBuilder.DropTable(
                name: "DocumentEntry_ConfidentialityCodes");

            migrationBuilder.DropTable(
                name: "SubmissionSet_Authors");

            migrationBuilder.DropTable(
                name: "RegistryObjects");
        }
    }
}
