using FluentValidation;
using FluentValidation.Results;
using IdeaBank.Controllers;
using IdeaBank.Models;
using IdeaBank.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;


namespace IdeaBank.Tests.Controllers
{
    public class IdeasControllerTests
    {
        private readonly Mock<IIdeaService> _mockIdeaService;
        private readonly Mock<IValidator<IdeaUpsertDto>> _mockValidator;
        private readonly IdeasController _controller;

        public IdeasControllerTests()
        {
            _mockIdeaService = new Mock<IIdeaService>();
            _mockValidator = new Mock<IValidator<IdeaUpsertDto>>();
            _controller = new IdeasController(_mockIdeaService.Object, _mockValidator.Object);
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfIdeas()
        {
            // Arrange
            var mockIdeas = new List<IdeaDto>
            {
                new() { Id = Guid.NewGuid(), Title = "Idea 1" },
                new() { Id = Guid.NewGuid(), Title = "Idea 2" }
            };
            _mockIdeaService.Setup(s => s.GetAllAsync()).ReturnsAsync(mockIdeas);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // CS8600 Fix: Use the null-forgiving operator (!) or cast cleanly if you know it won't be null
            var returnedIdeas = Assert.IsAssignableFrom<IEnumerable<IdeaDto>>(okResult.Value!);
            Assert.Equal(2, returnedIdeas.Count());
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_ReturnsOkResult_WhenIdeaExists()
        {
            // Arrange
            var ideaId = Guid.NewGuid();
            var mockIdea = new IdeaDto { Id = ideaId, Title = "Found Idea" };
            _mockIdeaService.Setup(s => s.GetByIdAsync(ideaId)).ReturnsAsync(mockIdea);

            // Act
            var result = await _controller.GetById(ideaId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // CS8600 Fix: Use the null-forgiving operator (!) because Assert.IsType verifies the type isn't null
            var returnedIdea = Assert.IsType<IdeaDto>(okResult.Value!);
            Assert.Equal(ideaId, returnedIdea.Id);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_ReturnsCreatedAtActionResult_WhenModelIsValid()
        {
            // Arrange
            var dto = new IdeaUpsertDto { Title = "Valid Title", Description = "Valid Desc" };
            var createdIdea = new IdeaDto { Id = Guid.NewGuid(), Title = "Valid Title" };

            _mockValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockIdeaService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(createdIdea);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);

            // CS8602 Fix: Use a safe dictionary check or null-conditional access to avoid compiler warnings
            Assert.NotNull(createdResult.RouteValues);
            Assert.Equal(createdIdea.Id, createdResult.RouteValues["id"]);
            Assert.Equal(createdIdea, createdResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var dto = new IdeaUpsertDto();
            var failures = new List<ValidationFailure> { new("Title", "Title is required") };

            _mockValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            // CS8600 Fix: Appended null-forgiving operator (!) to okResult/badRequestResult values
            var errors = Assert.IsAssignableFrom<IEnumerable<ValidationFailure>>(badRequestResult.Value!);
            Assert.Single(errors);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_ReturnsOkResult_WhenUpdateIsSuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new IdeaUpsertDto { Title = "Updated Title" };
            var updatedIdea = new IdeaDto { Id = id, Title = "Updated Title" };

            _mockValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockIdeaService.Setup(s => s.UpdateAsync(id, dto)).ReturnsAsync(updatedIdea);

            // Act
            var result = await _controller.Update(id, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updatedIdea, okResult.Value);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new IdeaUpsertDto();
            var failures = new List<ValidationFailure> { new("Title", "Title is required") };

            _mockValidator.Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            var result = await _controller.Update(id, dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsAssignableFrom<IEnumerable<ValidationFailure>>(badRequestResult.Value!);
            Assert.Single(errors);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ReturnsNoContentResult_WhenDeleteIsSuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockIdeaService.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockIdeaService.Verify(s => s.DeleteAsync(id), Times.Once);
        }

        #endregion
    }
}
