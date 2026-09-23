using FluentValidation;
using IdeaBank.Models;
using IdeaBank.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdeaBank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdeasController : ControllerBase
    {
        private readonly IIdeaService _ideaService;
        private readonly IValidator<IdeaUpsertDto> _validator;

        public IdeasController(IIdeaService ideaService, IValidator<IdeaUpsertDto> validator)
        {
            _ideaService = ideaService;
            _validator = validator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _ideaService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // If not found, GlobalExceptionHandler automatically generates a 404 ProblemDetails response
            var idea = await _ideaService.GetByIdAsync(id);
            return Ok(idea);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] IdeaUpsertDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            var createdIdea = await _ideaService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdIdea.Id }, createdIdea);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] IdeaUpsertDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            var updatedIdea = await _ideaService.UpdateAsync(id, dto);
            return Ok(updatedIdea);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _ideaService.DeleteAsync(id);
            return NoContent();
        }
    }
}
