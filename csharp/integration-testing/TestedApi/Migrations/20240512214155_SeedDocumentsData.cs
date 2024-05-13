using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestedApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedDocumentsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT Documents ON;

                INSERT INTO Documents (Id, Title, Author) 
                VALUES 
                (1, 'Document 1', 'Author of Document 1'),
                (2, 'Document 2', 'Author of Document 2'),
                (3, 'Document 3', 'Author of Document 3'),
                (4, 'Document 4', 'Author of Document 4'),
                (5, 'Document 5', 'Author of Document 5');

                SET IDENTITY_INSERT Documents OFF;
            ");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) 
        { 
            migrationBuilder.Sql("DELETE FROM Documents WHERE Id IN (1, 2, 3, 4, 5)"); 
        }
    }
}
