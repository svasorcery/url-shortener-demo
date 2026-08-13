using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortener.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStoredShortUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortUrl",
                table: "ShortenedUrls");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortUrl",
                table: "ShortenedUrls",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
