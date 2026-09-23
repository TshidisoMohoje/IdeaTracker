using AutoMapper;
using IdeaBank.Data;
using IdeaBank.Exceptions;
using IdeaBank.Models;
using Microsoft.EntityFrameworkCore;

namespace IdeaBank.Services
{
    public interface IIdeaService
    {
        Task<IEnumerable<IdeaDto>> GetAllAsync();

        Task<IdeaDto> GetByIdAsync(Guid id);

        Task<IdeaDto> CreateAsync(IdeaUpsertDto dto);

        Task<IdeaDto> UpdateAsync(Guid id, IdeaUpsertDto dto);

        Task<bool> DeleteAsync(Guid id);
    }
}



namespace IdeaBank.Services
{
    public class IdeaService : IIdeaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public IdeaService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<IdeaDto>> GetAllAsync()
        {
            var ideas = await _context.Ideas.ToListAsync();
            return _mapper.Map<IEnumerable<IdeaDto>>(ideas);
        }

        public async Task<IdeaDto> GetByIdAsync(Guid id)
        {
            var idea = await _context.Ideas.FindAsync(id);
            if (idea == null)
                throw new NotFoundException($"Idea with ID {id} was not found.");

            return _mapper.Map<IdeaDto>(idea);
        }

        public async Task<IdeaDto> CreateAsync(IdeaUpsertDto dto)
        {
            var idea = _mapper.Map<Idea>(dto);
            idea.CreatedAt = DateTime.UtcNow;
            idea.UpdatedAt = DateTime.UtcNow;

            _context.Ideas.Add(idea);
            await _context.SaveChangesAsync();

            return _mapper.Map<IdeaDto>(idea);
        }

        public async Task<IdeaDto> UpdateAsync(Guid id, IdeaUpsertDto dto)
        {
            var idea = await _context.Ideas.FindAsync(id);
            if (idea == null)
                throw new NotFoundException($"Idea with ID {id} was not found.");

            _mapper.Map(dto, idea);
            idea.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<IdeaDto>(idea);
        }

        // refactored to throw an exception instead of returning false
        public async Task<bool> DeleteAsync(Guid id)
        {
            var idea = await _context.Ideas.FindAsync(id);
            if (idea == null)
                throw new NotFoundException($"Idea with ID {id} was not found.");

            _context.Ideas.Remove(idea);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
