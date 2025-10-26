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
using FridaApi.Models.DTOs.Api.Responses;
using FridaApi.Models.DTOs.External.LlmApi;

namespace FridaApi.Tests
{
  public class ImageTaggingControllerTests : IDisposable
  {
    #region Test Setup and Constants

    private readonly Mock<ILogger<ImageTaggingController>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly ImageTaggingController _controller;

    // Test constants
    private const string DEFAULT_IMAGE_CONTENT = "fake image content";
    private const string DEFAULT_CONTENT_TYPE = "image/jpeg";
    private const string EXPECTED_ERROR_NO_FILE = "No image file provided.";
    private const string EXPECTED_ERROR_INVALID_FORMAT = "Invalid image format. Supported formats: JPEG, PNG, GIF, BMP.";
    private const string EXPECTED_ERROR_FAILED_EXTRACTION = "Failed to extract tags from LLM response.";
    private const string EXPECTED_ERROR_API_FAILURE = "Failed to analyze image with LLM API.";
    private const string EXPECTED_ERROR_INTERNAL = "An internal server error occurred while processing the image.";

    public ImageTaggingControllerTests()
    {
      _mockLogger = new Mock<ILogger<ImageTaggingController>>();
      _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
      _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
      _controller = new ImageTaggingController(_httpClient, _mockLogger.Object);
    }

    #endregion

    #region Helper Methods

    private Mock<IFormFile> CreateMockImageFile(string contentType = DEFAULT_CONTENT_TYPE, string content = DEFAULT_IMAGE_CONTENT)
    {
      var imageBytes = Encoding.UTF8.GetBytes(content);
      var stream = new MemoryStream(imageBytes);

      var mockFile = new Mock<IFormFile>();
      mockFile.Setup(f => f.Length).Returns(imageBytes.Length);
      mockFile.Setup(f => f.ContentType).Returns(contentType);
      mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
             .Callback<Stream, CancellationToken>((s, ct) =>
             {
               stream.Position = 0;
               stream.CopyTo(s);
             })
             .Returns(Task.CompletedTask);

      return mockFile;
    }

    private void SetupSuccessfulLlmResponse(string tags = "nature, landscape, mountains, sky")
    {
      var llmResponse = new LlmApiResponse
      {
        Choices = new[]
        {
          new LlmChoice
          {
            Message = new LlmMessage
            {
              Content = tags
            }
          }
        }
      };
      SetupHttpResponse(HttpStatusCode.OK, llmResponse);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, object? responseObject = null, string? plainTextResponse = null)
    {
      HttpResponseMessage httpResponse;

      if (responseObject != null)
      {
        var jsonResponse = JsonSerializer.Serialize(responseObject);
        httpResponse = new HttpResponseMessage(statusCode)
        {
          Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
        };
      }
      else
      {
        httpResponse = new HttpResponseMessage(statusCode)
        {
          Content = new StringContent(plainTextResponse ?? "", Encoding.UTF8, "text/plain")
        };
      }

      _mockHttpMessageHandler.Protected()
          .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(httpResponse);
    }

    #endregion

    #region Input Validation Tests

