using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RubiksCube.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CubeSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Facelets = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CubeSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RotationLog",
                columns: table => new
                {
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Face = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    Rotation = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RotationLog", x => new { x.SessionId, x.Sequence });
                    table.ForeignKey(
                        name: "FK_RotationLog_CubeSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "CubeSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RotationLog");

            migrationBuilder.DropTable(
                name: "CubeSessions");
        }
    }
}
