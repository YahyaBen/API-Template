using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace foodiya.app.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePushRegistrationWithFirebaseInstallation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PushRegistrationId",
                table: "UserDevices");

            migrationBuilder.AddColumn<string>(
                name: "FirebaseInstallationId",
                table: "UserDevices",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FirebaseInstallationUpdatedAtUtc",
                table: "UserDevices",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_FirebaseInstallationId",
                table: "UserDevices",
                column: "FirebaseInstallationId",
                unique: true,
                filter: "[FirebaseInstallationId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserDevices_FirebaseInstallationId",
                table: "UserDevices");

            migrationBuilder.DropColumn(
                name: "FirebaseInstallationId",
                table: "UserDevices");

            migrationBuilder.DropColumn(
                name: "FirebaseInstallationUpdatedAtUtc",
                table: "UserDevices");

            migrationBuilder.AddColumn<string>(
                name: "PushRegistrationId",
                table: "UserDevices",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
