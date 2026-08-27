using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SB.Solicitudes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkItemActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ResponsableNuevoId",
                table: "HistorialAsignaciones",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "ActividadesSolicitud",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Campo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ValorAnterior = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ValorNuevo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActividadesSolicitud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActividadesSolicitud_Solicitudes_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitudes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActividadesSolicitud_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActividadesSolicitud_SolicitudId_Fecha",
                table: "ActividadesSolicitud",
                columns: new[] { "SolicitudId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_ActividadesSolicitud_UsuarioId",
                table: "ActividadesSolicitud",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActividadesSolicitud");

            migrationBuilder.AlterColumn<Guid>(
                name: "ResponsableNuevoId",
                table: "HistorialAsignaciones",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
