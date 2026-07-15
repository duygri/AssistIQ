using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssistIQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StoreKnowledgeDocumentText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "text_content",
                table: "knowledge_documents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE knowledge_documents
                SET status = 'Failed',
                    error_summary = 'Document text was not retained by the previous version. Register the document again.',
                    provider_vector_store_id = NULL,
                    provider_file_id = NULL
                WHERE status = 'Ready' AND text_content = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "text_content",
                table: "knowledge_documents");
        }
    }
}
