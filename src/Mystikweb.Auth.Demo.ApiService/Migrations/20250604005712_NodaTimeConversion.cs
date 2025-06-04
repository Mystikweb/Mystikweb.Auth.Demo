using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Mystikweb.Auth.Demo.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class NodaTimeConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<LocalDateTime>(
                name: "birth_date",
                schema: "AddressBook",
                table: "person",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "birth_date",
                schema: "AddressBook",
                table: "person",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(LocalDateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }
    }
}
