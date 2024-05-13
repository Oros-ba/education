using Microsoft.EntityFrameworkCore;

namespace TestedApi.Model;

public class DocumentsDbContext(DbContextOptions<DocumentsDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents { get; set; }
}