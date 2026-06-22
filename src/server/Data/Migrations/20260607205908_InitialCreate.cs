using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskSystem.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    category = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    first_name = table.Column<string>(type: "TEXT", nullable: false),
                    last_name = table.Column<string>(type: "TEXT", nullable: false),
                    email = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    categoryId = table.Column<string>(type: "TEXT", nullable: true),
                    title = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    creator = table.Column<string>(type: "TEXT", nullable: true),
                    assignee = table.Column<string>(type: "TEXT", nullable: true),
                    createDate = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false),
                    dueDate = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_Tasks_Categories_categoryId",
                        column: x => x.categoryId,
                        principalTable: "Categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tasks_User_assignee",
                        column: x => x.assignee,
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tasks_User_creator",
                        column: x => x.creator,
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_category",
                table: "Categories",
                column: "category",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_tasks_assignee",
                table: "Tasks",
                column: "assignee");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_categoryId",
                table: "Tasks",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_creator",
                table: "Tasks",
                column: "creator");

            migrationBuilder.CreateIndex(
                name: "idx_user_email",
                table: "User",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
