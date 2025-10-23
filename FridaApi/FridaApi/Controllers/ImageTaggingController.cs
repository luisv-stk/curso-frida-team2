using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace FridaApi.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ImageTaggingController : ControllerBase
  {
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageTaggingController> _logger;
    private const string LLM_API_BASE_URL = "https://frida-llm-api.azurewebsites.net";
    private const string BEARER_TOKEN = "zIr4s4V6KDRXE8DvcH38";

    public ImageTaggingController(HttpClient httpClient, ILogger<ImageTaggingController> logger)
    {
      _httpClient = httpClient;
      _logger = logger;
    }

    [HttpPost("generate-tags")]
    public async Task<IActionResult> GenerateTags(IFormFile image)
    {
      try
      {
        if (image == null || image.Length == 0)
        {
          return BadRequest("No image file provided.");
        }

        // Validate image file type
        string[] allowedTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp" };
        if (!allowedTypes.Contains(image.ContentType.ToLower()))
        {
          return BadRequest("Invalid image format. Supported formats: JPEG, PNG, GIF, BMP.");
        }

        // Convert image to base64
        string base64Image;
        using (var memoryStream = new MemoryStream())
        {
          await image.CopyToAsync(memoryStream);
          var imageBytes = memoryStream.ToArray();
          base64Image = Convert.ToBase64String(imageBytes);
        }

        _logger.LogInformation("Image converted to base64. Size: {ImageSize} bytes", image.Length);

        // Prepare the request payload for the LLM API using a simple string approach
        var requestJson = $$"""
        {
          "model": "gpt-5",
          "messages": [
            {
              "role": "user",
              "content": [
                {
                  "type": "text",
                  "text": "Analyze this image and provide appropriate tags that describe its content. Return only a comma-separated list of relevant tags without any additional text or explanation."
                },
                {
                  "type": "image_url",
                  "image_url": {
                    "url": "data:{{image.ContentType}};base64,{{base64Image}}"
                  }
                }
              ]
            }
          ]
        }
        """;

        // Send request to LLM API
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", BEARER_TOKEN);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _logger.LogInformation("Sending request to LLM API: {Url}", $"{LLM_API_BASE_URL}/v1/chat/completions");

        var response = await _httpClient.PostAsync($"{LLM_API_BASE_URL}/v1/chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
          var errorContent = await response.Content.ReadAsStringAsync();
          _logger.LogError("LLM API request failed. Status: {StatusCode}, Response: {Response}", response.StatusCode, errorContent);
          return StatusCode((int)response.StatusCode, "Failed to analyze image with LLM API.");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("LLM API response received successfully");

        // Parse the response from the LLM API
        var llmResponse = JsonSerializer.Deserialize<LlmApiResponse>(responseContent);

        if (llmResponse?.Choices?.Length > 0 && !string.IsNullOrEmpty(llmResponse.Choices[0]?.Message?.Content))
        {
          var tagsText = llmResponse.Choices[0].Message.Content.Trim();
          var tags = tagsText.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(tag => tag.Trim())
                            .Where(tag => !string.IsNullOrEmpty(tag))
                            .ToArray();

          var result = new ImageTagsResponse
          {
            Tags = tags,
            ImageSize = image.Length,
            ProcessedAt = DateTime.UtcNow
          };

          _logger.LogInformation("Successfully generated {TagCount} tags for image", tags.Length);
          return Ok(result);
        }
        else
        {
          _logger.LogWarning("LLM API returned empty or invalid response");
          return BadRequest("Failed to extract tags from LLM response.");
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occurred while processing the image");
        return StatusCode(500, "An internal server error occurred while processing the image.");
      }
    }
  }

  // Response models
  public class ImageTagsResponse
  {
    public string[] Tags { get; set; } = Array.Empty<string>();
    public long ImageSize { get; set; }
    public DateTime ProcessedAt { get; set; }
  }

  public class LlmApiResponse
  {
    public LlmChoice[]? Choices { get; set; }
  }

  public class LlmChoice
  {
    public LlmMessage? Message { get; set; }
  }

  public class LlmMessage
  {
    public string? Content { get; set; }
  }
}
