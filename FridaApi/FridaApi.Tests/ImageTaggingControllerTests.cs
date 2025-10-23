using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using FridaApi.Controllers;

namespace FridaApi.Tests
{
  public class ImageTaggingControllerTests
  {
    private readonly Mock<ILogger<ImageTaggingController>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly ImageTaggingController _controller;

    public ImageTaggingControllerTests()
    {
      _mockLogger = new Mock<ILogger<ImageTaggingController>>();
      _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
      _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
      _controller = new ImageTaggingController(_httpClient, _mockLogger.Object);
    }

    [Fact]
    public async Task GenerateTags_WithNullImage_ReturnsBadRequest()
    {
      // Act
      var result = await _controller.GenerateTags(null);

      // Assert
      var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
      Assert.Equal("No image file provided.", badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTags_WithEmptyImage_ReturnsBadRequest()
    {
      // Arrange
      var mockFile = new Mock<IFormFile>();
      mockFile.Setup(f => f.Length).Returns(0);

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
      Assert.Equal("No image file provided.", badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTags_WithInvalidImageType_ReturnsBadRequest()
    {
      // Arrange
      var mockFile = new Mock<IFormFile>();
      mockFile.Setup(f => f.Length).Returns(1000);
      mockFile.Setup(f => f.ContentType).Returns("text/plain");

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
      Assert.Equal("Invalid image format. Supported formats: JPEG, PNG, GIF, BMP.", badRequestResult.Value);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/gif")]
    [InlineData("image/bmp")]
    public async Task GenerateTags_WithValidImageType_ProcessesSuccessfully(string contentType)
    {
      // Arrange
      var imageContent = "fake image content";
      var imageBytes = Encoding.UTF8.GetBytes(imageContent);
      var stream = new MemoryStream(imageBytes);

      var mockFile = new Mock<IFormFile>();
      mockFile.Setup(f => f.Length).Returns(imageBytes.Length);
      mockFile.Setup(f => f.ContentType).Returns(contentType);
      mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
             .Callback<Stream, CancellationToken>((s, ct) => stream.CopyTo(s))
             .Returns(Task.CompletedTask);

      // Mock successful LLM API response
      var llmResponse = new LlmApiResponse
      {
        Choices = new[]
          {
                    new LlmChoice
                    {
                        Message = new LlmMessage
                        {
                            Content = "nature, landscape, mountains, sky"
                        }
                    }
                }
      };

      var jsonResponse = JsonSerializer.Serialize(llmResponse);
      var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
      };

      _mockHttpMessageHandler.Protected()
          .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(httpResponse);

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var okResult = Assert.IsType<OkObjectResult>(result);
      var response = Assert.IsType<ImageTagsResponse>(okResult.Value);
      Assert.Equal(4, response.Tags.Length);
      Assert.Contains("nature", response.Tags);
      Assert.Contains("landscape", response.Tags);
      Assert.Contains("mountains", response.Tags);
      Assert.Contains("sky", response.Tags);
      Assert.Equal(imageBytes.Length, response.ImageSize);
    }

    [Fact]
    public async Task GenerateTags_WithLlmApiError_ReturnsStatusCodeResult()
    {
      // Arrange
      var imageContent = "fake image content";
      var imageBytes = Encoding.UTF8.GetBytes(imageContent);
      var stream = new MemoryStream(imageBytes);

      var mockFile = new Mock<IFormFile>();
      mockFile.Setup(f => f.Length).Returns(imageBytes.Length);
      mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
      mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
             .Callback<Stream, CancellationToken>((s, ct) => stream.CopyTo(s))
             .Returns(Task.CompletedTask);

      // Mock failed LLM API response
      var httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized)
      {
        Content = new StringContent("Unauthorized", Encoding.UTF8, "text/plain")
      };

      _mockHttpMessageHandler.Protected()
          .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(httpResponse);

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var statusCodeResult = Assert.IsType<ObjectResult>(result);
      Assert.Equal(401, statusCodeResult.StatusCode);
      Assert.Equal("Failed to analyze image with LLM API.", statusCodeResult.Value);
    }

    [Fact]
    public async Task GenerateTags_WithEmptyLlmResponse_ReturnsBadRequest()
    {
      // Arrange
      var imageContent = "fake image content";
      var imageBytes = Encoding.UTF8.GetBytes(imageContent);
      var stream = new MemoryStream(imageBytes);

      var mockFile = new Mock<IFormFile>();
      mockFile.Setup(f => f.Length).Returns(imageBytes.Length);
      mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
      mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
             .Callback<Stream, CancellationToken>((s, ct) => stream.CopyTo(s))
             .Returns(Task.CompletedTask);

      // Mock LLM API response with empty content
      var llmResponse = new LlmApiResponse
      {
        Choices = new[]
          {
                    new LlmChoice
                    {
                        Message = new LlmMessage
                        {
                            Content = ""
                        }
                    }
                }
      };

      var jsonResponse = JsonSerializer.Serialize(llmResponse);
      var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
      };

      _mockHttpMessageHandler.Protected()
          .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(httpResponse);

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
      Assert.Equal("Failed to extract tags from LLM response.", badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTags_VerifiesCorrectApiRequest()
    {
      // Arrange
      var imageContent = "fake image content";
      var imageBytes = Encoding.UTF8.GetBytes(imageContent);
      var stream = new MemoryStream(imageBytes);
      var expectedBase64 = Convert.ToBase64String(imageBytes);

      var mockFile = new Mock<IFormFile>();
      mockFile.Setup(f => f.Length).Returns(imageBytes.Length);
      mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
      mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
             .Callback<Stream, CancellationToken>((s, ct) => stream.CopyTo(s))
             .Returns(Task.CompletedTask);

      var llmResponse = new LlmApiResponse
      {
        Choices = new[]
          {
                    new LlmChoice
                    {
                        Message = new LlmMessage
                        {
                            Content = "test, tag"
                        }
                    }
                }
      };

      var jsonResponse = JsonSerializer.Serialize(llmResponse);
      var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
      };

      HttpRequestMessage capturedRequest = null;
      _mockHttpMessageHandler.Protected()
          .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
          .ReturnsAsync(httpResponse);

      // Act
      await _controller.GenerateTags(mockFile.Object);

      // Assert
      Assert.NotNull(capturedRequest);
      Assert.Equal(HttpMethod.Post, capturedRequest.Method);
      Assert.Equal("https://frida-llm-api.azurewebsites.net/v1/chat/completions", capturedRequest.RequestUri.ToString());
      Assert.Equal("Bearer", capturedRequest.Headers.Authorization.Scheme);
      Assert.Equal("zIr4s4V6KDRXE8DvcH38", capturedRequest.Headers.Authorization.Parameter);

      var requestContent = await capturedRequest.Content.ReadAsStringAsync();
      Assert.Contains($"data:image/jpeg;base64,{expectedBase64}", requestContent);
      Assert.Contains("gpt-4o-mini", requestContent);
      Assert.Contains("Analyze this image and provide appropriate tags", requestContent);
    }

    public void Dispose()
    {
      _httpClient?.Dispose();
    }
  }
}
