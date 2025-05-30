using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mystikweb.Auth.Demo.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AddressBook");

            migrationBuilder.CreateTable(
                name: "person",
                schema: "AddressBook",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    last_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    timestamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    insert_by = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    insert_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    update_by = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    update_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_person_id", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "address",
                schema: "AddressBook",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_1 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    line_2 = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    city = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    state = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    country = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    insert_by = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    insert_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    update_by = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    update_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_address_id", x => x.id);
                    table.ForeignKey(
                        name: "fk_person_address",
                        column: x => x.person_id,
                        principalSchema: "AddressBook",
                        principalTable: "person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_address_person_id",
                schema: "AddressBook",
                table: "address",
                column: "person_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "address",
                schema: "AddressBook");

            migrationBuilder.DropTable(
                name: "person",
                schema: "AddressBook");
        }
    }
}
