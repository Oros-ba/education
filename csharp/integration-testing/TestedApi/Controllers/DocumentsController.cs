using Microsoft.AspNetCore.Mvc;

namespace TestedApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("DocumentController is working");
}