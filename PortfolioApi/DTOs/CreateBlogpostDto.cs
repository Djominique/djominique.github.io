namespace PortfolioApi.DTOs;

public class CreateBlogpostDto
{
    public string Titel { get; set; } = "";
    public string Inhoud { get; set; } = "";
    public DateTime Publicatiedatum { get; set; }
}