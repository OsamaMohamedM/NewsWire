using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsWire.Migrations
{
    /// <inheritdoc/>
    public partial class add_profile_picture : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "profilePictureUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "profilePictureUrl",
                table: "AspNetUsers");
        }
    }
}