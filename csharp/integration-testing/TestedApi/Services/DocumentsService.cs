using System.Collections.Generic;
using System.Linq;
using TestedApi.Model;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TestedApi.Services;

public class DocumentsService
{
    private readonly DocumentsDbContext _dbContext;

    public DocumentsService(DocumentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<Document> GetAllDocuments()
    {
        return _dbContext.Documents.ToList();
    }

    public Document? GetDocumentById(int id)
    {
        return _dbContext.Documents.SingleOrDefault(d => d.Id == id);
    
    }

    public Document CreateDocument(Document document)
    {
        _dbContext.Documents.Add(document);
        _dbContext.SaveChanges();

        return document;
    }

    public Document UpdateDocument(Document document)
    {
        _dbContext.Entry(document).State = EntityState.Modified;
        _dbContext.SaveChanges();

        return document;
    }

    public void DeleteDocument(int id)
    {
        var document = _dbContext.Documents.Find(id);
        if (document != null)
        {
            _dbContext.Documents.Remove(document);
            _dbContext.SaveChanges();
        }
    }
}