using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IdeaBank.Exceptions;
using IdeaBank.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IdeaBank.Tests.Infrastructure
{
    public class GlobalExceptionHandlerTests
    {
        private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
        private readonly GlobalExceptionHandler _handler;
        private readonly DefaultHttpContext _httpContext;

        public GlobalExceptionHandlerTests()
        {
            _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
            _handler = new GlobalExceptionHandler(_loggerMock.Object);

            // Set up a fresh HttpContext for each test
            _httpContext = new DefaultHttpContext();
            _httpContext.Request.Path = "/api/test-endpoint";

            // Replace the response body stream with a memory stream so we can read the JSON output
            _httpContext.Response.Body = new MemoryStream();
        }

        [Fact]
        public async Task TryHandleAsync_WhenNotFoundException_Returns404NotFound()
        {
            // Arrange
            var exception = new NotFoundException("The requested item was not found.");

            // Act
            var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal((int)HttpStatusCode.NotFound, _httpContext.Response.StatusCode);

            var problemDetails = await ReadProblemDetailsFromResponseAsync();
            Assert.Equal((int)HttpStatusCode.NotFound, problemDetails.Status);
            Assert.Equal("Resource Not Found", problemDetails.Title);
            Assert.Equal(exception.Message, problemDetails.Detail);
            Assert.Equal("/api/test-endpoint", problemDetails.Instance);
        }

        [Fact]
        public async Task TryHandleAsync_WhenArgumentException_Returns400BadRequest()
        {
            // Arrange
            var exception = new ArgumentException("Invalid arguments provided.");

            // Act
            var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode);

            var problemDetails = await ReadProblemDetailsFromResponseAsync();
            Assert.Equal((int)HttpStatusCode.BadRequest, problemDetails.Status);
            Assert.Equal("Bad Request", problemDetails.Title);
            Assert.Equal(exception.Message, problemDetails.Detail);
        }

        [Fact]
        public async Task TryHandleAsync_WhenUnhandledException_Returns500InternalServerError()
        {
            // Arrange
            var exception = new Exception("Something went completely wrong.");

            // Act
            var result = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode);

            var problemDetails = await ReadProblemDetailsFromResponseAsync();
            Assert.Equal((int)HttpStatusCode.InternalServerError, problemDetails.Status);
            Assert.Equal("Internal Server Error", problemDetails.Title);
            Assert.Equal(exception.Message, problemDetails.Detail);
        }

        [Fact]
        public async Task TryHandleAsync_Always_LogsError()
        {
            // Arrange
            var exception = new Exception("Test logging message");

            // Act
            await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("An unhandled exception occurred")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // Helper method to parse the JSON response body back into a ProblemDetails object
        private async Task<ProblemDetails> ReadProblemDetailsFromResponseAsync()
        {
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(_httpContext.Response.Body);
            var content = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, options);

            return problemDetails ?? throw new InvalidOperationException("Failed to deserialize JSON content to ProblemDetails.");
        }
    }
}
