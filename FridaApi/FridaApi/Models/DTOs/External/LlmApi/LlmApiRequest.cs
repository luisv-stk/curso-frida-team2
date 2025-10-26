using System.Text.Json.Serialization;

namespace FridaApi.Models.DTOs.External.LlmApi
{
  public class LlmApiRequest
  {
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public LlmRequestMessage[] Messages { get; set; } = Array.Empty<LlmRequestMessage>();

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("tools")]
    public LlmTool[]? Tools { get; set; }

    [JsonPropertyName("enable_caching")]
    public bool? EnableCaching { get; set; }

    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
  }

  public class LlmRequestMessage
  {
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public object Content { get; set; } = string.Empty;
  }

  public class LlmContentItem
  {
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("image_url")]
    public LlmImageUrl? ImageUrl { get; set; }
  }

  public class LlmImageUrl
  {
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
  }

  public class LlmTool
  {
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public LlmParameter[]? Parameters { get; set; }
  }

  public class LlmParameter
  {
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("required")]
    public bool Required { get; set; }
  }
}
