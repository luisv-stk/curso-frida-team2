namespace FridaApi.Models.DTOs.Api.Responses
{
  public class ImageTagsResponse
  {
    public string[] Tags { get; set; } = Array.Empty<string>();
    public long ImageSize { get; set; }
    public DateTime ProcessedAt { get; set; }
  }
}
