using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoupDiscover.Database.SqliteData
{
    /// <inheritdoc />
    public partial class sqlitemigration564 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Repositories_Credentials_SshKeyId",
                table: "Repositories");

            migrationBuilder.RenameColumn(
                name: "SshKeyId",
                table: "Repositories",
                newName: "CredentialId");

            migrationBuilder.RenameIndex(
                name: "IX_Repositories_SshKeyId",
                table: "Repositories",
                newName: "IX_Repositories_CredentialId");

            migrationBuilder.RenameColumn(
                name: "Licence",
                table: "Packages",
                newName: "RepositoryUrl");

            migrationBuilder.AddColumn<string>(
                name: "License",
                table: "Packages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LicenseType",
                table: "Packages",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProjectUrl",
                table: "Packages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepositoryCommit",
                table: "Packages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepositoryType",
                table: "Packages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CredentialType",
                table: "Credentials",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Login",
                table: "Credentials",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Credentials",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "Credentials",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Repositories_Credentials_CredentialId",
                table: "Repositories",
                column: "CredentialId",
                principalTable: "Credentials",
                principalColumn: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Repositories_Credentials_CredentialId",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "License",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "LicenseType",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ProjectUrl",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "RepositoryCommit",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "RepositoryType",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "CredentialType",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "Login",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Credentials");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "Credentials");

            migrationBuilder.RenameColumn(
                name: "CredentialId",
                table: "Repositories",
                newName: "SshKeyId");

            migrationBuilder.RenameIndex(
                name: "IX_Repositories_CredentialId",
                table: "Repositories",
                newName: "IX_Repositories_SshKeyId");

            migrationBuilder.RenameColumn(
                name: "RepositoryUrl",
                table: "Packages",
                newName: "Licence");

            migrationBuilder.AddForeignKey(
                name: "FK_Repositories_Credentials_SshKeyId",
                table: "Repositories",
                column: "SshKeyId",
                principalTable: "Credentials",
                principalColumn: "Name");
        }
    }
}
