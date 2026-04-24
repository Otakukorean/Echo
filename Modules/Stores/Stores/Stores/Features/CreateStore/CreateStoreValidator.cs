using FluentValidation;

namespace Stores.Stores.Features.CreateStore;

public class CreateStoreValidator : AbstractValidator<CreateStoreCommand>
{
    public CreateStoreValidator()
    {
        RuleFor(x => x.CreateStoreDto).NotNull();
        RuleFor(x => x.CreateStoreDto.Name).NotEmpty();
        RuleFor(x => x.CreateStoreDto.Slug).NotEmpty()
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$");
        RuleFor(x => x.CreateStoreDto.Description).NotEmpty();
        RuleFor(x => x.CreateStoreDto.LogoUrl).NotEmpty();       
        RuleFor(x => x.CreateStoreDto.CoverUrl);      
    }
}