using Microsoft.AspNetCore.Mvc;
using TestedApi.Services;
using TestedApi.Model;

namespace TestedApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentsController(DocumentsService documentService) : ControllerBase
    {

        [HttpGet]
        public IActionResult GetAll()
        {
            var documents = documentService.GetAllDocuments();
            return Ok(documents);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var document = documentService.GetDocumentById(id);
            return document == null ? NotFound() : Ok(document);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Document document)
        {
            var createdDocument = documentService.CreateDocument(document);
            return CreatedAtAction(nameof(Get), new { id = createdDocument.Id }, createdDocument);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Document document)
        {
            if (id != document.Id)
            {
                return BadRequest();
            }
            var updatedDocument = documentService.UpdateDocument(document);
            return Ok(updatedDocument);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            documentService.DeleteDocument(id);
            return NoContent();
        }
    }
}