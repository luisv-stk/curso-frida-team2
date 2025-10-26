using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FridaApi.Models.DTOs.Api.Responses;
using FridaApi.Models.DTOs.External.LlmApi;

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
        if (string.IsNullOrEmpty(image.ContentType) || !allowedTypes.Contains(image.ContentType.ToLower()))
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

        // Prepare the request payload using DTOs
        var request = new LlmApiRequest
        {
          Model = "gpt-5",
          Messages = new[]
          {
            new LlmRequestMessage
            {
              Role = "system",
              Content = new[]
              {
                new LlmContentItem
                {
                  Type = "text",
                  Text = "You are an expert image tagging AI with advanced computer vision capabilities. Your mission: generate comprehensive, searchable metadata tags.\n\n" +
                        "CORE PRINCIPLES:\n" +
                        "• Accuracy over assumptions - only tag what you can clearly observe\n" +
                        "• Specificity over generality (e.g., 'border-collie' not 'dog', 'vintage-1950s-car' not 'vehicle')\n" +
                        "• Balance obvious elements with nuanced details\n" +
                        "• Include both literal and interpretive tags when justified\n\n" +
                        "TAG FORMATTING RULES:\n" +
                        "• Lowercase with hyphens for multi-word concepts\n" +
                        "• No spaces, underscores, or special characters\n" +
                        "• Consistent terminology (standardize similar concepts)\n" +
                        "• 3-30 characters per tag ideal length\n\n" +
                        "OUTPUT: Only comma-separated tags. No explanations, descriptions, or additional text."
                }
              }
            },
            new LlmRequestMessage
            {
              Role = "user",
              Content = new[]
              {
                new LlmContentItem
                {
                  Type = "text",
                  Text = "Generate comprehensive tags for this image. Systematically analyze and tag:\n\n" +
                        "🎯 SUBJECTS & OBJECTS:\n" +
                        "• People: age-group, gender, ethnicity, clothing, expressions, poses, activities\n" +
                        "• Animals: species, breed, age, behavior, position\n" +
                        "• Objects: specific items, brands, materials, conditions, purposes\n" +
                        "• Architecture: building-types, styles, periods, materials\n\n" +
                        "🎨 VISUAL CHARACTERISTICS:\n" +
                        "• Colors: dominant-hues, color-schemes, saturation-levels\n" +
                        "• Lighting: source-type, direction, quality, time-of-day\n" +
                        "• Composition: framing, angles, perspective, balance\n" +
                        "• Style: artistic-movements, techniques, effects\n\n" +
                        "📍 CONTEXT & ENVIRONMENT:\n" +
                        "• Location: indoor/outdoor, specific-venues, geographic-regions\n" +
                        "• Season/weather: climate-conditions, seasonal-indicators\n" +
                        "• Era: historical-periods, cultural-contexts, fashion-eras\n" +
                        "• Activity: events, situations, purposes, functions\n\n" +
                        "🔧 TECHNICAL ASPECTS:\n" +
                        "• Image-type: photography, illustration, digital-art, painting\n" +
                        "• Quality: resolution, clarity, grain, artifacts\n" +
                        "• Camera-work: focal-length, depth-of-field, exposure, techniques\n\n" +
                        "💭 MOOD & INTERPRETATION:\n" +
                        "• Emotional-tone: cheerful, melancholic, energetic, peaceful\n" +
                        "• Atmosphere: formal, casual, dramatic, serene\n" +
                        "• Themes: concepts, messages, symbolism\n\n" +
                        "Aim for 15-40 relevant tags. Prioritize accuracy and searchability."
                },
                new LlmContentItem
                {
                  Type = "image_url",
                  ImageUrl = new LlmImageUrl
                  {
                    Url = $"data:{image.ContentType ?? "application/octet-stream"};base64,{base64Image}"
                  }
                }
              }
            }
          }
        };

        // Send request to LLM API
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", BEARER_TOKEN);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var jsonOptions = new JsonSerializerOptions
        {
          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var requestJson = JsonSerializer.Serialize(request, jsonOptions);
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
        var options = new JsonSerializerOptions
        {
          PropertyNameCaseInsensitive = true
        };
        var llmResponse = JsonSerializer.Deserialize<LlmApiResponse>(responseContent, options);

        if (llmResponse?.Choices?.Length > 0)
        {
          var firstChoice = llmResponse.Choices[0];
          if (firstChoice?.Message?.Content != null && !string.IsNullOrEmpty(firstChoice.Message.Content))
          {
            var messageContent = firstChoice.Message.Content;
            var tagsText = messageContent.Trim();

            // Check if after trimming we have empty content
            if (string.IsNullOrWhiteSpace(tagsText))
            {
              _logger.LogWarning("LLM API returned whitespace-only content");
              return BadRequest("Failed to extract tags from LLM response.");
            }

            var tags = tagsText.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(tag => tag.Trim())
                              .Where(tag => !string.IsNullOrEmpty(tag))
                              .ToArray();

            // Check if we have any valid tags after processing
            if (tags.Length == 0)
            {
              _logger.LogWarning("LLM API returned content that resulted in no valid tags");
              return BadRequest("Failed to extract tags from LLM response.");
            }

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
}
