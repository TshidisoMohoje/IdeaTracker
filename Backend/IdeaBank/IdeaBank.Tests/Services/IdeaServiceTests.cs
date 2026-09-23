
using AutoMapper;
using IdeaBank.Data;
using IdeaBank.Exceptions;
using IdeaBank.Models;
using IdeaBank.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IdeaBank.Tests.Services
{
    public class IdeaServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IdeaService _service;

        public IdeaServiceTests()
        {
            // 1. Setup EF Core In-Memory Database with a unique name per test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            // 2. Setup Real AutoMapper Instance using your Profiles
            var configuration = new MapperConfiguration(cfg =>
            {
                // Replace these with your actual Mapping Profile classes if you have them
                cfg.CreateMap<IdeaUpsertDto, Idea>();
                cfg.CreateMap<Idea, IdeaDto>();
            });

            configuration.AssertConfigurationIsValid(); // Verifies mapping setup
            _mapper = configuration.CreateMapper();

            // 3. Initialize Service
            _service = new IdeaService(_context, _mapper);
        }

        // Cleans up the In-Memory DB after each test execution
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WhenIdeasExist_ReturnsListOfIdeaDtos()
        {
            // Arrange
            var ideas = new List<Idea>
            {
                new() { Id = Guid.NewGuid(), Title = "Idea 1" },
                new() { Id = Guid.NewGuid(), Title = "Idea 2" }
            };
            await _context.Ideas.AddRangeAsync(ideas);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WhenIdeaExists_ReturnsIdeaDto()
        {
            // Arrange
            var ideaId = Guid.NewGuid();
            var idea = new Idea { Id = ideaId, Title = "Target Idea" };
            await _context.Ideas.AddAsync(idea);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetByIdAsync(ideaId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ideaId, result.Id);
            Assert.Equal("Target Idea", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_WhenIdeaDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.GetByIdAsync(nonExistentId));

            Assert.Contains(nonExistentId.ToString(), exception.Message);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidDto_SavesIdeaAndReturnsDto()
        {
            // Arrange
            var dto = new IdeaUpsertDto { Title = "Innovative Idea" };

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("Innovative Idea", result.Title);

            // Verify database state
            var savedIdea = await _context.Ideas.FindAsync(result.Id);
            Assert.NotNull(savedIdea);
            Assert.True((DateTime.UtcNow - savedIdea.CreatedAt).TotalSeconds < 5);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WhenIdeaExists_UpdatesFieldsAndReturnsDto()
        {
            // Arrange
            var ideaId = Guid.NewGuid();
            var existingIdea = new Idea
            {
                Id = ideaId,
                Title = "Old Title",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };
            await _context.Ideas.AddAsync(existingIdea);
            await _context.SaveChangesAsync();

            var updateDto = new IdeaUpsertDto { Title = "Updated Title" };

            // Act
            var result = await _service.UpdateAsync(ideaId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Title", result.Title);

            // Verify db state and timestamp modifications
            var updatedIdea = await _context.Ideas.FindAsync(ideaId);
            Assert.NotNull(updatedIdea);
            Assert.Equal("Updated Title", updatedIdea.Title);
            Assert.True((DateTime.UtcNow - updatedIdea.UpdatedAt).TotalSeconds < 5);
        }

        [Fact]
        public async Task UpdateAsync_WhenIdeaDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateDto = new IdeaUpsertDto { Title = "New Title" };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.UpdateAsync(nonExistentId, updateDto));
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenIdeaExists_RemovesIdeaFromDbAndReturnsTrue()
        {
            // Arrange
            var ideaId = Guid.NewGuid();
            var idea = new Idea { Id = ideaId, Title = "To Be Deleted" };
            await _context.Ideas.AddAsync(idea);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteAsync(ideaId);

            // Assert
            Assert.True(result);

            var deletedIdea = await _context.Ideas.FindAsync(ideaId);
            Assert.Null(deletedIdea);
        }

        [Fact]
        public async Task DeleteAsync_WhenIdeaDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DeleteAsync(nonExistentId));
        }

        #endregion
    }
}
