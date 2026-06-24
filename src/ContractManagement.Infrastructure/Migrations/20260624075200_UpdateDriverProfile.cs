using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDriverProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DriverProfiles_Name_Code",
                table: "DriverProfiles");

            migrationBuilder.DropIndex(
                name: "UX_DriverProfiles_DriverCode",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "DriverCode",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "DriverProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriverCode",
                table: "DriverProfiles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "DriverProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DriverProfiles_Name_Code",
                table: "DriverProfiles",
                columns: new[] { "FullName", "DriverCode" });

            migrationBuilder.CreateIndex(
                name: "UX_DriverProfiles_DriverCode",
                table: "DriverProfiles",
                column: "DriverCode",
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
