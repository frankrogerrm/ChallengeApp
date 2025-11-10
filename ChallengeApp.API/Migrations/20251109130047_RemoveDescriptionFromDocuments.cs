using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChallengeApp.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDescriptionFromDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Documents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
