using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestedApi.Model;

public class Document
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public required string Title { get; set; }

 
    public string? Content { get; set; }

    public string? Status { get; set; }

    [Required]
    public required string Author { get; set; }
}