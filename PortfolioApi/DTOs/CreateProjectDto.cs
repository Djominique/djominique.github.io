using System.ComponentModel.DataAnnotations;

namespace PortfolioApi.DTOs;

public class CreateProjectDto
{
    [Required(ErrorMessage = "Titel is verplicht.")]
    public string Titel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Beschrijving is verplicht.")]
    public string Beschrijving { get; set; } = string.Empty;

    [Required(ErrorMessage = "Categorie is verplicht.")]
    public string Categorie { get; set; } = string.Empty;

    [Required(ErrorMessage = "GitHubUrl is verplicht.")]
    [Url(ErrorMessage = "Voer een geldige URL in.")]
    public string GitHubUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Datum is verplicht.")]
    public DateTime? Datum { get; set; }
}
