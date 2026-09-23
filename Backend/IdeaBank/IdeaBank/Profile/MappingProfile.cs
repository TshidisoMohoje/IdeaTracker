using AutoMapper;
using IdeaBank.Models;

namespace IdeaBank.Profile
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {
            CreateMap<Idea, IdeaDto>();
            CreateMap<IdeaUpsertDto, Idea>();
        }
    }
}

