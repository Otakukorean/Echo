
namespace Identity.Identity.Features.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.RegisterDto).NotNull();
        RuleFor(x => x.RegisterDto.Email).EmailAddress().NotEmpty();
        RuleFor(x => x.RegisterDto.Password).NotEmpty();
        RuleFor(x => x.RegisterDto.DisplayName).NotEmpty();
    }
}