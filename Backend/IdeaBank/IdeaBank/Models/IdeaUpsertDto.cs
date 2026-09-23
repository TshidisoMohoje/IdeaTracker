using FluentValidation;

namespace IdeaBank.Models
{
    public class IdeaUpsertDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Proposed";
        public List<string> Tags { get; set; } = new();
    }
}

namespace IdeaBank.Models
{
    public class IdeaUpsertDtoValidator : AbstractValidator<IdeaUpsertDto>
    {
        public IdeaUpsertDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.Status).NotEmpty();
            RuleFor(x => x.Tags).NotNull();
        }
    }
}

