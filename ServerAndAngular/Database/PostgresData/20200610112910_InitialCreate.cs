using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace SoupDiscover.Database.PostgresData
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Credentials",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Key = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PackageId = table.Column<string>(nullable: true),
                    Version = table.Column<string>(nullable: true),
                    Licence = table.Column<string>(nullable: true),
                    PackageType = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Repositories",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    Branch = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    SshKeyId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repositories", x => x.Name);
                    table.ForeignKey(
                        name: "FK_Repositories_Credentials_SshKeyId",
                        column: x => x.SshKeyId,
                        principalTable: "Credentials",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    RepositoryId = table.Column<string>(nullable: false),
                    CommandLinesBeforeParse = table.Column<string>(nullable: true),
                    NugetServerUrl = table.Column<string>(nullable: true),
                    LastAnalysisError = table.Column<string>(nullable: true),
                    LastAnalysisDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Name);
                    table.ForeignKey(
                        name: "FK_Projects_Repositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "Repositories",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageConsumer",
                columns: table => new
                {
                    PackageConsumerId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    PackageId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageConsumer", x => x.PackageConsumerId);
                    table.ForeignKey(
                        name: "FK_PackageConsumer_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageConsumer_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackageConsumerPackages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PackageConsumerId = table.Column<int>(nullable: false),
                    PackageId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageConsumerPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageConsumerPackages_PackageConsumer_PackageConsumerId",
                        column: x => x.PackageConsumerId,
                        principalTable: "PackageConsumer",
                        principalColumn: "PackageConsumerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageConsumerPackages_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackageConsumer_PackageId",
                table: "PackageConsumer",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageConsumer_ProjectId",
                table: "PackageConsumer",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageConsumerPackages_PackageConsumerId",
                table: "PackageConsumerPackages",
                column: "PackageConsumerId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageConsumerPackages_PackageId",
                table: "PackageConsumerPackages",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_PackageId",
                table: "Packages",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_RepositoryId",
                table: "Projects",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Repositories_SshKeyId",
                table: "Repositories",
                column: "SshKeyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageConsumerPackages");

            migrationBuilder.DropTable(
                name: "PackageConsumer");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Repositories");

            migrationBuilder.DropTable(
                name: "Credentials");
        }
    }
}
