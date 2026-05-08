using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Clients.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientTagsManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE tags DROP CONSTRAINT IF EXISTS "FK_tags_clients_ClientId";
                DROP INDEX IF EXISTS "IX_tags_ClientId";
                ALTER TABLE tags DROP COLUMN IF EXISTS "ClientId";

                CREATE TABLE IF NOT EXISTS client_tags (
                    "ClientId" uuid NOT NULL,
                    "TagsId" uuid NOT NULL,
                    CONSTRAINT "PK_client_tags" PRIMARY KEY ("ClientId", "TagsId"),
                    CONSTRAINT "FK_client_tags_clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES clients ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_client_tags_tags_TagsId" FOREIGN KEY ("TagsId") REFERENCES tags ("Id") ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS "IX_client_tags_TagsId" ON client_tags ("TagsId");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS client_tags;
                ALTER TABLE tags ADD COLUMN IF NOT EXISTS "ClientId" uuid;
                CREATE INDEX IF NOT EXISTS "IX_tags_ClientId" ON tags ("ClientId");

                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_tags_clients_ClientId') THEN
                        ALTER TABLE tags ADD CONSTRAINT "FK_tags_clients_ClientId"
                            FOREIGN KEY ("ClientId") REFERENCES clients ("Id");
                    END IF;
                END $$;
                """);
        }
    }
}
