using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_Driver_CreatedAt",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Area_Active",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CitizenId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CitizenIdBackUrl",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CitizenIdFrontUrl",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AreaCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CitizenId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.RenameIndex(
                name: "UX_AspNetUsers_EmployeeCode",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_EmployeeCode");

            migrationBuilder.AlterColumn<string>(
                name: "CitizenIdFrontUrl",
                table: "DriverProfiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CitizenIdBackUrl",
                table: "DriverProfiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "DriverProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AreaCode",
                table: "DriverProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenId",
                table: "DriverProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "DriverProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverLicenseBackUrl",
                table: "DriverProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverLicenseClass",
                table: "DriverProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverLicenseFrontUrl",
                table: "DriverProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DriverLicenseIssuedDate",
                table: "DriverProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "DriverProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "DriverProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ContractCount",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedAt",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriverProfiles_Area_Active",
                table: "DriverProfiles",
                columns: new[] { "AreaCode", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverProfiles_CitizenId",
                table: "DriverProfiles",
                column: "CitizenId",
                filter: "[CitizenId] IS NOT NULL AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_DriverProfiles_Name_Code",
                table: "DriverProfiles",
                columns: new[] { "FullName", "DriverCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Driver_LastUsedAt",
                table: "Customers",
                columns: new[] { "CreatedByDriverId", "LastUsedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Driver_PhoneNumber",
                table: "Customers",
                columns: new[] { "CreatedByDriverId", "PhoneNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsActive",
                table: "AspNetUsers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PhoneNumber",
                table: "AspNetUsers",
                column: "PhoneNumber",
                filter: "[PhoneNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DriverProfiles_Area_Active",
                table: "DriverProfiles");

            migrationBuilder.DropIndex(
                name: "IX_DriverProfiles_CitizenId",
                table: "DriverProfiles");

            migrationBuilder.DropIndex(
                name: "IX_DriverProfiles_Name_Code",
                table: "DriverProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Driver_LastUsedAt",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Driver_PhoneNumber",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IsActive",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PhoneNumber",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "AreaCode",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "CitizenId",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "DriverLicenseBackUrl",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "DriverLicenseClass",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "DriverLicenseFrontUrl",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "DriverLicenseIssuedDate",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "ContractCount",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LastUsedAt",
                table: "Customers");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_EmployeeCode",
                table: "AspNetUsers",
                newName: "UX_AspNetUsers_EmployeeCode");

            migrationBuilder.AlterColumn<string>(
                name: "CitizenIdFrontUrl",
                table: "DriverProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CitizenIdBackUrl",
                table: "DriverProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenIdBackUrl",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenIdFrontUrl",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AreaCode",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenId",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Driver_CreatedAt",
                table: "Customers",
                columns: new[] { "CreatedByDriverId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Area_Active",
                table: "AspNetUsers",
                columns: new[] { "AreaCode", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CitizenId",
                table: "AspNetUsers",
                column: "CitizenId",
                filter: "[CitizenId] IS NOT NULL");
        }
    }
}
