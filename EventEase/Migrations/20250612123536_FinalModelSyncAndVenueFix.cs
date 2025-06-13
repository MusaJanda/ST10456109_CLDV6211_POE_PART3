using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventEase.Migrations
{
    /// <inheritdoc />
    public partial class FinalModelSyncAndVenueFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // IMPORTANT: Add this SQL command to update existing NULL values
            // This sets any existing NULL AvailabilityStatus values to 'Available'.
            migrationBuilder.Sql("UPDATE Venues SET AvailabilityStatus = 'Available' WHERE AvailabilityStatus IS NULL;");

            // This is the EF Core generated code that makes the column NOT NULL
            // and sets the default value for new rows to an empty string.
            migrationBuilder.AlterColumn<string>(
                name: "AvailabilityStatus",
                table: "Venues",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "", // This means new rows will default to ""
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No change needed here, this reverts the column to nullable.
            migrationBuilder.AlterColumn<string>(
                name: "AvailabilityStatus",
                table: "Venues",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}