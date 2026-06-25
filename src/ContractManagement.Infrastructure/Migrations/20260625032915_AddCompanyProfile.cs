using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    TaxCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BusinessLicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RepresentativeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RepresentativePosition = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RepresentativeCitizenId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RepresentativeCitizenIdIssuedDate = table.Column<DateTime>(type: "date", nullable: true),
                    RepresentativeCitizenIdIssuedPlace = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    ManagedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyProfiles_AspNetUsers_ManagedByUserId",
                        column: x => x.ManagedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProfiles_IsActive",
                table: "CompanyProfiles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProfiles_ManagedByUserId",
                table: "CompanyProfiles",
                column: "ManagedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProfiles_ManagedByUserId_IsDefault",
                table: "CompanyProfiles",
                columns: new[] { "ManagedByUserId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProfiles_TaxCode",
                table: "CompanyProfiles",
                column: "TaxCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyProfiles");
        }
    }
}
