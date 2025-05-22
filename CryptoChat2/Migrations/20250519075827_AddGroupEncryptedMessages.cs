using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoChat2.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupEncryptedMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupEncryptedMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    SenderId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceiverId = table.Column<int>(type: "INTEGER", nullable: false),
                    CipherText = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Nonce = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupEncryptedMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupEncryptedMessages_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupEncryptedMessages_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupEncryptedMessages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupEncryptedMessages_GroupId",
                table: "GroupEncryptedMessages",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupEncryptedMessages_ReceiverId",
                table: "GroupEncryptedMessages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupEncryptedMessages_SenderId",
                table: "GroupEncryptedMessages",
                column: "SenderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupEncryptedMessages");
        }
    }
}
