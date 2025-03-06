using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EchoTrace.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMonitoringProjectApiField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cron_expression",
                table: "monitoring_project_api");

            migrationBuilder.AddColumn<int>(
                name: "monitor_interval",
                table: "monitoring_project_api",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "monitor_interval",
                table: "monitoring_project_api");

            migrationBuilder.AddColumn<string>(
                name: "cron_expression",
                table: "monitoring_project_api",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
