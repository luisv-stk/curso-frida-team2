using System.Text.Json.Serialization;

namespace FridaApi.Models.DTOs.External.LlmApi
{
  public class LlmApiResponse
  {
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("choices")]
    public LlmChoice[]? Choices { get; set; }

    [JsonPropertyName("usage")]
    public LlmUsage? Usage { get; set; }
  }

  public class LlmChoice
  {
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public LlmMessage? Message { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
  }

  public class LlmMessage
  {
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }
  }

  public class LlmUsage
  {
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }

    [JsonPropertyName("cache_read_input_tokens")]
    public int? CacheReadInputTokens { get; set; }

    [JsonPropertyName("cache_write_input_tokens")]
    public int? CacheWriteInputTokens { get; set; }
  }
}
