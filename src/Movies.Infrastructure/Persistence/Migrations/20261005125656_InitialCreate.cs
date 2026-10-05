using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Movies.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "actors",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tmdb_person_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false, collation: "und-x-icu"),
                    search_name = table.Column<string>(type: "text", nullable: false),
                    profile_path = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_actors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "genres",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_genres", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "movies",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false, collation: "und-x-icu"),
                    search_title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    overview = table.Column<string>(type: "text", nullable: false),
                    release_date = table.Column<DateOnly>(type: "date", nullable: false),
                    popularity = table.Column<double>(type: "double precision", nullable: false),
                    vote_count = table.Column<int>(type: "integer", nullable: false),
                    vote_average = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: false),
                    original_language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    poster_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    tmdb_id = table.Column<int>(type: "integer", nullable: true),
                    cast_synced_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_movies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cast_members",
                columns: table => new
                {
                    movie_id = table.Column<int>(type: "integer", nullable: false),
                    actor_id = table.Column<int>(type: "integer", nullable: false),
                    character = table.Column<string>(type: "text", nullable: true),
                    billing_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cast_members", x => new { x.movie_id, x.actor_id });
                    table.ForeignKey(
                        name: "fk_cast_members_actors_actor_id",
                        column: x => x.actor_id,
                        principalTable: "actors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cast_members_movies_movie_id",
                        column: x => x.movie_id,
                        principalTable: "movies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movie_genres",
                columns: table => new
                {
                    genres_id = table.Column<int>(type: "integer", nullable: false),
                    movies_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_movie_genres", x => new { x.genres_id, x.movies_id });
                    table.ForeignKey(
                        name: "fk_movie_genres_genres_genres_id",
                        column: x => x.genres_id,
                        principalTable: "genres",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_movie_genres_movies_movies_id",
                        column: x => x.movies_id,
                        principalTable: "movies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_actors_search_name",
                table: "actors",
                column: "search_name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_actors_tmdb_person_id",
                table: "actors",
                column: "tmdb_person_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cast_members_actor_id",
                table: "cast_members",
                column: "actor_id");

            migrationBuilder.CreateIndex(
                name: "ix_genres_name",
                table: "genres",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_movie_genres_movies_id",
                table: "movie_genres",
                column: "movies_id");

            migrationBuilder.CreateIndex(
                name: "ix_movies_popularity",
                table: "movies",
                column: "popularity");

            migrationBuilder.CreateIndex(
                name: "ix_movies_release_date",
                table: "movies",
                column: "release_date");

            migrationBuilder.CreateIndex(
                name: "ix_movies_search_title",
                table: "movies",
                column: "search_title")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_movies_title",
                table: "movies",
                column: "title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cast_members");

            migrationBuilder.DropTable(
                name: "movie_genres");

            migrationBuilder.DropTable(
                name: "actors");

            migrationBuilder.DropTable(
                name: "genres");

            migrationBuilder.DropTable(
                name: "movies");
        }
    }
}
