using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistIQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    entity_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    before_json = table.Column<string>(type: "jsonb", nullable: true),
                    after_json = table.Column<string>(type: "jsonb", nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "drafts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    generated_answer = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    edited_answer = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    edited_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drafts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "knowledge_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    content_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    provider_vector_store_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    provider_file_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    error_summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    uploaded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    indexed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    disabled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_knowledge_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_question = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    customer_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    customer_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    drafted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tickets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usage_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uuid", nullable: true),
                    draft_id = table.Column<Guid>(type: "uuid", nullable: true),
                    provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    model = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    response_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    input_tokens = table.Column<int>(type: "integer", nullable: false),
                    output_tokens = table.Column<int>(type: "integer", nullable: false),
                    total_tokens = table.Column<int>(type: "integer", nullable: false),
                    estimated_cost = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    error_summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usage_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    display_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    disabled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "draft_citations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    draft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    knowledge_document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    provider_file_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    quote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    provider_result_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_draft_citations", x => x.id);
                    table.ForeignKey(
                        name: "FK_draft_citations_drafts_draft_id",
                        column: x => x.draft_id,
                        principalTable: "drafts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_action",
                table: "audit_logs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_entity_name_entity_id",
                table: "audit_logs",
                columns: new[] { "entity_name", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_occurred_at",
                table: "audit_logs",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "IX_draft_citations_draft_id",
                table: "draft_citations",
                column: "draft_id");

            migrationBuilder.CreateIndex(
                name: "IX_draft_citations_knowledge_document_id",
                table: "draft_citations",
                column: "knowledge_document_id");

            migrationBuilder.CreateIndex(
                name: "IX_drafts_status",
                table: "drafts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_drafts_ticket_id_version_number",
                table: "drafts",
                columns: new[] { "ticket_id", "version_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_documents_provider_file_id",
                table: "knowledge_documents",
                column: "provider_file_id");

            migrationBuilder.CreateIndex(
                name: "IX_knowledge_documents_status",
                table: "knowledge_documents",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_created_at",
                table: "tickets",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_tickets_status",
                table: "tickets",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_usage_logs_created_at",
                table: "usage_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_usage_logs_status",
                table: "usage_logs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_usage_logs_ticket_id",
                table: "usage_logs",
                column: "ticket_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "draft_citations");

            migrationBuilder.DropTable(
                name: "knowledge_documents");

            migrationBuilder.DropTable(
                name: "tickets");

            migrationBuilder.DropTable(
                name: "usage_logs");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "drafts");
        }
    }
}
