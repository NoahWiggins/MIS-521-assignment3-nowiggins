using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MIS_521_assignment3_nowiggins.Data.Migrations
{
    /// <inheritdoc />
    public partial class Start : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Actor",
                columns: table => new
                {
                    ActorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActorGender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActorAge = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActorIMDBHyperlink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActorPhoto = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actor", x => x.ActorId);
                });

            migrationBuilder.CreateTable(
                name: "Movie",
                columns: table => new
                {
                    MovieId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovieTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MovieGenre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YearOfRelease = table.Column<DateOnly>(type: "date", nullable: false),
                    MovieIMDBHyperlink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MoviePoster = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movie", x => x.MovieId);
                });

            migrationBuilder.CreateTable(
                name: "ActorMovie",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorId = table.Column<int>(type: "int", nullable: true),
                    MovieId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActorMovie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActorMovie_Actor_ActorId",
                        column: x => x.ActorId,
                        principalTable: "Actor",
                        principalColumn: "ActorId");
                    table.ForeignKey(
                        name: "FK_ActorMovie_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "MovieId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActorMovie_ActorId",
                table: "ActorMovie",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_ActorMovie_MovieId",
                table: "ActorMovie",
                column: "MovieId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActorMovie");

            migrationBuilder.DropTable(
                name: "Actor");

            migrationBuilder.DropTable(
                name: "Movie");
        }
    }
}
