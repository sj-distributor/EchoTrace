using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EchoTrace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteMonitoringProjectIsActiveField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_active",
                table: "monitoring_project");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "monitoring_project",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
