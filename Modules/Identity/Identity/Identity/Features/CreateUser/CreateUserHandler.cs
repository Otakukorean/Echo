
namespace Identity.Identity.Features.CreateUser;

public record CreateUserCommand(RegisterDto RegisterDto) : ICommand<CreateUserResponse>;

public record CreateUserResponse(bool IsSuccess);




public class CreateUserHandler(IdentityDbContext dbContext) : ICommandHandler<CreateUserCommand , CreateUserResponse>
{
    public async Task<CreateUserResponse> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == command.RegisterDto.Email , cancellationToken);;
        if (user is not null)
        {
            throw new UserAlreadyExist("User Already Exist");
        }
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(command.RegisterDto.Password);
        var userEntity = User.Create(command.RegisterDto.Email , hashedPassword , command.RegisterDto.DisplayName);
        dbContext.Users.Add(userEntity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CreateUserResponse(true);
    }
}