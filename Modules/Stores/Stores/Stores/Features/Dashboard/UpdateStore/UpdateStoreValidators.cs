using FluentValidation;

namespace Stores.Stores.Features.Dashboard.UpdateStore;

public class UpdateStoreValidators : AbstractValidator<UpdateStoreRequest>
{
    public UpdateStoreValidators()
    {
        RuleFor(x => x.UpdateStoreDto).NotNull();
        RuleFor(x => x.UpdateStoreDto.Name).NotEmpty();
        RuleFor(x => x.UpdateStoreDto.Slug).NotEmpty();
        RuleFor(x => x.UpdateStoreDto.Description).NotEmpty();
        RuleFor(x => x.UpdateStoreDto.LogoFileName).NotEmpty();      
    }
}