    [Fact]
    public async Task GenerateTags_WhenImageIsNull_ShouldReturnBadRequest()
    {
      // Act
      var result = await _controller.GenerateTags(null!);

      // Assert
      var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
      Assert.Equal(EXPECTED_ERROR_NO_FILE, badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTags_WhenImageIsEmpty_ShouldReturnBadRequest()
    {
      // Arrange
      var mockFile = new Mock<IFormFile>();
      mockFile.Setup(f => f.Length).Returns(0);

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
      Assert.Equal(EXPECTED_ERROR_NO_FILE, badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTags_WhenContentTypeIsInvalid_ShouldReturnBadRequest()
    {
      // Arrange
      var mockFile = new Mock<IFormFile>();
      mockFile.Setup(f => f.Length).Returns(1000);
      mockFile.Setup(f => f.ContentType).Returns("text/plain");

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
      Assert.Equal(EXPECTED_ERROR_INVALID_FORMAT, badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTags_WhenContentTypeIsNull_ShouldReturnBadRequest()
    {
      // Arrange
      var mockFile = new Mock<IFormFile>();
      mockFile.Setup(f => f.Length).Returns(1000);
      mockFile.Setup(f => f.ContentType).Returns(null as string);

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
      Assert.Equal(EXPECTED_ERROR_INVALID_FORMAT, badRequestResult.Value);
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/jpg")]
    [InlineData("image/png")]
    [InlineData("image/gif")]
    [InlineData("image/bmp")]
    public async Task GenerateTags_WhenContentTypeIsValid_ShouldProcessSuccessfully(string contentType)
    {
      // Arrange
      var mockFile = CreateMockImageFile(contentType);
      SetupSuccessfulLlmResponse();

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
      Assert.Equal(Encoding.UTF8.GetBytes(DEFAULT_IMAGE_CONTENT).Length, response.ImageSize);
    }

    [Theory]
    [InlineData("IMAGE/JPEG")]
    [InlineData("Image/PNG")]
    [InlineData("IMAGE/GIF")]
    public async Task GenerateTags_WhenContentTypeHasMixedCase_ShouldProcessSuccessfully(string contentType)
    {
      // Arrange
      var mockFile = CreateMockImageFile(contentType);
      SetupSuccessfulLlmResponse("test, tag");

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var okResult = Assert.IsType<OkObjectResult>(result);
      Assert.IsType<ImageTagsResponse>(okResult.Value);
    }

    #endregion

    #region LLM API Response Processing Tests

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task GenerateTags_WhenLlmApiReturnsHttpError_ShouldReturnCorrectStatusCode(HttpStatusCode errorCode)
    {
      // Arrange
      var mockFile = CreateMockImageFile();
      SetupHttpResponse(errorCode, plainTextResponse: "Error message");

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var statusCodeResult = Assert.IsType<ObjectResult>(result);
      Assert.Equal((int)errorCode, statusCodeResult.StatusCode);
      Assert.Equal(EXPECTED_ERROR_API_FAILURE, statusCodeResult.Value);
    }

    [Fact]
    public async Task GenerateTags_WhenLlmResponseHasEmptyContent_ShouldReturnBadRequest()
    {
      // Arrange
      var mockFile = CreateMockImageFile();
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
      SetupHttpResponse(HttpStatusCode.OK, llmResponse);

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
      Assert.Equal(EXPECTED_ERROR_FAILED_EXTRACTION, badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTags_WhenLlmResponseHasWhitespaceOnlyContent_ShouldReturnBadRequest()
    {
      // Arrange
      var mockFile = CreateMockImageFile();
      var llmResponse = new LlmApiResponse
      {
        Choices = new[]
        {
          new LlmChoice
          {
            Message = new LlmMessage
            {
              Content = "   \t\n   "
            }
          }
        }
      };
      SetupHttpResponse(HttpStatusCode.OK, llmResponse);

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
      Assert.Equal(EXPECTED_ERROR_FAILED_EXTRACTION, badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTags_WhenLlmResponseHasNullChoices_ShouldReturnBadRequest()
    {
      // Arrange
      var mockFile = CreateMockImageFile();
      var llmResponse = new LlmApiResponse
      {
        Choices = null
      };
      SetupHttpResponse(HttpStatusCode.OK, llmResponse);

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
      Assert.Equal(EXPECTED_ERROR_FAILED_EXTRACTION, badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTags_WhenLlmResponseHasEmptyChoicesArray_ShouldReturnBadRequest()
    {
      // Arrange
      var mockFile = CreateMockImageFile();
      var llmResponse = new LlmApiResponse
      {
        Choices = Array.Empty<LlmChoice>()
      };
      SetupHttpResponse(HttpStatusCode.OK, llmResponse);

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
      Assert.Equal(EXPECTED_ERROR_FAILED_EXTRACTION, badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTags_WhenLlmResponseHasNullMessage_ShouldReturnBadRequest()
    {
      // Arrange
      var mockFile = CreateMockImageFile();
      var llmResponse = new LlmApiResponse
      {
        Choices = new[]
        {
          new LlmChoice
          {
            Message = null
          }
        }
      };
      SetupHttpResponse(HttpStatusCode.OK, llmResponse);

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
      Assert.Equal(EXPECTED_ERROR_FAILED_EXTRACTION, badRequestResult.Value);
    }

    [Fact]
    public async Task GenerateTags_WhenJsonIsInvalid_ShouldReturnInternalServerError()
    {
      // Arrange
      var mockFile = CreateMockImageFile();
      var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent("{ invalid json", Encoding.UTF8, "application/json")
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
      Assert.Equal(500, statusCodeResult.StatusCode);
      Assert.Equal(EXPECTED_ERROR_INTERNAL, statusCodeResult.Value);
    }

    #endregion

    #region Tag Processing Tests

    [Fact]
    public async Task GenerateTags_WhenTagsContainWhitespace_ShouldTrimCorrectly()
    {
      // Arrange
      var mockFile = CreateMockImageFile();
      SetupSuccessfulLlmResponse(" nature , landscape,  mountains  , sky ");

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
    }

    [Fact]
    public async Task GenerateTags_WhenTagsContainEmptyValues_ShouldFilterThemOut()
    {
      // Arrange
      var mockFile = CreateMockImageFile();
      SetupSuccessfulLlmResponse("nature,,landscape,   ,mountains,sky,");

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
    }

    #endregion

    #region Integration and Edge Case Tests

    [Fact]
    public async Task GenerateTags_WhenLargeImageProvided_ShouldProcessCorrectly()
    {
      // Arrange
      var largeContent = new string('x', 10000); // 10KB fake image
      var mockFile = CreateMockImageFile("image/png", largeContent);
      SetupSuccessfulLlmResponse("large, image, test");

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var okResult = Assert.IsType<OkObjectResult>(result);
      var response = Assert.IsType<ImageTagsResponse>(okResult.Value);
      Assert.Equal(10000, response.ImageSize);
      Assert.Equal(3, response.Tags.Length);
    }

    [Fact]
    public async Task GenerateTags_WhenHttpClientThrowsException_ShouldReturnInternalServerError()
    {
      // Arrange
      var mockFile = CreateMockImageFile();
      _mockHttpMessageHandler.Protected()
          .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .ThrowsAsync(new HttpRequestException("Network error"));

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);

      // Assert
      var statusCodeResult = Assert.IsType<ObjectResult>(result);
      Assert.Equal(500, statusCodeResult.StatusCode);
      Assert.Equal(EXPECTED_ERROR_INTERNAL, statusCodeResult.Value);
    }

    [Fact]
    public async Task GenerateTags_WhenSuccessful_ShouldHaveRecentTimestamp()
    {
      // Arrange
      var mockFile = CreateMockImageFile();
      var beforeTest = DateTime.UtcNow;
      SetupSuccessfulLlmResponse("test, tag");

      // Act
      var result = await _controller.GenerateTags(mockFile.Object);
      var afterTest = DateTime.UtcNow;

      // Assert
      var okResult = Assert.IsType<OkObjectResult>(result);
      var response = Assert.IsType<ImageTagsResponse>(okResult.Value);
      Assert.True(response.ProcessedAt >= beforeTest && response.ProcessedAt <= afterTest);
    }

    [Fact]
    public async Task GenerateTags_WhenSuccessful_ShouldSendCorrectRequestToLlmApi()
    {
      // Arrange
      var mockFile = CreateMockImageFile();
      var expectedBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(DEFAULT_IMAGE_CONTENT));

      LlmApiRequest? capturedRequest = null;
      _mockHttpMessageHandler.Protected()
          .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>())
          .Callback<HttpRequestMessage, CancellationToken>(async (req, ct) =>
          {
            var requestContent = await req.Content!.ReadAsStringAsync();
            capturedRequest = JsonSerializer.Deserialize<LlmApiRequest>(requestContent, new JsonSerializerOptions
            {
              PropertyNameCaseInsensitive = true
            });
          })
          .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
          {
            Content = new StringContent(JsonSerializer.Serialize(new LlmApiResponse
            {
              Choices = new[]
              {
                new LlmChoice
                {
                  Message = new LlmMessage { Content = "test, tag" }
                }
              }
            }), Encoding.UTF8, "application/json")
          });

      // Act
      await _controller.GenerateTags(mockFile.Object);

      // Assert
      Assert.NotNull(capturedRequest);
      Assert.Equal("gpt-5", capturedRequest.Model);
      Assert.Single(capturedRequest.Messages);

      var message = capturedRequest.Messages[0];
      Assert.Equal("user", message.Role);

      var contentItems = JsonSerializer.Deserialize<LlmContentItem[]>(message.Content!.ToString()!);
      Assert.NotNull(contentItems);
      Assert.Equal(2, contentItems.Length);

      Assert.Equal("text", contentItems[0].Type);
      Assert.Contains("Analyze this image", contentItems[0].Text);

      Assert.Equal("image_url", contentItems[1].Type);
      Assert.Contains($"data:{DEFAULT_CONTENT_TYPE};base64,{expectedBase64}", contentItems[1].ImageUrl!.Url);
    }

    #endregion

    public void Dispose()
    {
      _httpClient?.Dispose();
    }
  }
}
