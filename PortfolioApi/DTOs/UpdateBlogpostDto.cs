using System.ComponentModel.DataAnnotations;

namespace PortfolioApi.DTOs;

public class UpdateBlogpostDto
{
    [Required(ErrorMessage = "Titel is verplicht.")]
    public string Titel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Inhoud is verplicht.")]
    public string Inhoud { get; set; } = string.Empty;

    [Required(ErrorMessage = "Publicatiedatum is verplicht.")]
    public DateTime? Publicatiedatum { get; set; }
}
