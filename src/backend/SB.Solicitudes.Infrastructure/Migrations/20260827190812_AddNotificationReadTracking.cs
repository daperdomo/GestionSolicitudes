using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SB.Solicitudes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationReadTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FechaLectura",
                table: "Notificaciones",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_DestinatarioId_FechaCreacion",
                table: "Notificaciones",
                columns: new[] { "DestinatarioId", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_DestinatarioId_FechaLectura",
                table: "Notificaciones",
                columns: new[] { "DestinatarioId", "FechaLectura" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notificaciones_DestinatarioId_FechaCreacion",
                table: "Notificaciones");

            migrationBuilder.DropIndex(
                name: "IX_Notificaciones_DestinatarioId_FechaLectura",
                table: "Notificaciones");

            migrationBuilder.DropColumn(
                name: "FechaLectura",
                table: "Notificaciones");
        }
    }
}
